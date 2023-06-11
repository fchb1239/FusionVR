using System.Collections;
using UnityEngine;

namespace Fusion.VR.Player
{
    public class FusionVRInterpolation : MonoBehaviour
    {
        public Transform target;
        [Tooltip("The higer this it, the smother the player, but the more unsynced it will be")]
        public float interpolationSpeed = 75;

        private void Update()
        {
            if (transform.root.GetComponent<FusionVRPlayer>() != null)
            {
                FusionVRPlayer p = transform.root.GetComponent<FusionVRPlayer>();
                if (!p.Object.HasInputAuthority)
                {
                    transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * interpolationSpeed);
                    transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, Time.deltaTime * interpolationSpeed);
                }
                else
                {
                    transform.position = target.position;
                    transform.rotation = target.rotation;
                }
            }
        }
    }

}