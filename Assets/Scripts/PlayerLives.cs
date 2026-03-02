using UnityEngine;

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

    public void Die()
    {
        _spriteManager.OnDeath();
        _levelManager.OnPlayerDeath();
    }
}
