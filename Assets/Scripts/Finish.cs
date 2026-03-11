using UnityEngine;

public class Finish : MonoBehaviour
{
    private LevelManager _levelManager;
    
    private void Start()
    {
        _levelManager = LevelManager.Instance;
    }

    /// <summary>
    /// If player enters the collider hitbox, end the level.
    /// </summary>
    /// <param name="other">The <see cref="GameObject"/> that enters the collider hitbox.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _levelManager.LevelComplete();
        }
    }
}
