using UnityEngine;
using Fusion;

namespace Fusion.VR.Player
{
    public struct FusionVRNetworkedPlayerData : INetworkInput
    {
        public Vector3 headPosition;
        public Quaternion headRotation;
        public Vector3 leftHandPosition;
        public Quaternion leftHandRotation;
        public Vector3 rightHandPosition;
        public Quaternion rightHandRotation;

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }

        public static bool operator ==(FusionVRNetworkedPlayerData lhs, FusionVRNetworkedPlayerData rhs)
        {
            if (lhs.headPosition != rhs.headPosition)
                return false;

            if (lhs.headRotation != rhs.headRotation)
                return false;

            if (lhs.leftHandPosition != rhs.leftHandPosition)
                return false;

            if (lhs.leftHandRotation != rhs.leftHandRotation)
                return false;

            if (lhs.rightHandPosition != rhs.rightHandPosition)
                return false;

            if (lhs.rightHandRotation != rhs.rightHandRotation)
                return false;


            return true;
        }

        public static bool operator !=(FusionVRNetworkedPlayerData lhs, FusionVRNetworkedPlayerData rhs)
        {
            return !(lhs == rhs);
        }
    }
}