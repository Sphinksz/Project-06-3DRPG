using UnityEngine;

namespace Objects.Player.Scripts
{
    public class MoveCamera : MonoBehaviour
    {
        public Transform cameraPosition;

        private void Update()
        {
            transform.position = cameraPosition.position;
        }
    }
}