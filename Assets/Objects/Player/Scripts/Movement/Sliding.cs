using UnityEngine;

namespace Objects.Player.Scripts.Movement
{
    public class Sliding : MonoBehaviour
    {
        [Header("References")] 
        public Transform orientation;
        public Transform playerObject;
        private Rigidbody _rb;
        private PlayerMovement _playerMovement;

        [Header("Sliding")] 
        public float maxSlideTime;
        public float slideForce;
        private float _slideTimer;
        public float slideYScale;
        private float _startYScale;
        
        [Header("Input")]
        public KeyCode slideKey = KeyCode.C;
        private float _horizontalInput;
        private float _verticalInput;

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            _playerMovement = GetComponent<PlayerMovement>();

            _startYScale = playerObject.localScale.y;
        }

        private void Update()
        {
            _verticalInput = Input.GetAxisRaw("Vertical");
            _horizontalInput = Input.GetAxisRaw("Horizontal");

            if (Input.GetKeyDown(slideKey) && _playerMovement.sprinting) 
                StartSliding();
            if (Input.GetKeyUp(slideKey) && _playerMovement.sliding)
                StopSliding();
        }

        private void FixedUpdate()
        {
            if (_playerMovement.sliding)
            {
                SlidingMovement();
            }
        }
        
        private void StartSliding()
        {
            _playerMovement.sliding = true;
            playerObject.localScale = new Vector3(playerObject.localScale.x, slideYScale, playerObject.localScale.z);
            _rb.AddForce(Vector3.down * 5.0f, ForceMode.Impulse);
            _slideTimer = maxSlideTime;
        }

        private void SlidingMovement()
        {
            var inputDirection = orientation.forward * _verticalInput + orientation.right * _horizontalInput;
            
            if (!_playerMovement.OnSlope() || _rb.velocity.y > -0.1f)
            {
                
                _rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);
                _slideTimer -= Time.deltaTime;
            }
            else
            {
                _rb.AddForce(_playerMovement.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
            }
            
            if (_slideTimer <= 0)
                StopSliding();
        }

        private void StopSliding()
        {
            _playerMovement.sliding = false;
            playerObject.localScale = new Vector3(playerObject.localScale.x, _startYScale, playerObject.localScale.z);
        }
        
    }
}