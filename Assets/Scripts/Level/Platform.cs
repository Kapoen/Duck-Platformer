using System;
using System.Collections;
using UnityEngine;

namespace Level {
    public enum PlatformType
    {
        Static,
        OneWay,
        Falling,
        Moving
    }
    public class Platform : MonoBehaviour
    {
        [SerializeField] public PlatformType type = PlatformType.Static;
        private Vector3 _startPosition;
        private Collider2D _collider;

        [Header("General")]
        [SerializeField] private bool leverActivated;
    
        [Header("One Way Platforms")] 
        [SerializeField] private float disableTime = 0.25f;
    
        [Header("Falling Platforms")]
        [SerializeField] private float fallDelay = 1f;
        [SerializeField] private float maxFallSpeed= 12.5f;
        private bool _isFalling;
        private Rigidbody2D _rb;

        [Header("Moving Platforms")]
        [SerializeField] private Transform pointA;
        [SerializeField] private Transform pointB;
        [SerializeField] private float moveSpeed = 2f;
        private Vector3 _nextPosition;
        private bool _isMoving;

        private void Awake()
        {
            _startPosition = transform.position;
            _collider = GetComponent<Collider2D>();
            _rb = GetComponent<Rigidbody2D>();

            if (type == PlatformType.Moving)
            {
                _nextPosition = pointB.position;
            }
        }

        /// <summary>
        /// Checks if the player enters the hitbox from above.
        /// </summary>
        /// <param name="other">The <see cref="GameObject"/> that enters the hitbox.</param>
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!other.gameObject.CompareTag("Player"))
            {
                return;
            }

            other.gameObject.transform.parent = transform;
            
            if (other.GetContact(0).normal.y < -0.5f)
            {
                OnPlayerLanded();
            }
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            if (!other.gameObject.CompareTag("Player"))
            {
                return;
            }

            other.gameObject.transform.parent = null;
        }

        /// <summary>
        /// If the platform should be falling, keep it's falling velocity lower than <see cref="maxFallSpeed"/>.
        /// </summary>
        private void FixedUpdate()
        {
            if (_isFalling)
            {
                _rb.linearVelocityY = Mathf.Max(_rb.linearVelocityY, -maxFallSpeed);
            }
        }

        /// <summary>
        /// Reset the platform to its original position and state.
        /// </summary>
        public void ResetPlatform()
        {
            StopAllCoroutines();
        
            if (_rb)
            {
                _rb.bodyType = RigidbodyType2D.Static;
            }

            _isFalling = false;
            transform.position = _startPosition;
            gameObject.SetActive(true);
            _collider.enabled = true;

            _isMoving = false;
        }
    
        /// <summary>
        /// Handle the player landing on the platform depending on the <see cref="PlatformType"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Throws if unknown platform type.</exception>
        private void OnPlayerLanded()
        {
            if (leverActivated)
            {
                return;
            }
            
            switch (type)
            {
                case PlatformType.Falling:
                    StartFalling();
                    break;
                case PlatformType.Static:
                case PlatformType.OneWay:
                case PlatformType.Moving:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Let the platform fall.
        /// </summary>
        public void StartFalling()
        {
            if (_isFalling)
            {
                return;
            }
                
            StartCoroutine(HandleFallingPlatform());
        }
    
        /// <summary>
        /// Handle the falling platform: <br />
        /// 1. Wait for <see cref="fallDelay"/>. <br />
        /// 2. Allow the platform to fall. <br />
        /// 3. Disable the platform after falling for 2.5 seconds. <br />
        /// </summary>
        /// <returns></returns>
        private IEnumerator HandleFallingPlatform()
        {
            yield return new WaitForSeconds(fallDelay);
        
            _rb.bodyType = RigidbodyType2D.Dynamic;
            _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            _isFalling = true;

            yield return new WaitForSeconds(2.5f);
            gameObject.SetActive(false);
        }
    
        /// <summary>
        /// Allow the player to drop through the platform.
        /// </summary>
        public void DropThrough()
        {
            StartCoroutine(HandleDropThrough());
        }

        /// <summary>
        /// Handle the player dropping through the platform: <br />
        /// 1. Disable the collider. <br />
        /// 2. Wait for <see cref="disableTime"/>. <br />
        /// 3. Enable the collider again. <br />
        /// </summary>
        /// <returns></returns>
        private IEnumerator HandleDropThrough()
        {
            _collider.enabled = false;
        
            yield return new WaitForSeconds(disableTime);

            _collider.enabled = true;
        }

        private void Update()
        {
            if (type != PlatformType.Moving || !_isMoving)
            {
                return;
            }
            
            transform.position = Vector3.MoveTowards(transform.position, _nextPosition, moveSpeed * Time.deltaTime);

            if (transform.position == _nextPosition)
            {
                _nextPosition = (_nextPosition == pointA.position) ? pointB.position : pointA.position;
            }
        }

        /// <summary>
        /// Start or stop the moving platform.
        /// </summary>
        public void SwitchMoving()
        {
            _isMoving = !_isMoving;
        }
    }
}