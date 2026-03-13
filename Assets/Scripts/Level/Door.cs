using UnityEngine;

namespace Level
{
    public class Door : MonoBehaviour
    {
        [SerializeField] private bool openUpwards = true;
        [SerializeField] private float openTime = 2f;
        [SerializeField] private float doorHeight = 2f;

        private float _movementSpeed;

        private Vector3 _startingPosition;
        private Vector3 _targetPosition;
        
        private bool _opened;
        private bool _moving;

        private void Awake()
        {
            _movementSpeed = doorHeight / openTime;
            
            _startingPosition = transform.position;
            _targetPosition = new Vector3(
                transform.position.x, 
                openUpwards ? transform.position.y + doorHeight : transform.position.y - doorHeight, 
                transform.position.z
            );
        }

        /// <summary>
        /// Open the door, by setting its movement speed.
        /// </summary>
        public void OpenDoor()
        {
            _moving = true;
        }

        private void Update()
        {
            if (!_moving)
            {
                return;
            }

            if ((!_opened && transform.position == _targetPosition) 
                || (_opened && transform.position == _startingPosition))
            {
                _opened = !_opened;
                _moving = false;
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                _opened ? _startingPosition : _targetPosition,
                _movementSpeed * Time.deltaTime
            );
        }

        /// <summary>
        /// Resets the door to its original position.
        /// </summary>
        public void ResetDoor()
        {
            _opened = false;
            _moving = false;

            transform.position = _startingPosition;
        }
    }
}
