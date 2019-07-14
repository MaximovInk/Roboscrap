using UnityEngine;

namespace MaximovInk
{
    public class CameraFollow : MonoBehaviour
    {
        public float FollowSpeed = 2f;
        public float Zoffset = 100;
        public Transform camIk;
        public Transform player;
        private Vector3 lastPlayerPos;

        private void Start()
        {
            lastPlayerPos = player.position;
        }

        private void FixedUpdate()
        {
            
                var newPosition = camIk.position;
                newPosition.z = camIk.transform.position.z - Zoffset;
                transform.position += player.position - lastPlayerPos;
                transform.position = Vector3.Slerp(transform.position, newPosition, FollowSpeed * Time.deltaTime);

                lastPlayerPos = player.position;
        }

    }
}