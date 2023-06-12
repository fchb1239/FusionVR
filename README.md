![FusionVRLogoNoBackSmall](https://github.com/fchb1239/FusionVR/assets/29258204/48221303-cec0-47b9-bc0e-d129bba3dbcc)

A Unity Package containing all the necessary components to do VR networking with [Photon Fusion](https://www.photonengine.com/fusion)

[![Download](https://img.shields.io/badge/Download-blue.svg)](https://github.com/fchb1239/FusionVR/releases/tag/1.0.0)
[![Discord](https://img.shields.io/badge/Discord-blue.svg)](https://discord.gg/rRvnU846Bf)

![FusionPreview1](https://github.com/fchb1239/FusionVR/assets/29258204/dd51f353-cbea-4947-896f-da7525317d4f)

# Documentation
The recommended Unity version is [2021.3.25.1](https://unity.com/releases/editor/whats-new/2021.3.25).

You need the [Photon Fusion SDK](https://doc.photonengine.com/fusion/current/getting-started/sdk-download) and [Photon Voice](https://assetstore.unity.com/packages/tools/audio/photon-voice-2-130518) installed to proceed.

If you are having trouble with Voice, please read [Fusion Voice Intergration Docs](https://doc.photonengine.com/voice/current/getting-started/voice-for-fusion)

Start off by finding the player in the [Resources folder](https://docs.unity3d.com/Manual/BestPracticeUnderstandingPerformanceInUnity6.html), press on it and you'll be met with this dialog.

![image](https://github.com/fchb1239/FusionVR/assets/29258204/23ebf9d5-6833-42b6-b637-5f19e063af91)

Press Import TMP Essentials.

After that navigate to Resources/FusionVR/Prefabs.

![image](https://github.com/fchb1239/FusionVR/assets/29258204/0f1e3eed-0852-48bb-859e-db4b7051fe1d)

Then drag FusionVRManager into your scene. There should only be 1 manager in the entire game. The manager will therefore mark itself your player as "DontDestroyOnLoad".
The manager will automatically attempt to fill out the required fields

![image](https://github.com/fchb1239/FusionVR/assets/29258204/b65946cd-4a84-4203-abd6-67b6ab59eae1)

Including Fusion.VR
```cs
include Fusion.VR
```

Connecting to servers
```
FusionVRManager.Connect();
```

Joining rooms
```cs
// It will only join people on the same queue but the room codes themselves are random
string queue = "Space";
// Optional
int maxPlayers = 100;
FusionVRManager.JoinRandomRoom(queue, maxPlayers);
```

Joining private rooms
```cs
string roomCode = "1234";
// Optional
int maxPlayers = 100;
FusionVRManager.JoinPrivateRoom(roomCode, maxPlayers);
```

Leaving the current room
```cs
FusionVRManager.LeaveRoom();
```

Setting name
```cs
FusionVRManager.SetUsername("fchb1239");
```

Setting colour
```cs
Color myColour = new Color(0, 0, 1);
FusionVRManager.SetColour(myColour);
```

# Cosmetics
Cosmetics in Fusion VR Work differently to [Photon VR](https://github.com/fchb1239/PhotonVR).
In Fusion, cosmetics are stored as a dictionary, with the slot name, and the cosmetic name.

Start off by locating "Cosmetic Slots" on the manager.
![image](https://github.com/fchb1239/FusionVR/assets/29258204/b2a1e88e-fe9e-43bb-a4d6-1b74d5a21506)
Here you can add or remove slots.

Navigate to the player in Resources/FusionVR, here you can add the cosmetic slot parents.
![image](https://github.com/fchb1239/FusionVR/assets/29258204/75f17d20-e278-4d96-b8e8-c6141cc14202)
A cosmetic slot parent, is the parent of all the comsetics in that slot.
Under a parent, you make the cosmetics. Set the GameObject/cosmetic name to something under 32 characters.

Enabling a cosmetic
```cs
string SlotName = "Head";
string CosmeticName = "VRTopHat"; // VRTopHat is one of the 4 default cosmetics
FusionVRManager.SetCosmetics(SlotName, CosmeticName);
```

Enabling a list cosmetic
```cs
Dictionary<string, string> Cosmetics = new Dictionary<string, string>();
Cosmetics.Add("Head", "VRTopHat");
Cosmetics.Add("Face", "VRSunglasses");
FusionVRManager.SetCosmetics(Cosmetics);
```

Small note: Some functions are async, they may cause warnings but don't worry about them. If you have your own async function, you can await them.
Have fun devs!
