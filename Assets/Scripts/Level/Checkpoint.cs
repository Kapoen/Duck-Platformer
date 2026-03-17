namespace Level
{
    using UnityEngine;

    /// <summary>
    /// Handle checkpoint logic.
    /// </summary>
    public class Checkpoint : MonoBehaviour
    {
        [Header("Checkpoint")]
        [SerializeField]
        private int checkpointNumber;

        private LevelManager _levelManager;
        private SpriteRenderer _spriteRenderer;

        private bool _activated;

        private void Start()
        {
            this._levelManager = LevelManager.Instance;
            this._spriteRenderer = this.GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// If player enters the collider hitbox, active the checkpoint.
        /// </summary>
        /// <param name="other">The <see cref="GameObject"/> that enters the collider hitbox.</param>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (this._activated)
            {
                return;
            }

            if (other.CompareTag("Player"))
            {
                this._levelManager.CheckpointActivated(this.gameObject.transform, this.checkpointNumber);
                this._spriteRenderer.color = Color.darkGoldenRod;
                this._activated = true;
            }
        }
    }
}
