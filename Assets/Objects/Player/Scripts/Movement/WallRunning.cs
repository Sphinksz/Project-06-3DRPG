using Objects.Player.Scripts.FirstPerson;
using UnityEngine;

namespace Objects.Player.Scripts.Movement
{
    public class WallRunning : MonoBehaviour
    {
        [Header("Wall Running")]
        public LayerMask wallLayer;
        public LayerMask groundLayer;
        public float wallRunForce;
        public float wallJumpUpForce;
        public float wallJumpSideForce;
        public float maxWallRunTime;
        private float _wallRunTimer;
        
        [Header("Input")]
        public KeyCode jumpKey = KeyCode.Space;
        private float _horizontalInput;
        private float _verticalInput;

        [Header("Detection")] 
        public float wallCheckDistance;
        public float minJumpHeight;
        private RaycastHit _leftWallHit;
        private RaycastHit _rightWallHit;
        private bool _wallLeft;
        private bool _wallRight;

        [Header("Exiting")] 
        private bool _exitingWall;
        public float exitWallTime;
        private float _exitWallTimer;

        [Header("Gravity")] 
        public bool useGravity;
        public float gravityCounterForce;
        
        [Header("References")] 
        public Transform orientation;
        public PlayerCamFp cam;
        private PlayerMovement _playerMovement;
        private Rigidbody _rb;

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _playerMovement = GetComponent<PlayerMovement>();
        }

        private void Update()
        {
            CheckForWall();
            StateMachine();
        }

        private void FixedUpdate()
        {
            if (_playerMovement.wallrunning)
                WallRunningMovement();
        }
        
        private void CheckForWall()
        {
            _wallRight = Physics.Raycast(transform.position, orientation.right, out _rightWallHit, wallCheckDistance, wallLayer);
            _wallLeft  = Physics.Raycast(transform.position, -orientation.right, out _leftWallHit, wallCheckDistance, wallLayer);
        }

        private bool AboveGround()
        {
            return !_playerMovement.grounded;
        }

        private void StateMachine()
        {
            _horizontalInput = Input.GetAxisRaw("Horizontal");
            _verticalInput = Input.GetAxisRaw("Vertical");

            if ((_wallLeft || _wallRight) && _verticalInput > 0 && AboveGround() && !_exitingWall)
            {
                if (!_playerMovement.wallrunning)
                    StartWallRunning();
                if (_wallRunTimer > 0)
                    _wallRunTimer -= Time.deltaTime;
                if (_exitWallTimer <= 0 && _playerMovement.wallrunning)
                {
                    _exitingWall = true;
                    _exitWallTimer = exitWallTime;
                }
                if (Input.GetKeyDown(jumpKey)) WallJump();
            }
            else if (_exitingWall)
            {
                if (!_playerMovement.wallrunning)
                    StopWallRunning();
                if (_exitWallTimer > 0)
                    _exitWallTimer -= Time.deltaTime;
                if (_exitWallTimer <= 0)
                    _exitingWall = false;
            }
            else
            {
                if (_playerMovement.wallrunning)
                    StopWallRunning();
            }
        }

        private void StartWallRunning()
        {
            _playerMovement.wallrunning = true;
            _wallRunTimer = maxWallRunTime;
            _rb.velocity = new Vector3(_rb.velocity.x, 0.0f, _rb.velocity.z);
            if (_playerMovement.inFirstPersonCamera)
            {
                cam.DoFov(90.0f);
                if (_wallLeft) cam.DoTilt(-5.0f);
                if (_wallRight) cam.DoTilt(5.0f);
            }
        }

        
        //cam.forward == firstperson
        //orientation.forward == thirdperson
        private void WallRunningMovement()
        {
            _rb.useGravity = useGravity;
            
            var wallNormal = _wallRight ? _rightWallHit.normal : _leftWallHit.normal;
            var wallForward = Vector3.Cross(wallNormal, transform.up);

            if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
                wallForward = -wallForward;
            
            _rb.AddForce(wallForward * wallRunForce, ForceMode.Force);
            
            if (!(_wallLeft && _horizontalInput > 0) && !(_wallRight && _horizontalInput < 0))
                _rb.AddForce(-wallNormal* 100, ForceMode.Force);
            if (useGravity)
                _rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
            if (_playerMovement.inFirstPersonCamera)
            {
                cam.DoFov(90.0f);
                if (_wallLeft) cam.DoTilt(-5.0f);
                if (_wallRight) cam.DoTilt(5.0f);
            }
        }

        private void StopWallRunning()
        {
            _playerMovement.wallrunning = false;
            if (_playerMovement.inFirstPersonCamera)
            {
                cam.DoFov(80.0f);
                cam.DoTilt(0.0f);
            }
        }

        private void WallJump()
        {
            _exitingWall = true;
            _exitWallTimer = exitWallTime;
            var wallNormal = _wallRight ? _rightWallHit.normal : _leftWallHit.normal;
            var forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;
            _rb.velocity = new Vector3(_rb.velocity.x, 0.0f, _rb.velocity.z);
            _rb.AddForce(forceToApply, ForceMode.Impulse);
        }
        
    }
}