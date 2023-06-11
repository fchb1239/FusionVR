using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Fusion;
using Fusion.VR;
using Fusion.VR.Cosmetics;

using TMPro;

namespace Fusion.VR.Player
{
    public class FusionVRPlayer : NetworkBehaviour
    {
        public static FusionVRPlayer localPlayer;

        public int PlayerId { get; private set; }

        [Header("Objects")]
        public Transform Head;
        public Transform Body;
        public Transform LeftHand;
        public Transform RightHand;

        [Header("Colour Objects")]
        public List<Renderer> renderers = new List<Renderer>();

        [Header("Networked Transforms")]
        public NetworkTransform HeadTransform;
        public NetworkTransform LeftHandTransform;
        public NetworkTransform RightHandTransform;

        [Header("Cosmetics")]
        public List<PlayerCosmeticSlot> cosmeticSlots = new List<PlayerCosmeticSlot>();

        [Header("Other")]
        public TextMeshPro NameText;
        public bool HideLocalName = true;
        public bool HideLocalPlayer = false;

        [Header("Networked Variables")]
        public bool isLocalPlayer;  // Not exactly networked, but you can't make a stupid header without a regular stupid variable
        [Networked(OnChanged = nameof(OnNickNameChanged))]
        public NetworkString<_32> NickName { get; set; } // I feel as if nobody is going to have their name over 32 characters, feel free to change it though
        [Networked(OnChanged = nameof(OnColourChanged))]
        public Color Colour { get; set; }
        [Networked(OnChanged = nameof(OnCosmeticsChanged)), Capacity(10)] // Default is max 10, because beyond that the game would probably start lagging
        public NetworkDictionary<NetworkString<_16>, NetworkString<_32>> Cosmetics => default;

        public override void Spawned()
        {
            if (Object.HasInputAuthority)
            {
                localPlayer = this;
                isLocalPlayer = true;
                FusionVRManager.LoadPlayer();

                NameText.gameObject.SetActive(!HideLocalName);

                Head.gameObject.SetActive(!HideLocalPlayer);
                Body.gameObject.SetActive(!HideLocalPlayer);
                LeftHand.gameObject.SetActive(!HideLocalPlayer);
                RightHand.gameObject.SetActive(!HideLocalPlayer);
            }
        }

        private void Update()
        {
            // Check if this is the local player
            if (Object.HasInputAuthority)
            {
                // Move the objects locally
                // Head
                HeadTransform.transform.position = FusionVRManager.Manager.Head.position;
                HeadTransform.transform.rotation = FusionVRManager.Manager.Head.rotation;
                // Left hand
                LeftHandTransform.transform.position = FusionVRManager.Manager.LeftHand.position;
                LeftHandTransform.transform.rotation = FusionVRManager.Manager.LeftHand.rotation;
                // Right hand
                RightHandTransform.transform.position = FusionVRManager.Manager.RightHand.position;
                RightHandTransform.transform.rotation = FusionVRManager.Manager.RightHand.rotation;
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasInputAuthority)
            {
                if (GetInput(out FusionVRNetworkedPlayerData data))
                {
                    HeadTransform.TeleportToPositionRotation(data.headPosition, data.headRotation);
                    LeftHandTransform.TeleportToPositionRotation(data.leftHandPosition, data.leftHandRotation);
                    RightHandTransform.TeleportToPositionRotation(data.rightHandPosition, data.rightHandRotation);

                    //Debug.Log(data.ToString());
                }
            }
        }

        public static void OnNickNameChanged(Changed<FusionVRPlayer> changed)
        {
            changed.Behaviour.NameText.text = changed.Behaviour.NickName.Value;
            changed.Behaviour.gameObject.name = $"Player ({changed.Behaviour.NickName.Value})";
        }

        public static void OnColourChanged(Changed<FusionVRPlayer> changed)
        {
            List<Renderer> renderers = changed.Behaviour.renderers;
            foreach (Renderer renderer in renderers)
            {
                renderer.material.color = changed.Behaviour.Colour;
            }
        }

        public static void OnCosmeticsChanged(Changed<FusionVRPlayer> changed)
        {
            List<PlayerCosmeticSlot> slots = changed.Behaviour.cosmeticSlots;

            // Foreach, foreach, foreach, foreach!! We love foreach!!
            foreach (KeyValuePair<NetworkString<_16>, NetworkString<_32>> cosmetic in changed.Behaviour.Cosmetics)
            {
                foreach (PlayerCosmeticSlot slot in slots)
                {
                    if (cosmetic.Key == slot.SlotName)
                    {
                        foreach (Transform t in slot.Slot)
                        {
                            GameObject obj = t.gameObject;
                            obj.SetActive(obj.name == cosmetic.Value);

                            if (t.GetComponentInChildren<Collider>() != null)
                            {
                                Debug.LogWarning($"It is not recommended to have a collider on a cosmetic ({obj.name})");
                            }
                        }
                        break;
                    }
                }
            }
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPCSetNickName(string name, RpcInfo info = default)
        {
            NickName = name;
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPCSetColour(Color colour, RpcInfo info = default)
        {
            Colour = colour;
        }

        [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
        public void RPCSetCosmetics(CosmeticSlot[] cosmetics, RpcInfo info = default)
        {
            int i = 0;
            foreach (CosmeticSlot cos in cosmetics)
            {
                if (i < Cosmetics.Capacity)
                {
                    Cosmetics.Set(cos.SlotName, cos.CosmeticName);
                }
            }
        }
    }
}