using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;

using Photon.Voice.Fusion;

using Fusion.VR.Player;
using Fusion.VR.Cosmetics;
using Fusion.VR.Saving;

namespace Fusion.VR
{
    public class FusionVRManager : MonoBehaviour
    {
        public static FusionVRManager Manager { get; private set; }

        [Header("Photon")]
        public string FusionAppId;
        public string VoiceAppId;
        [Tooltip("Please read https://doc.photonengine.com/en-us/pun/current/connection-and-authentication/regions for more information.\nLeave this empty to default to the nearest region for the player.")]
        public string Region = "eu";

        [Header("Player")]
        public Transform Head;
        public Transform LeftHand;
        public Transform RightHand;
        public Color Colour = Color.black;
        [Tooltip("If left as nothing, there will be no default username")]
        public string DefaultUsername = "Player";

        [Header("Networking")]
        public string DefaultQueue = "Default";
        public int DefaultRoomLimit = 100; // We love Fusion
        public GameMode NetworkingMode = GameMode.Shared; // P2P by default!!
        public NetworkPrefabRef NetworkedPlayerPrefab;
        public GameObject VoiceAndRunner;

        [Header("Other")]
        public List<string> CosmeticSlots = new List<string>();
        [Tooltip("If the user shall connect when this object has awoken")]
        public bool ConnectOnAwake = true;
        [Tooltip("If the user shall join a room when they connect")]
        public bool JoinRoomOnConnect = true;

        [NonSerialized]
        public NetworkRunner Runner;
        [NonSerialized]
        public FusionVoiceClient VoiceClient;
        [NonSerialized]
        public Dictionary<PlayerRef, NetworkObject> playerCache = new Dictionary<PlayerRef, NetworkObject>();
        [NonSerialized]
        public Dictionary<string, string> Cosmetics = new Dictionary<string, string>();

        public static Action<NetworkRunner> OnHostMigrationResume;

        private void Start()
        {
            if (Manager == null)
                Manager = this;
            else
            {
                Debug.LogError("There can't be multiple PhotonVRManagers in a scene");
                Application.Quit();
            }

            DontDestroyOnLoad(Head.root);
            DontDestroyOnLoad(gameObject);

            if (ConnectOnAwake)
                Connect();

            if (string.IsNullOrEmpty(PlayerPrefs.GetString("Username")) && !string.IsNullOrEmpty(DefaultUsername))
                SetUsername(DefaultUsername + GenerateRoomCode()); // Reduce, reuse, recyle!!

            if (!string.IsNullOrEmpty(PlayerPrefs.GetString("Colour")))
                SetColour(JsonUtility.FromJson<Color>(PlayerPrefs.GetString("Colour")));

            if (!string.IsNullOrEmpty(PlayerPrefs.GetString("Cosmetics")))
                SetCosmetics(FusionVRPrefs.GetCosmetics(CosmeticSlots));

            if (Cosmetics == null)
                SetCosmetics(new Dictionary<string, string>());

            /*
            if (!string.IsNullOrEmpty(PlayerPrefs.GetString("Cosmetics")))
                Cosmetics = JsonUtility.FromJson<PhotonVRCosmeticsData>(PlayerPrefs.GetString("Cosmetics"));
            */
        }

#if UNITY_EDITOR
        public void CheckDefaultValues()
        {
            bool b = CheckForRig(this);
            if (b)
            {
                if (string.IsNullOrEmpty(FusionAppId))
                    FusionAppId = Photon.Realtime.PhotonAppSettings.Instance.AppSettings.AppIdFusion;

                if (string.IsNullOrEmpty(VoiceAppId))
                    VoiceAppId = Photon.Realtime.PhotonAppSettings.Instance.AppSettings.AppIdVoice;

                Debug.Log("Attempted to set default values");
            }
        }

        private bool CheckForRig(FusionVRManager manager)
        {
            GameObject[] objects = FindObjectsOfType<GameObject>();

            bool b = false;

            if (manager.Head == null)
            {
                b = true;
                foreach (GameObject obj in objects)
                {
                    if (obj.name.Contains("Camera") || obj.name.Contains("Head"))
                    {
                        manager.Head = obj.transform;
                        break;
                    }
                }
            }

            if (manager.LeftHand == null)
            {
                b = true;
                foreach (GameObject obj in objects)
                {
                    if (obj.name.Contains("Left") && (obj.name.Contains("Hand") || obj.name.Contains("Controller")))
                    {
                        manager.LeftHand = obj.transform;
                        break;
                    }
                }
            }

            if (manager.RightHand == null)
            {
                b = true;
                foreach (GameObject obj in objects)
                {
                    if (obj.name.Contains("Right") && (obj.name.Contains("Hand") || obj.name.Contains("Controller")))
                    {
                        manager.RightHand = obj.transform;
                        break;
                    }
                }
            }

            return b;
        }
#endif

