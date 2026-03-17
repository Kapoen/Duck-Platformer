namespace Level
{
    using Player;
    using UnityEngine;

    /// <summary>
    /// Handle the logic for a hazard.
    /// </summary>
    public class Hazard : MonoBehaviour
    {
        /// <summary>
        /// If the player enters the hitbox of the hazard, kills the player.
        /// </summary>
        /// <param name="other">The <see cref="GameObject"/> that enters the hitbox.</param>
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!other.gameObject.CompareTag("Player"))
            {
                return;
            }

            other.gameObject.GetComponent<PlayerLives>().Die();
        }
    }
}
