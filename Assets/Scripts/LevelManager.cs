using System.Collections.Generic;
using UnityEngine;

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

        _currentSpawn = playerSpawn;
        
        _player = GameObject.FindGameObjectWithTag("Player");
        
        _platforms.AddRange(FindObjectsByType<Platform>(FindObjectsInactive.Include, FindObjectsSortMode.None));
        _enemies.AddRange(FindObjectsByType<Enemy>(FindObjectsInactive.Include, FindObjectsSortMode.None));

        _spriteManager = _player.GetComponent<PlayerSpriteManager>();
    }

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

    public void OnPlayerDeath()
    {
        ResetLevel();
    }
    
    public void SetPlayerSpawn(Transform position)
    {
        _currentSpawn = position;
    }
}