        /// <summary>
        /// Connects to Photon using the specified AppId and VoiceAppId
        /// </summary>
        public static bool Connect()
        {
            if (Manager.Runner != null)
            {
                Debug.LogError("Already connected to server");
                return false;
            }

            if (string.IsNullOrEmpty(Manager.FusionAppId))
            {
                Debug.LogError("Please input an app id");
                return false;
            }

            GameObject voiceAndRunner = Instantiate(Manager.VoiceAndRunner);

            NetworkProjectConfig.Global.EnableHostMigration = true;
            NetworkProjectConfig.Global.HostMigrationSnapshotInterval = 5;
            Photon.Realtime.PhotonAppSettings.Instance.AppSettings.AppIdFusion = Manager.FusionAppId;
            Photon.Realtime.PhotonAppSettings.Instance.AppSettings.AppIdVoice = Manager.VoiceAppId;

            if (!string.IsNullOrEmpty(Manager.Region))
                Photon.Realtime.PhotonAppSettings.Instance.AppSettings.FixedRegion = Manager.Region;

            //Manager.State = ConnectionState.Connecting;
            Manager.Runner = voiceAndRunner.GetComponent<NetworkRunner>();
            Manager.Runner.ProvideInput = true;

            if (!string.IsNullOrEmpty(Manager.VoiceAppId))
            {
                Manager.VoiceClient = voiceAndRunner.GetComponent<FusionVoiceClient>();
            }

            Debug.Log($"Connected - FusionAppId: {Photon.Realtime.PhotonAppSettings.Instance.AppSettings.AppIdFusion} VoiceAppId: {Photon.Realtime.PhotonAppSettings.Instance.AppSettings.AppIdVoice}");

            if (Manager.JoinRoomOnConnect)
            {
                Debug.Log("Joining room on connect");
                JoinRandomRoom(Manager.DefaultQueue, Manager.DefaultRoomLimit);
            }

            return true;
        }

        /// <summary>
        /// Sets the Fusion nickname to something
        /// </summary>
        /// <param name="Name">The string you want the Fusion nickname to be</param>
        public static void SetUsername(string Name)
        {
            Name = Name.Substring(0, 32);
            if (FusionVRPlayer.localPlayer != null)
            {
                if (FusionVRPlayer.localPlayer.NickName != Name)
                    FusionVRPlayer.localPlayer.RPCSetNickName(Name);
                else
                {
                    // Sleeep
                    Debug.LogWarning("NickName was not set, due to the attempted nickname being the same as the current nickname.\nThis is to save on bandwidth");
                    return;
                }
            }
            PlayerPrefs.SetString("Username", Name);

            Debug.Log($"Set username to {Name}");
        }

        /// <summary>
        /// Sets the colour
        /// </summary>
        /// <param name="PlayerColour">The colour you want the player to be</param>
        public static void SetColour(Color PlayerColour)
        {
            Manager.Colour = PlayerColour;

            if (FusionVRPlayer.localPlayer != null)
            {
                if (FusionVRPlayer.localPlayer.Colour != PlayerColour)
                    FusionVRPlayer.localPlayer.RPCSetColour(PlayerColour);
                else
                {
                    // This is virtually meaningless, but it gives me sleep at night
                    Debug.LogWarning("Colour was not set, due to the attempted colour being the same as the current colour.\nThis is to save on bandwidth");
                    return;
                }
            }

            PlayerPrefs.SetString("Colour", JsonUtility.ToJson(PlayerColour));

            Debug.Log($"Set colour to {JsonUtility.ToJson(PlayerColour)}");
        }

        /// <summary>
        /// Sets the cosmetics
        /// </summary>
        /// <param name="SlotName">The name of the slot you would like to put the cosmetic on</param>
        /// <param name="CosmeticName">The cosmetics you want to set</param>
        public static void SetCosmetics(string SlotName, string CosmeticName)
        {
            Manager.Cosmetics[SlotName] = CosmeticName;

            if (FusionVRPlayer.localPlayer != null)
                FusionVRPlayer.localPlayer.RPCSetCosmetics(CosmeticSlot.CopyFrom(Manager.Cosmetics).ToArray());

            FusionVRPrefs.SaveCosmetics(Manager.Cosmetics);

            Debug.Log("Set cosmetics");
        }

        /// <summary>
        /// Sets the cosmetics
        /// </summary>
        /// <param name="PlayerCosmetics">The cosmetics you want to set</param>
        public static void SetCosmetics(Dictionary<string, string> PlayerCosmetics)
        {
            Manager.Cosmetics = PlayerCosmetics;

            if (FusionVRPlayer.localPlayer != null)
                FusionVRPlayer.localPlayer.RPCSetCosmetics(CosmeticSlot.CopyFrom(Manager.Cosmetics).ToArray());

            FusionVRPrefs.SaveCosmetics(Manager.Cosmetics);

            Debug.Log("Set cosmetics");
        }

