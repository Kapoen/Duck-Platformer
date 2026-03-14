using UnityEngine;

namespace Level
{
    public class Checkpoint : MonoBehaviour
    {
        [Header("Checkpoint")]
        [SerializeField] private int checkpointNumber;
    
        private LevelManager _levelManager;
        private SpriteRenderer _spriteRenderer;

        private bool _activated;
    
        private void Start()
        {
            _levelManager = LevelManager.Instance;
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// If player enters the collider hitbox, active the checkpoint.
        /// </summary>
        /// <param name="other">The <see cref="GameObject"/> that enters the collider hitbox.</param>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_activated)
            {
                return;
            }
        
            if (other.CompareTag("Player"))
            {
                _levelManager.CheckpointActivated(gameObject.transform, checkpointNumber);
                _spriteRenderer.color = Color.darkGoldenRod;
                _activated = true;
            }
        }
    }
}
