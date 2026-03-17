namespace Level
{
    using UnityEngine;

    /// <summary>
    /// Handle the logic for the finish.
    /// </summary>
    public class Finish : MonoBehaviour
    {
        private LevelManager _levelManager;

        private void Start()
        {
            this._levelManager = LevelManager.Instance;
        }

        /// <summary>
        /// If player enters the collider hitbox, end the level.
        /// </summary>
        /// <param name="other">The <see cref="GameObject"/> that enters the collider hitbox.</param>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                this._levelManager.LevelComplete();
            }
        }
    }
}
