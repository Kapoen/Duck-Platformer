using System;
using System.Collections;
using UnityEngine;

namespace Level
{
    public enum LeverType
    {
        Door,
        Platform
    }
    
    public class Lever : MonoBehaviour
    {
        [SerializeField] private GameObject linkedObject;
        [SerializeField] private LeverType type;
        [SerializeField] private float cooldownTime = 0.5f;

        private SpriteRenderer _spriteRenderer;

        private bool _onCooldown;

        private void Start()
        {
            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// Activate the lever.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Throws if unkown lever type.</exception>
        public void ActivateLever()
        {
            if (_onCooldown)
            {
                return;
            }
            
            switch (type)
            {
                case LeverType.Door:
                    linkedObject.GetComponent<Door>().OpenDoor();
                    break;
                case LeverType.Platform:
                    linkedObject.GetComponent<Platform>().SwitchMoving();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _spriteRenderer.flipY = !_spriteRenderer.flipY;

            _onCooldown = true;
            StartCoroutine(LeverCooldown());
        }

        /// <summary>
        /// Wait for the lever cooldown.
        /// </summary>
        /// <returns></returns>
        private IEnumerator LeverCooldown()
        {
            yield return new WaitForSeconds(cooldownTime);

            _onCooldown = false;
        }

        /// <summary>
        /// Resets the lever to its original state.
        /// </summary>
        public void ResetLever()
        {
            StopAllCoroutines();
            _onCooldown = false;
            
            _spriteRenderer.flipY = false;
        }
    }
}