        /// <summary>
        /// Checks if the player has a specefic cosmetic on
        /// </summary>
        /// <param name="SlotName">The name of the slot you would like to check</param>
        /// <param name="CosmeticName">The cosmetic you would like to check</param>
        /// <returns>If the player has the specified cosmetic on</returns>
        public static bool HasCosmeticOn(string SlotName, string CosmeticName)
        {
            foreach (KeyValuePair<string, string> slot in Manager.Cosmetics)
            {
                if (slot.Key == SlotName)
                {
                    return slot.Value == CosmeticName;
                }
            }

            return false;
        }

        /// <summary>
        /// Disconnects from the Fusion servers
        /// </summary>
        public static bool Disconnect()
        {
            Manager.Runner.Disconnect(Manager.Runner.LocalPlayer);
            return true;
        }

        #region Join publics
        /// <summary>
        /// Joins a room
        /// </summary>
        public static async Task<bool> JoinRandomRoom(string Queue, int MaxPlayers)
        {
            return await _JoinRandomRoom(Queue, MaxPlayers);
        }

        /// <summary>
        /// Joins a room
        /// </summary>
        public static async Task<bool> JoinRandomRoom(string Queue)
        {
            return await _JoinRandomRoom(Queue, Manager.DefaultRoomLimit);
        }

        public static async Task<bool> _JoinRandomRoom(string queue, int maxPlayers)
        {
            if (Manager.Runner == null)
                Connect();
            else
                Debug.Log($"Runner state: {Manager.Runner.State}");

            Dictionary<string, SessionProperty> roomProperties = new Dictionary<string, SessionProperty>();
            roomProperties.Add("queue", queue);
            roomProperties.Add("version", Application.version); // So we don't join players across different versions

            Manager.Runner.ProvideInput = true;

            StartGameResult result = await Manager.Runner.StartGame(new StartGameArgs()
            {
                GameMode = Manager.NetworkingMode,
                SessionProperties = roomProperties,
                PlayerCount = maxPlayers,
                SceneManager = Manager.gameObject.AddComponent<NetworkSceneManagerDefault>()
            });

            if (!result.Ok)
                Debug.LogError("Failed to join room");
            else
                Debug.Log($"Joined a room");

            return result.Ok;
        }
        #endregion

        #region Join privates

        /// <summary>
        /// Joins a private room
        /// </summary>
        /// <param name="RoomId">The room code</param>
        /// <param name="MaxPlayers">The maximum amount of players that can be in the room</param>
        public static async Task<bool> JoinPrivateRoom(string RoomId, int MaxPlayers)
        {
            return await _JoinPrivateRoom(RoomId, MaxPlayers);
        }

        /// <summary>
        /// Joins a private room
        /// </summary>
        /// <param name="RoomId">The room code</param>
        /// <param name="MaxPlayers">The maximum amount of players that can be in the room</param>
        public static async Task<bool> JoinPrivateRoom(string RoomId)
        {
            return await _JoinPrivateRoom(RoomId, Manager.DefaultRoomLimit);
        }

        public static async Task<bool> _JoinPrivateRoom(string roomCode, int maxPlayers)
        {
            if (Manager.Runner == null)
                Connect();
            else
                Debug.Log($"Runner state: {Manager.Runner.State}");

            Dictionary<string, SessionProperty> roomProperties = new Dictionary<string, SessionProperty>();
            roomProperties.Add("version", Application.version); // So we don't join players across different versions

            Manager.Runner.ProvideInput = true;

            StartGameResult result = await Manager.Runner.StartGame(new StartGameArgs()
            {
                GameMode = Manager.NetworkingMode,
                SessionProperties = roomProperties,
                PlayerCount = maxPlayers,
                SessionName = roomCode,
                SceneManager = Manager.gameObject.AddComponent<NetworkSceneManagerDefault>()
            });

            if (!result.Ok)
                Debug.LogError($"Failed to join room: {result.ShutdownReason}");
            else
                Debug.Log($"Joined {roomCode}");

            return result.Ok;
        }
        #endregion

        public static void LeaveRoom()
        {
            Manager.Runner.Shutdown(shutdownReason: ShutdownReason.Ok);
        }

        /// <summary>
        /// Generates a random room code
        /// </summary>
        /// <returns>A room code</returns>
        public static string GenerateRoomCode()
        {
            return new System.Random().Next(99999).ToString();
        }

        /// <summary>
        /// Loads all saved player values
        /// </summary>
        public static void LoadPlayer()
        {
            Debug.Log("I own player - setting values");
            SetUsername(PlayerPrefs.GetString("Username"));
            SetColour(Manager.Colour);
            SetCosmetics(Manager.Cosmetics);
        }
    }
}