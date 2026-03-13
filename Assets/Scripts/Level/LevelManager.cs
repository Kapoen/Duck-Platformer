using Player;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils;

namespace Level
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance;

        private PlayerSpriteManager _spriteManager;
    
        [SerializeField] private Transform playerSpawn;
        [SerializeField] private float killPlaneY;
        private Transform _currentSpawn;
        private int _currentCheckpoint;
    
        private GameObject _player;

        private readonly List<Platform> _platforms = new List<Platform>();
        private readonly List<Enemy> _enemies = new List<Enemy>();
        private readonly List<Lever> _levers = new List<Lever>();
        private readonly List<Door> _doors = new List<Door>();

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
            _levers.AddRange(FindObjectsByType<Lever>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            _doors.AddRange(FindObjectsByType<Door>(FindObjectsInactive.Include, FindObjectsSortMode.None));

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
            
            foreach (Lever lever in _levers)
            {
                lever.ResetLever();
            }
            
            foreach (Door door in _doors)
            {
                door.ResetDoor();
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
        /// <param name="checkpointNumber">The number of the checkpoint.</param>
        public void CheckpointActivated(Transform position, int checkpointNumber)
        {
            if (checkpointNumber > _currentCheckpoint)
            {
                _currentSpawn = position;
                _currentCheckpoint = checkpointNumber;
            }
        }

        /// <summary>
        /// Completes the level and saves it.
        /// </summary>
        public void LevelComplete()
        {
            SaveManager.Instance.CompleteLevel(SceneManager.GetActiveScene().name);
            SceneManager.LoadScene("LevelSelect");
        }

        /// <summary>
        /// Get the y value for the kill plane.
        /// </summary>
        /// <returns></returns>
        public float GetKillPlaneY()
        {
            return killPlaneY;
        }
    }
}
