using System.Collections;
using UnityEngine;

public enum PlatformType
{
    Static,
    OneWay,
    Falling
}
public class Platform : MonoBehaviour
{
    [SerializeField] public PlatformType type = PlatformType.Static;
    private Vector3 _startPosition;
    private Collider2D _collider;
    
    [Header("One Way Platforms")] 
    [SerializeField] private float disableTime = 0.25f;
    
    [Header("Falling Platforms")]
    [SerializeField] private float fallDelay = 1f;
    [SerializeField] private float maxFallSpeed= 12.5f;
    private bool _isFalling;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _startPosition = transform.position;
        _collider = GetComponent<Collider2D>();
        _rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.CompareTag("Player"))
        {
            return;
        }

        if (other.GetContact(0).normal.y < -0.5f)
        {
            OnPlayerLanded();
        }
    }
    
    private void FixedUpdate()
    {
        if (_isFalling)
        {
            _rb.linearVelocityY = Mathf.Max(_rb.linearVelocityY, -maxFallSpeed);
        }
    }

    public void ResetPlatform()
    {
        StopAllCoroutines();
        
        if (_rb)
        {
            _rb.bodyType = RigidbodyType2D.Static;
        }

        _isFalling = false;
        transform.position = _startPosition;
        gameObject.SetActive(true);
    }
    
    private void OnPlayerLanded()
    {
        switch (type)
        {
            case PlatformType.Falling:
                if (_isFalling)
                {
                    break;
                }
                
                StartCoroutine(HandleFallingPlatform());
                break;
            case PlatformType.Static:
            case PlatformType.OneWay:
            default:
                break;
        }
    }
    
    private IEnumerator HandleFallingPlatform()
    {
        yield return new WaitForSeconds(fallDelay);
        
        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        _isFalling = true;

        yield return new WaitForSeconds(2.5f);
        gameObject.SetActive(false);
    }
    
    public void DropThrough()
    {
        StartCoroutine(HandleDropThrough());
    }

    private IEnumerator HandleDropThrough()
    {
        _collider.enabled = false;
        
        yield return new WaitForSeconds(disableTime);

        _collider.enabled = true;
    }
}
