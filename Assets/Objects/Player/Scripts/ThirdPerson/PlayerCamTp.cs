using System;
using System.Collections.Generic;
using UnityEngine;

namespace Objects.Player.Scripts.ThirdPerson
{
    public class PlayerCamTp : MonoBehaviour
    {
        [Header("References")] 
        public Transform orientation;
        public Transform player;
        public Transform playerObj;
        public Rigidbody rb;
        public GameObject thirdPersonCamera;
        public GameObject combatCamera;
        public GameObject topDownCamera;
        
        [Header("Variables ")]
        public float rotationSpeed;
        public Transform combatLookAt;
        public CameraStyle currentStyle;
        
        [Header("Keybinds")]
        public KeyCode switchCamera = KeyCode.BackQuote;

        public enum CameraStyle
        {
            Basic,
            Combat,
            Topdown
        }

        private void OnEnable()
        {
            SwitchCameraStyle(CameraStyle.Basic);
            currentStyle = CameraStyle.Basic;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        private void Update()
        {
            CheckForCameraSwitch();
            
            var viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
            orientation.forward = viewDir.normalized;

            if (currentStyle is CameraStyle.Basic or CameraStyle.Topdown)
            {
                var horizontalInput = Input.GetAxis("Horizontal");
                var verticalInput = Input.GetAxis("Vertical");
                var inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

                if (inputDir != Vector3.zero)
                    playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
            }
            
            else if (currentStyle == CameraStyle.Combat)
            {
                var dirToCombatLookAt = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
                orientation.forward = dirToCombatLookAt.normalized;
                playerObj.forward = dirToCombatLookAt.normalized;
                
            }

        }

        private void CheckForCameraSwitch()
        {
            if (Input.GetKeyDown(switchCamera))
            {
                switch (currentStyle)
                {
                    case CameraStyle.Basic:
                        SwitchCameraStyle(CameraStyle.Combat);
                        break;
                    case CameraStyle.Combat:
                        SwitchCameraStyle(CameraStyle.Topdown);
                        break;
                    case CameraStyle.Topdown:
                        SwitchCameraStyle(CameraStyle.Basic);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        private void SwitchCameraStyle(CameraStyle newStyle)
        {
            combatCamera.SetActive(false);
            topDownCamera.SetActive(false);
            thirdPersonCamera.SetActive(false);
            
            switch (newStyle)
            {
                case CameraStyle.Basic:
                    thirdPersonCamera.SetActive(true);
                    break;
                case CameraStyle.Combat:
                    combatCamera.SetActive(true);
                    break;
                case CameraStyle.Topdown:
                    topDownCamera.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(newStyle), newStyle, null);
            }
            
            currentStyle = newStyle;
        }
        
    }
}