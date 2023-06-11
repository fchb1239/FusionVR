using UnityEngine;

namespace Fusion.VR.Player
{
    public class FusionVRPlayerBody : MonoBehaviour
    {
        public Transform Head;
        public float Offset;

        private void Update()
        {
            transform.rotation = new Quaternion(0, Head.rotation.y, 0, Head.rotation.w);
            transform.position = new Vector3(Head.position.x, Head.position.y + Offset, Head.position.z);
        }
    }
}