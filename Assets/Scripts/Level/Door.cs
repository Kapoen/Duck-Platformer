namespace Level
{
    using UnityEngine;

    /// <summary>
    /// Handle the logic for a door
    /// </summary>
    public class Door : MonoBehaviour
    {
        [SerializeField]
        private bool openUpwards = true;
        [SerializeField]
        private float openTime = 2f;
        [SerializeField]
        private float doorHeight = 2f;

        private float _movementSpeed;

        private Vector3 _startingPosition;
        private Vector3 _targetPosition;

        private bool _opened;
        private bool _moving;

        /// <summary>
        /// Resets the door to its original position.
        /// </summary>
        public void ResetDoor()
        {
            this._opened = false;
            this._moving = false;

            this.transform.position = this._startingPosition;
        }

        /// <summary>
        /// Open the door, by setting its movement speed.
        /// </summary>
        public void OpenDoor()
        {
            this._moving = true;
        }

        private void Awake()
        {
            this._movementSpeed = this.doorHeight / this.openTime;

            this._startingPosition = this.transform.position;
            this._targetPosition = new Vector3(
                this.transform.position.x,
                this.openUpwards ? this.transform.position.y + this.doorHeight : this.transform.position.y - this.doorHeight,
                this.transform.position.z);
        }

        private void Update()
        {
            if (!this._moving)
            {
                return;
            }

            if ((!this._opened && this.transform.position == this._targetPosition)
                || (this._opened && this.transform.position == this._startingPosition))
            {
                this._opened = !this._opened;
                this._moving = false;
            }

            this.transform.position = Vector3.MoveTowards(
                this.transform.position,
                this._opened ? this._startingPosition : this._targetPosition,
                this._movementSpeed * Time.deltaTime);
        }
    }
}
