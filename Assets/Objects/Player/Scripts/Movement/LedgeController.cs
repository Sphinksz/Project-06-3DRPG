using UnityEngine;

namespace Objects.Player.Scripts.Movement
{
    public class LedgeController : MonoBehaviour
    {
        [Header("References")] 
        public PlayerMovement Pmc;
        public Transform orientation;
        public Transform cam;
        private Rigidbody _rb;

        [Header("Ledge Grabbing")] 
        public float moveToLedgeSpeed;
        public float maxLedgeGrabDistance;
        public float minTimeOnLedge;
        private float _timeOnLedge;
        public bool holdingOnLedge;
        
        [Header("Ledge Jumping")]
        public KeyCode jumpkey = KeyCode.Space;
        public float ledgeJumpForwardForce;
        public float ledgeJumpUpwardForce;
        
        [Header("Ledge Detection")] 
        public float ledgeDetectionLength;
        public float ledgeSphereCastRadius;
        public LayerMask ledgeDetectionLayers;
        private Transform _lastLedge;
        private Transform _currentLedge;
        private RaycastHit _ledgeHit;

        [Header("Exiting")] 
        public bool exitingLedge;
        public float exitingLedgeTime;
        private float _exitLedgeTimer;

        [Header("CameraInfo")] 
        private bool _inThirdPerson;
        private Vector3 _directionToUse;
        
        private void Update()
        {
            SetCameraDirection();
            LedgeDetection();
            SubStateMachine();
        }

        private void SetCameraDirection()
        {
            _inThirdPerson = Pmc.inThirdPersonCamera;
            _directionToUse = _inThirdPerson ? orientation.forward : cam.forward;
            
        }

        private void SubStateMachine()
        {
            var horizontal = Input.GetAxisRaw("Horizontal");
            var vertical = Input.GetAxisRaw("Vertical");
            var anyInputKeyPressed = horizontal != 0 || vertical != 0;
            
            //Holding ledge
            if (holdingOnLedge)
            {
                FreezeRigidbodyOnLedge();
                _timeOnLedge += Time.deltaTime;
                if (_timeOnLedge > minTimeOnLedge && anyInputKeyPressed) ExitLedgeHold();
                if (Input.GetKeyDown(jumpkey)) LedgeJump();
            }
            else if (exitingLedge)
            {
                if (_exitLedgeTimer >= 0.0f) _exitLedgeTimer -= Time.deltaTime;
                else exitingLedge = false;
            }
        }
        
        private void LedgeDetection()
        {
            var ledgeDetected = Physics.SphereCast(transform.position, ledgeSphereCastRadius, _directionToUse, out _ledgeHit, ledgeDetectionLength, ledgeDetectionLayers);
            if (!ledgeDetected) return;
            var distanceToLedge = Vector3.Distance(transform.position, _ledgeHit.transform.position);
            if (_ledgeHit.transform == _lastLedge) return;
            if (distanceToLedge < maxLedgeGrabDistance && !holdingOnLedge) EnterLedgeHold();
        }

        private void LedgeJump()
        {
            ExitLedgeHold();
            Invoke(nameof(DelayedJumpForce), 0.05f);

        }

        private void DelayedJumpForce()
        {
            var forceToAdd = _directionToUse * ledgeJumpForwardForce + orientation.up * ledgeJumpUpwardForce;
            _rb.velocity = Vector3.zero;
            _rb.AddForce(forceToAdd, ForceMode.Impulse);
        }
        
        private void EnterLedgeHold()
        {
            holdingOnLedge = true;
            Pmc.unlimited = true;
            Pmc.restricted = true;
            _currentLedge = _ledgeHit.transform;
            _lastLedge = _ledgeHit.transform;
            
            _rb.useGravity = false;
            _rb.velocity = Vector3.zero;
        }

        private void FreezeRigidbodyOnLedge()
        {
            _rb.useGravity = false;
            var directionToLedge = _currentLedge.position - transform.position;
            var distanceToLedge = Vector3.Distance(transform.position, _currentLedge.position);
            if (distanceToLedge > 1.0f)
            {
                if (_rb.velocity.magnitude < moveToLedgeSpeed)
                {
                    _rb.AddForce(directionToLedge.normalized * (moveToLedgeSpeed * 1000f * Time.deltaTime));
                }
            }
            else
            {
                if (!Pmc.freeze) Pmc.freeze = true;
                if (Pmc.unlimited) Pmc.unlimited = false;
            }
            if (distanceToLedge > maxLedgeGrabDistance) ExitLedgeHold();
        }

        private void ExitLedgeHold()
        {
            holdingOnLedge = false;
            _exitLedgeTimer = exitingLedgeTime;
            _timeOnLedge = 0.0f;
            
            Pmc.restricted = false;
            Pmc.freeze = false;
            
            _rb.useGravity = true;
            
            StopAllCoroutines();
            Invoke(nameof(ResetLastLedge), 1f);
        }

        private void ResetLastLedge()
        {
            _lastLedge = null;
        }

    }
}