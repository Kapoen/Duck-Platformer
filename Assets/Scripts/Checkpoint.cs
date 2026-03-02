using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private LevelManager _levelManager;
    private SpriteRenderer _spriteRenderer;

    private bool _activated;
    
    private void Start()
    {
        _levelManager = LevelManager.Instance;
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_activated)
        {
            return;
        }
        
        if (other.CompareTag("Player"))
        {
            _levelManager.SetPlayerSpawn(gameObject.transform);
            _spriteRenderer.color = Color.softYellow;
            _activated = true;
        }
    }
}
