using Level;
using UnityEngine;

namespace Player
{
    public class PlayerLives : MonoBehaviour
    {
        private LevelManager _levelManager;
        private PlayerSpriteManager _spriteManager;
    
        private void Start()
        {
            _levelManager = LevelManager.Instance;
        }

        private void Awake()
        {
            _spriteManager = gameObject.GetComponent<PlayerSpriteManager>();
        }

        /// <summary>
        /// Handle the player dying.
        /// </summary>
        public void Die()
        {
            _spriteManager.OnDeath();
            _levelManager.OnPlayerDeath();
        }
    }
}
