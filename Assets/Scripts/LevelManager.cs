using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    private PlayerSpriteManager _spriteManager;
    
    [SerializeField] private Transform playerSpawn;
    private Transform _currentSpawn;
    
    private GameObject _player;

    private readonly List<Platform> _platforms = new List<Platform>();
    private readonly List<Enemy> _enemies = new List<Enemy>();

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        _player = GameObject.FindGameObjectWithTag("Player");
        
        _currentSpawn = playerSpawn;
        _player.transform.position = playerSpawn.position;
        
        _platforms.AddRange(FindObjectsByType<Platform>(FindObjectsInactive.Include, FindObjectsSortMode.None));
        _enemies.AddRange(FindObjectsByType<Enemy>(FindObjectsInactive.Include, FindObjectsSortMode.None));

        _spriteManager = _player.GetComponent<PlayerSpriteManager>();
    }

    /// <summary>
    /// Resets everything in the level to its original state.
    /// </summary>
    private void ResetLevel()
    {
        foreach (Platform platform in _platforms)
        {
            platform.ResetPlatform();
        }
        
        foreach (Enemy enemy in _enemies)
        {
            enemy.ResetEnemy();
        }

        // TODO: Add cooldown for respawning
        _spriteManager.OnRespawn();
        
        _player.transform.position = _currentSpawn.position;
        _player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
    }

    /// <summary>
    /// Trigger for if the player dies.
    /// </summary>
    public void OnPlayerDeath()
    {
        ResetLevel();
    }
    
    /// <summary>
    /// Updates the spawn position of the player.
    /// </summary>
    /// <param name="position">The new spawn position of the player.</param>
    public void SetPlayerSpawn(Transform position)
    {
        _currentSpawn = position;
    }

    /// <summary>
    /// Completes the level and saves it.
    /// </summary>
    public void LevelComplete()
    {
        SaveManager.Instance.CompleteLevel(SceneManager.GetActiveScene().name);
        SceneManager.LoadScene("LevelSelect");
    }
}
