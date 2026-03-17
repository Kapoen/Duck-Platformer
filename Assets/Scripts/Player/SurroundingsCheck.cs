namespace Player
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Handles all of the checks for the player based on its surroundings.
    /// </summary>
    public class SurroundingsCheck : MonoBehaviour
    {
        [Header("Ground Check")]
        [SerializeField]
        private Transform groundCheck;
        [SerializeField]
        private Vector2 groundCheckSize = new Vector2(0.9f, 0.05f);
        private bool _grounded;

        [Header("Wall Check")]
        [SerializeField]
        private Transform wallCheckLeft;
        [SerializeField]
        private Transform wallCheckRight;
        [SerializeField]
        private Vector2 wallCheckSize = new Vector2(0.05f, 0.8f);
        private int _wallDirection;

        [Header("Layers")]
        [SerializeField]
        private LayerMask groundLayer;
        [SerializeField]
        private LayerMask platformLayer;

        private BoxCollider2D _collider;

        /// <summary>
        /// Gets if the player is grounded.
        /// </summary>
        /// <returns>True iff the player is grounded.</returns>
        public bool IsGrounded()
        {
            return this._grounded;
        }

        /// <summary>
        /// Gets the direction of the wall.
        /// </summary>
        /// <returns>The direction of the wall.</returns>
        public int WallDirection()
        {
            return this._wallDirection;
        }

        /// <summary>
        /// Checks if the player is wall sliding, so on a wall and not on the ground.
        /// </summary>
        /// <returns>True iff the player is wall sliding.</returns>
        public bool IsWallSliding()
        {
            return this._wallDirection != 0 && !this._grounded;
        }

        private void Awake()
        {
            this._collider = this.gameObject.GetComponent<BoxCollider2D>();
        }

        private void Update()
        {
            this.GroundedCheck();
            this.WallDirectionCheck();
        }

        /// <summary>
        /// Check if the player is grounded.
        /// </summary>
        private void GroundedCheck()
        {
            ContactFilter2D filter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = this.platformLayer,
            };

            // Stricter for platforms, since you can be inside of those
            List<ContactPoint2D> contacts = new List<ContactPoint2D>();
            this._collider.GetContacts(filter, contacts);
            bool onPlatform = contacts.Any(point => point.otherCollider.bounds.min.y > point.collider.bounds.max.y);

            RaycastHit2D hit = Physics2D.BoxCast(
                this.groundCheck.position,
                this.groundCheckSize,
                0f,
                Vector2.down,
                0.025f,
                this.groundLayer);

            // Check for normal, to avoid clipping collider
            this._grounded = (hit.collider && Mathf.Abs(hit.normal.y) > 0.01f) || onPlatform;
        }

        /// <summary>
        /// Check if the player is on a wall to it's left.
        /// </summary>
        /// <returns>True iff the player is on a wall to it's left.</returns>
        private bool IsOnWallLeft()
        {
            return Physics2D.OverlapBox(this.wallCheckLeft.position, this.wallCheckSize, 0f, this.groundLayer);
        }

        /// <summary>
        /// Check if the player is on a wall to it's right.
        /// </summary>
        /// <returns>True iff the player is on a wall to it's right.</returns>
        private bool IsOnWallRight()
        {
            return Physics2D.OverlapBox(this.wallCheckRight.position, this.wallCheckSize, 0f, this.groundLayer);
        }

        /// <summary>
        /// Check if the player is on a wall, and what the direction of the wall is.
        /// </summary>
        private void WallDirectionCheck()
        {
            if (this.IsOnWallLeft())
            {
                this._wallDirection = -1;
            }
            else if (this.IsOnWallRight())
            {
                this._wallDirection = 1;
            }
            else
            {
                this._wallDirection = 0;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (this.groundCheck != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(this.groundCheck.position, this.groundCheckSize);
            }

            if (this.wallCheckLeft != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(this.wallCheckLeft.position, this.wallCheckSize);
            }

            if (this.wallCheckRight != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(this.wallCheckRight.position, this.wallCheckSize);
            }
        }
    }
}
