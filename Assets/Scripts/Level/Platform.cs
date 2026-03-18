namespace Level
{
    using System;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// The type of platform.
    /// </summary>
    public enum PlatformType
    {
        /// <summary>
        /// A static platform.
        /// </summary>
        Static,

        /// <summary>
        /// A one way platform, you can fall through it.
        /// </summary>
        OneWay,

        /// <summary>
        /// A falling platform, it falls upon player landing.
        /// </summary>
        Falling,

        /// <summary>
        /// A moving platform, it moves between two points.
        /// </summary>
        Moving,
    }

    /// <summary>
    /// Handles the platform logic.
    /// </summary>
    public class Platform : MonoBehaviour
    {
        private Vector3 _startPosition;
        private Collider2D _collider;

        [Header("General")]
        [SerializeField]
        private PlatformType type = PlatformType.Static;
        [SerializeField]
        private bool leverActivated;

        [Header("One Way Platforms")]
        [SerializeField]
        private float disableTime = 0.25f;

        [Header("Falling Platforms")]
        [SerializeField]
        private float fallDelay = 1f;
        [SerializeField]
        private float fallSpeed = 7.5f;
        private bool _isFalling;
        private Rigidbody2D _rb;

        [Header("Moving Platforms")]
        [SerializeField]
        private Transform pointA;
        [SerializeField]
        private Transform pointB;
        [SerializeField]
        private float moveSpeed = 2f;
        private Vector3 _nextPosition;
        private bool _isMoving;

        /// <summary>
        /// Gets the type of the platform.
        /// </summary>
        public PlatformType Type
        {
            get { return this.type; }
        }

        /// <summary>
        /// Reset the platform to its original position and state.
        /// </summary>
        public void ResetPlatform()
        {
            this.StopAllCoroutines();

            this._isFalling = false;
            this.transform.position = this._startPosition;
            this.gameObject.SetActive(true);
            this._collider.enabled = true;

            this._isMoving = false;
        }

        /// <summary>
        /// Let the platform fall.
        /// </summary>
        public void StartFalling()
        {
            if (this._isFalling)
            {
                return;
            }

            this.StartCoroutine(this.HandleFallingPlatform());
        }

        /// <summary>
        /// Allow the player to drop through the platform.
        /// </summary>
        public void DropThrough()
        {
            this.StartCoroutine(this.HandleDropThrough());
        }

        /// <summary>
        /// Start or stop the moving platform.
        /// </summary>
        public void SwitchMoving()
        {
            this._isMoving = !this._isMoving;
        }

        /// <summary>
        /// Handle the player landing on the platform depending on the <see cref="PlatformType"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Throws if unknown platform type.</exception>
        private void OnPlayerLanded()
        {
            if (this.leverActivated)
            {
                return;
            }

            switch (this.type)
            {
                case PlatformType.Falling:
                    this.StartFalling();
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
        /// Handle the falling platform: <br />
        /// 1. Wait for <see cref="fallDelay"/>. <br />
        /// 2. Set the end destination of the platform and let it fall.
        /// </summary>
        /// <returns>...</returns>
        private IEnumerator HandleFallingPlatform()
        {
            yield return new WaitForSeconds(this.fallDelay);

            this._isFalling = true;
            this._nextPosition = new Vector3(
                this.transform.position.x,
                LevelManager.Instance.GetKillPlaneY() - 1,
                this.transform.position.z);
        }

        /// <summary>
        /// Handle the player dropping through the platform: <br />
        /// 1. Disable the collider. <br />
        /// 2. Wait for <see cref="disableTime"/>. <br />
        /// 3. Enable the collider again. <br />
        /// </summary>
        /// <returns>...</returns>
        private IEnumerator HandleDropThrough()
        {
            this._collider.enabled = false;

            yield return new WaitForSeconds(this.disableTime);

            this._collider.enabled = true;
        }

        private void Update()
        {
            if (this.type == PlatformType.Moving && this._isMoving)
            {
                this.transform.position = Vector3.MoveTowards(
                    this.transform.position,
                    this._nextPosition,
                    this.moveSpeed * Time.deltaTime);

                if (this.transform.position == this._nextPosition)
                {
                    this._nextPosition = (this._nextPosition == this.pointA.position) ? this.pointB.position : this.pointA.position;
                }
            }

            if (this.type == PlatformType.Falling && this._isFalling)
            {
                this.transform.position = Vector3.MoveTowards(
                    this.transform.position,
                    this._nextPosition,
                    this.fallSpeed * Time.deltaTime);

                if (this.transform.position == this._nextPosition)
                {
                    this.gameObject.SetActive(false);
                }
            }
        }

        private void Awake()
        {
            this._startPosition = this.transform.position;
            this._collider = this.GetComponent<Collider2D>();

            if (this.type == PlatformType.Moving)
            {
                this._nextPosition = this.pointB.position;
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

            other.gameObject.transform.parent = this.transform;

            if (other.GetContact(0).normal.y < -0.5f)
            {
                this.OnPlayerLanded();
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
    }
}