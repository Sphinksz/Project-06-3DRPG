using UnityEngine;
using DG.Tweening;

namespace Objects.Player.Scripts.FirstPerson
{
    public class PlayerCamFp : MonoBehaviour
    {
        [Header("References")]
        public Transform Orientation;
        public Transform camHolder;
        public Camera cam;
        
        [Header("Variables")] 
        public float SensitivityX;
        public float SensitivityY;
        float xRotation;
        float yRotation;
        public float MinVerticalLookAngle = -90f;
        public float MaxVerticalLookAngle = 90f;
        float minHorizontalLookAngle;
        float maxHorizontalLookAngle;


        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            cam = GetComponent<Camera>();
        }

        private void Update()
        {
            var mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * SensitivityX;
            var mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * SensitivityY;
            yRotation += mouseX;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, MinVerticalLookAngle, MaxVerticalLookAngle);
            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
            camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0f);
            Orientation.rotation = Quaternion.Euler(0f, yRotation, 0f);
            
        }

        public void DoFov(float endVal)
        {
            cam.DOFieldOfView(endVal, 0.25f);
        }

        public void DoTilt(float zTilt)
        {
            transform.DOLocalRotate(new Vector3(0,0,zTilt), 0.25f);
        }
        
    }
}