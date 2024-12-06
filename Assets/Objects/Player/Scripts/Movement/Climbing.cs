using RotaryHeart.Lib.PhysicsExtension;
using UnityEngine;
using Physics = UnityEngine.Physics;

namespace Objects.Player.Scripts.Movement
{
    public class Climbing : MonoBehaviour
    {
        [Header("References")] 
        public Transform orientation;
        public Rigidbody rb;
        public PlayerMovement pm;
        public LayerMask wallLayerMask;

        [Header("Climbing")] 
        public float climbSpeed;
        public float maxClimbTime;
        private float _climbTimer;
        private bool _isClimbing;

        [Header("ClimbJumping")] 
        public float climbJumpUpForce;
        public float climbJumpBackForce;
        public KeyCode jumpKey = KeyCode.Space;
        public int climbJumpCount;
        public int climbJumpsLeft;
        
        [Header("Detection")]
        public float detectionLength;
        public float sphereCastRadius;
        public float maxWallLookAngle;
        private float _wallLookAngle;
        private RaycastHit _frontWallHit;
        private bool _wallFront;
        private Transform _lastWall;
        private Vector3 _lastWallNormal;
        public float minWallNormalAngleChange;

        [Header("Exiting")] 
        public bool exitingWall;
        public float exitingWallTime;
        private float _exitWallTimer;

        private void Update()
        {
            WallCheck();
            StateMachine();
            
            if(_isClimbing && !exitingWall) ClimbingMovement();
        }

        private void StateMachine()
        {
            if (_wallFront && Input.GetKey(KeyCode.W) && _wallLookAngle < maxWallLookAngle && !exitingWall)
            {
                if (!_isClimbing && _climbTimer > 0) StartClimbing();
                if (_climbTimer > 0) _climbTimer -= Time.deltaTime;
                if (_climbTimer <0) StopClimbing();
            }
            else if (exitingWall)
            {
                if (_isClimbing) StopClimbing();
                if (_exitWallTimer > 0) _exitWallTimer -= Time.deltaTime;
                if (_exitWallTimer < 0) exitingWall = false;
            }
            else
            {
                if (_isClimbing) StopClimbing();
            }
            if (_wallFront && Input.GetKeyDown(jumpKey) && climbJumpCount > 0) ClimbJump();
            
        }
        
        private void WallCheck()
        {
            //_wallFront = Physics.SphereCast(transform.position, sphereCastRadius, orientation.forward, out _frontWallHit, detectionLength, wallLayerMask);
            //Debug.Log("Wall Check: " + _frontWallHit.collider.gameObject.name);
            _wallFront = RotaryHeart.Lib.PhysicsExtension.Physics.SphereCast(transform.position, sphereCastRadius,
                orientation.forward, out _frontWallHit, detectionLength, wallLayerMask,
                PreviewCondition.Editor, 2.0f, Color.red, Color.green);
            _wallLookAngle = Vector3.Angle(orientation.forward, -_frontWallHit.normal);
            var newWall = _frontWallHit.transform != _lastWall || Mathf.Abs(Vector3.Angle(_lastWallNormal, _frontWallHit.normal)) > minWallNormalAngleChange;
            
            if ((_wallFront && newWall) || pm.grounded)
            {
                _climbTimer = maxClimbTime;
                climbJumpsLeft = climbJumpCount;
            }
        }

        private void StartClimbing()
        {
            _isClimbing = true;
            pm.climbing = true;
            _lastWall = _frontWallHit.transform;
            _lastWallNormal = _frontWallHit.normal;
        }

        private void ClimbingMovement()
        {
            rb.velocity = new Vector3(rb.velocity.x, climbSpeed, rb.velocity.z);
        }

        private void StopClimbing()
        {
            _isClimbing = false;
            pm.climbing = false;
        }

        private void ClimbJump()
        {
            exitingWall = true;
            _exitWallTimer = exitingWallTime;
            var forceToApply = transform.up * climbJumpUpForce + _frontWallHit.normal * climbJumpBackForce;
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(forceToApply, ForceMode.Impulse);
            climbJumpsLeft--;
        }
    }
}