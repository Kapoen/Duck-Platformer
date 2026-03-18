namespace Level
{
    using System.Collections.Generic;
    using Player;
    using Unity.Cinemachine;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using Utils;

    /// <summary>
    /// Handle the general logic of a level.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        private readonly List<Platform> _platforms = new List<Platform>();
        private readonly List<Enemy> _enemies = new List<Enemy>();
        private readonly List<Lever> _levers = new List<Lever>();
        private readonly List<Door> _doors = new List<Door>();
        private CrackedWall _crackedWall;

        private PlayerSpriteManager _spriteManager;

        [SerializeField]
        private CinemachineCamera cineCamera;
        [SerializeField]
        private Transform playerSpawn;
        [SerializeField]
        private float killPlaneY;
        private Transform _currentSpawn;
        private int _currentCheckpoint;

        private GameObject _player;

        /// <summary>
        /// Gets the level manager instance.
        /// </summary>
        public static LevelManager Instance { get; private set; }

        /// <summary>
        /// Trigger for if the player dies.
        /// </summary>
        public void OnPlayerDeath()
        {
            this.ResetLevel();
        }

        /// <summary>
        /// Updates the spawn position of the player.
        /// </summary>
        /// <param name="position">The new spawn position of the player.</param>
        /// <param name="checkpointNumber">The number of the checkpoint.</param>
        public void CheckpointActivated(Transform position, int checkpointNumber)
        {
            if (checkpointNumber > this._currentCheckpoint)
            {
                this._currentSpawn = position;
                this._currentCheckpoint = checkpointNumber;
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
        /// <returns>The y value of the kill plane.</returns>
        public float GetKillPlaneY()
        {
            return this.killPlaneY;
        }

        private void Awake()
        {
            if (!Instance)
            {
                Instance = this;
            }
            else
            {
                Destroy(this.gameObject);
            }

            this._player = GameObject.FindGameObjectWithTag("Player");

            this._currentSpawn = this.playerSpawn;
            this._player.transform.position = this.playerSpawn.position;

            this._platforms.AddRange(FindObjectsByType<Platform>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            this._enemies.AddRange(FindObjectsByType<Enemy>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            this._levers.AddRange(FindObjectsByType<Lever>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            this._doors.AddRange(FindObjectsByType<Door>(FindObjectsInactive.Include, FindObjectsSortMode.None));
            this._crackedWall = FindAnyObjectByType<CrackedWall>(FindObjectsInactive.Include);

            this._spriteManager = this._player.GetComponent<PlayerSpriteManager>();
        }

        /// <summary>
        /// Resets everything in the level to its original state.
        /// </summary>
        private void ResetLevel()
        {
            foreach (Platform platform in this._platforms)
            {
                platform.ResetPlatform();
            }

            foreach (Enemy enemy in this._enemies)
            {
                enemy.ResetEnemy();
            }

            foreach (Lever lever in this._levers)
            {
                lever.ResetLever();
            }

            foreach (Door door in this._doors)
            {
                door.ResetDoor();
            }

            if (this._crackedWall)
            {
                this._crackedWall.ResetWalls();
            }

            // TODO: Add cooldown for respawning
            this._spriteManager.OnRespawn();

            this.cineCamera.Follow = this._player.transform;

            this._player.transform.position = this._currentSpawn.position;
            this._player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }
    }
}
