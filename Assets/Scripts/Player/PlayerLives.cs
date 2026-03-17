namespace Player
{
    using Level;
    using UnityEngine;

    /// <summary>
    /// Handle the player damage.
    /// </summary>
    public class PlayerLives : MonoBehaviour
    {
        private LevelManager _levelManager;
        private PlayerSpriteManager _spriteManager;

        /// <summary>
        /// Handle the player dying.
        /// </summary>
        public void Die()
        {
            this._spriteManager.OnDeath();
            this._levelManager.OnPlayerDeath();
        }

        private void Start()
        {
            this._levelManager = LevelManager.Instance;
        }

        private void Awake()
        {
            this._spriteManager = this.gameObject.GetComponent<PlayerSpriteManager>();
        }
    }
}
