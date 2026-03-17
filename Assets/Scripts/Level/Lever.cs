namespace Level
{
    using System;
    using System.Collections;
    using UnityEngine;

    /// <summary>
    /// The type of what the lever activates.
    /// </summary>
    public enum LeverType
    {
        /// <summary>
        /// The lever activates a door.
        /// </summary>
        Door,

        /// <summary>
        /// The lever activates a platform.
        /// </summary>
        Platform,
    }

    /// <summary>
    /// Handles the logic of a lever.
    /// </summary>
    public class Lever : MonoBehaviour
    {
        [SerializeField]
        private GameObject linkedObject;
        [SerializeField]
        private LeverType type;
        [SerializeField]
        private float cooldownTime = 0.5f;

        private SpriteRenderer _spriteRenderer;

        private bool _onCooldown;

        /// <summary>
        /// Activate the lever.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Throws if unknown lever type.</exception>
        public void ActivateLever()
        {
            if (this._onCooldown)
            {
                return;
            }

            switch (this.type)
            {
                case LeverType.Door:
                    this.linkedObject.GetComponent<Door>().OpenDoor();
                    break;
                case LeverType.Platform:
                    this.linkedObject.GetComponent<Platform>().SwitchMoving();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            this._spriteRenderer.flipY = !this._spriteRenderer.flipY;

            this._onCooldown = true;
            this.StartCoroutine(this.LeverCooldown());
        }

        /// <summary>
        /// Resets the lever to its original state.
        /// </summary>
        public void ResetLever()
        {
            this.StopAllCoroutines();
            this._onCooldown = false;

            this._spriteRenderer.flipY = false;
        }

        /// <summary>
        /// Wait for the lever cooldown.
        /// </summary>
        /// <returns>...</returns>
        private IEnumerator LeverCooldown()
        {
            yield return new WaitForSeconds(this.cooldownTime);

            this._onCooldown = false;
        }

        private void Start()
        {
            this._spriteRenderer = this.gameObject.GetComponent<SpriteRenderer>();
        }
    }
}
