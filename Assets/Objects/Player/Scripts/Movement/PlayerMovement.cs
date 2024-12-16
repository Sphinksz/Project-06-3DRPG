using System.Collections;
using Objects.Player.Scripts.FirstPerson;
using Objects.Player.Scripts.ThirdPerson;
using TMPro;
using UnityEngine;

namespace Objects.Player.Scripts.Movement
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")] 
        private float _moveSpeed;
        private float _desiredMoveSpeed;
        private float _lastDesiredMoveSpeed;
        public float walkSpeed;
        public float sprintSpeed;
        public float slideSpeed;
        public float wallRunSpeed;
        public float climbSpeed;
        public float vaultSpeed;
        public float airMinSpeed;
        public float speedIncreaseMultiplier;
        public float slopeIncreaseMultiplier;
        public float groundDrag;
        public Transform orientation;
        float horizontalInput;
        float verticalInput;
        Vector3 _moveDirection;
        
        [Header("Keybinds")]
        public KeyCode jumpKey = KeyCode.Space;
        public KeyCode sprintKey = KeyCode.LeftShift;
        public KeyCode crouchKey = KeyCode.C;
        public KeyCode cameraMode = KeyCode.RightBracket;
        
        [Header("Jumping")] 
        public float jumpForce;
        public float jumpCooldown;
        public float airMultiplier;
        private bool _readyToJump;
        
        [Header("Crouching")] 
        public float crouchSpeed;
        public float crouchYScale;
        private float _startYScale;
        
        [Header("GroundCheck")] 
        public float playerHeight;
        public LayerMask groundLayer;
        public LayerMask wallLayer;
        public bool grounded;
        
        [Header("Slope Handling")]
        public float maxSlopeAngle;
        private RaycastHit _slopeHit;
        private bool _exitingSlope;
        
        [Header("References")]
        public Climbing climbingScript;
        Rigidbody _rb;
        public GameObject thirdPersonCamera;
        public GameObject firstPersonCamera;
        public GameObject playerObject;
        public TextMeshProUGUI currentCamMode;
        public TextMeshProUGUI currentMovementState;
        public TextMeshProUGUI currentOnSlopeState;
        public TextMeshProUGUI currentSpeed;
        
        private PlayerCamFp _firstPersonCamComponent;
        private PlayerCamTp _thirdPersonCamComponent;

        [Header("Camera")] 
        public CameraType currentCamera;
        public bool inThirdPersonCamera;
        public bool inFirstPersonCamera;
        
        [Header("Animation")]
        private bool _hasAnimator;
        private Animator _animator;
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        private int _wallIDRun;
        
        public MovementState state;

        public enum MovementState
        {
            Freeze,
            Unlimited,
            Walking,
            Sprinting,
            Wallrunning,
            Climbing,
            Vaulting,
            Crouching,
            Sliding,
            Air
        }

        public bool sliding;
        public bool crouching;
        public bool wallrunning;
        public bool climbing;
        public bool vaulting;
        public bool freeze;
        public bool unlimited;
        public bool restricted;
        public bool sprinting;

        public CameraType cameraType;
        
        public enum CameraType
        {
            FirstPerson,
            ThirdPerson
        }
        
        private void Start()
        {
            _hasAnimator = GetComponentInChildren<Animator>(true);
            if (_hasAnimator)
            {
                _animator = GetComponentInChildren<Animator>(true);
            }
            _rb = GetComponent<Rigidbody>();
            _firstPersonCamComponent = firstPersonCamera.gameObject.GetComponent<PlayerCamFp>();
            _thirdPersonCamComponent = thirdPersonCamera.gameObject.GetComponent<PlayerCamTp>();
            _rb.freezeRotation = true;
            _readyToJump = true;
            _startYScale = transform.localScale.y;
            SetCameraToFirstPerson();
            AssignAnimationIDs();
        }

        private void Update()
        {
            CheckGrounded();
            PlayerInput();
            SpeedControl();
            StateHandler();
            UpdateDrag(); //Always last
            currentMovementState.text = state.ToString();
            currentCamMode.text = currentCamera.ToString();

        }

        private void CheckGrounded()
        {
            grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight *0.5f + 0.2f, groundLayer) || Physics.Raycast(transform.position, Vector3.down, playerHeight *0.5f + 0.2f, wallLayer);
        }

        private void FixedUpdate()
        {
            MovePlayer();
        }

        private void PlayerInput()
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
            verticalInput = Input.GetAxisRaw("Vertical");

            if (Input.GetKeyUp(cameraMode))
            {
                switch (currentCamera)
                {
                    case CameraType.FirstPerson:
                        SetCameraToThirdPerson();
                        break;
                    case CameraType.ThirdPerson:
                        SetCameraToFirstPerson();
                        break;
                    default:
                        SetCameraToThirdPerson();
                        break;
                }
            }
            
            if (Input.GetKey(jumpKey) && _readyToJump && grounded)
            {
                _readyToJump = false;
                Jump();
                Invoke(nameof(ResetJump), jumpCooldown);
            }

            if (Input.GetKey(crouchKey) && OnSlope())
            {
                sliding = true;
            }
            
            if (Input.GetKeyDown(crouchKey) && grounded && !sliding)
            {
                crouching = true;
                // TODO: Change so it only scales for first person, but uses animator for 3rd person
                transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                _rb.AddForce(Vector3.down * 5.0f, ForceMode.Impulse);
            }

            if (Input.GetKeyUp(crouchKey) && grounded)
            {
                crouching = false;
                // TODO: Change so it only scales for first person, but uses animator for 3rd person
                transform.localScale = new Vector3(transform.localScale.x, _startYScale, transform.localScale.z);
            }
            
            if (Input.GetKeyDown(sprintKey) && grounded)
            {
                sprinting = true;
            }

            if (Input.GetKeyUp(sprintKey) && grounded)
            {
                sprinting = false;
            }
        }

        private void SetCameraToThirdPerson()
        {
            playerObject.SetActive(true);
            currentCamera = CameraType.ThirdPerson;
            inThirdPersonCamera = true;
            inFirstPersonCamera = false;
            firstPersonCamera.SetActive(false);
            thirdPersonCamera.SetActive(true);
        }

        private void SetCameraToFirstPerson()
        {
            playerObject.SetActive(false);
            currentCamera = CameraType.FirstPerson;
            inThirdPersonCamera = false;
            inFirstPersonCamera = true;
            thirdPersonCamera.SetActive(false);
            firstPersonCamera.SetActive(true);
            _thirdPersonCamComponent.combatCamera.gameObject.SetActive(false);
            _thirdPersonCamComponent.thirdPersonCamera.gameObject.SetActive(false);
            _thirdPersonCamComponent.topDownCamera.gameObject.SetActive(false);
        }
        
        private void MovePlayer()
        {
            if (restricted) return;

            if (climbingScript.exitingWall) return;
            
            _moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

            currentOnSlopeState.text = OnSlope().ToString();
            
            if (OnSlope() && !_exitingSlope)
            {
                _rb.AddForce(GetSlopeMoveDirection(_moveDirection) * _moveSpeed * 20.0f, ForceMode.Force);
                if (_rb.velocity.y > 0.0f)
                    _rb.AddForce(Vector3.down * 80.0f, ForceMode.Force);
            }
            
            else if (grounded)
            {
                _rb.AddForce(_moveDirection.normalized * (_moveSpeed * 10.0f), ForceMode.Force);
                

                if (_hasAnimator && inThirdPersonCamera)
                {
                    _animator.SetBool(_animIDGrounded, true);
                    _animator.SetFloat(_animIDSpeed, new Vector3(_rb.velocity.x, 0f, _rb.velocity.z).magnitude);
                    _animator.SetFloat(_animIDMotionSpeed, _rb.velocity.magnitude);
                    _animator.SetBool(_animIDJump, false);
                    
                }
            }

            else if (!grounded)
            {
                _rb.AddForce(_moveDirection.normalized * (_moveSpeed * 10.0f * airMultiplier), ForceMode.Force);
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDGrounded, false);
                }
            }

            if (!wallrunning) _rb.useGravity = !OnSlope();
        }

        private void SpeedControl()
        {
            if (OnSlope())
            {
                if (_rb.velocity.magnitude > _moveSpeed)
                {
                    _rb.velocity = _rb.velocity.normalized * _moveSpeed;
                }
            }
            else
            {
                var flatVel = new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);

                if (flatVel.magnitude > _moveSpeed)
                {
                    var limitedVelocity = flatVel.normalized * _moveSpeed;
                    _rb.velocity = new Vector3(limitedVelocity.x, _rb.velocity.y, limitedVelocity.z);
                }
                currentSpeed.text = flatVel.magnitude.ToString("0.00");
            }
            
        }

        private void UpdateDrag()
        {
            if (grounded)
            {
                _rb.drag = groundDrag;
            }
            else
            {
                _rb.drag = 0;
            }
        }
        
        private void Jump()
        {
            _exitingSlope = true;
            _rb.velocity= new Vector3(_rb.velocity.x, 0f, _rb.velocity.z);
            _rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            if (_hasAnimator && inThirdPersonCamera)
            {
                _animator.SetBool(_animIDGrounded, false);
                _animator.SetBool(_animIDJump, true);
                if (_animator.GetBool(_wallIDRun))
                {
                    _animator.SetBool(_wallIDRun, false);
                    _rb.AddForce(-transform.up * jumpForce, ForceMode.Impulse);
                }
                
            }
        }

        private void ResetJump()
        {
            _exitingSlope = false;
            _readyToJump = true;
        }

        public bool OnSlope()
        {
            if (!Physics.Raycast(transform.position, Vector3.down, out _slopeHit, playerHeight * 0.5f + 0.3f,
                    groundLayer)) return false;
            var angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;

        }

        public Vector3 GetSlopeMoveDirection(Vector3 movementDirection)
        {
            return Vector3.ProjectOnPlane(movementDirection, _slopeHit.normal).normalized;
        }
        
        private void StateHandler()
        {
            if (climbing)
            {
                state = MovementState.Climbing;
                _desiredMoveSpeed = climbSpeed;
            }
            else if (grounded && sprinting)
            {
                state = MovementState.Sprinting;
                _desiredMoveSpeed = sprintSpeed;
            }

            else if (wallrunning)
            {
                state = MovementState.Wallrunning;
                _animator.SetBool(_wallIDRun, true);
                _animator.SetBool(_animIDGrounded, false);
                _desiredMoveSpeed = wallRunSpeed;
            }
            else if (sliding)
            {
                state = MovementState.Sliding;
                if (OnSlope() && _rb.velocity.y < 0.1f)
                    _desiredMoveSpeed = slideSpeed;
                else
                    _desiredMoveSpeed = sprintSpeed;
            }
            else if (crouching)
            {
                state = MovementState.Crouching;
                _desiredMoveSpeed = walkSpeed;
            }
            else if (freeze)
            {
                state = MovementState.Freeze;
                _rb.velocity = Vector3.zero;
            }
            else if (unlimited)
            {
                state = MovementState.Unlimited;
                _desiredMoveSpeed = 999f;
            }
            else if (vaulting)
            {
                state = MovementState.Vaulting;
                _desiredMoveSpeed = vaultSpeed;
            }
            else if (grounded)
            {
                state = MovementState.Walking;
                _desiredMoveSpeed = walkSpeed;
            }
            else
            {
                state = MovementState.Air;
                _animator.SetBool(_wallIDRun, false);
            }

            if (Mathf.Abs(_desiredMoveSpeed - _lastDesiredMoveSpeed) > 4.0f)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                _moveSpeed = _desiredMoveSpeed;
            }
            _lastDesiredMoveSpeed = _desiredMoveSpeed;
        }

        private IEnumerator SmoothlyLerpMoveSpeed()
        {
            var time = 0.0f;
            var difference = Mathf.Abs(_desiredMoveSpeed - _moveSpeed);
            var startValue = _moveSpeed;

            while (time < difference)
            {
                _moveSpeed = Mathf.Lerp(startValue, _desiredMoveSpeed, time/difference);

                if (OnSlope())
                {
                    var slopeAngle = Vector3.Angle(Vector3.up, _slopeHit.normal);
                    float slopeAngleIncrease = 1.0f + (slopeAngle / 90.0f);
                    
                    time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
                }
                else
                {
                    time += Time.deltaTime * speedIncreaseMultiplier;
                }
                yield return null;
            }
            _moveSpeed = _desiredMoveSpeed;
        }
        
        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _wallIDRun = Animator.StringToHash("Wallrun");
        }
        
    }
}