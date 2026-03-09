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

    /// <summary>
    /// Checks if the player enters the hitbox from above.
    /// </summary>
    /// <param name="other">The <see cref="GameObject"/> that enters the hitbox.</param>
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
    
    /// <summary>
    /// If the platform should be falling, keep it's falling velocity lower than <see cref="maxFallSpeed"/>.
    /// </summary>
    private void FixedUpdate()
    {
        if (_isFalling)
        {
            _rb.linearVelocityY = Mathf.Max(_rb.linearVelocityY, -maxFallSpeed);
        }
    }

    /// <summary>
    /// Reset the platform to its original position and state.
    /// </summary>
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
        _collider.enabled = true;
    }
    
    /// <summary>
    /// Handle the player landing on the platform depending on the <see cref="PlatformType"/>.
    /// </summary>
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
    
    /// <summary>
    /// Handle the falling platform: <br />
    /// 1. Wait for <see cref="fallDelay"/>. <br />
    /// 2. Allow the platform to fall. <br />
    /// 3. Disable the platform after falling for 2.5 seconds. <br />
    /// </summary>
    /// <returns></returns>
    private IEnumerator HandleFallingPlatform()
    {
        yield return new WaitForSeconds(fallDelay);
        
        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        _isFalling = true;

        yield return new WaitForSeconds(2.5f);
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Allow the player to drop through the platform.
    /// </summary>
    public void DropThrough()
    {
        StartCoroutine(HandleDropThrough());
    }

    /// <summary>
    /// Handle the player dropping through the platform: <br />
    /// 1. Disable the collider. <br />
    /// 2. Wait for <see cref="disableTime"/>. <br />
    /// 3. Enable the collider again. <br />
    /// </summary>
    /// <returns></returns>
    private IEnumerator HandleDropThrough()
    {
        _collider.enabled = false;
        
        yield return new WaitForSeconds(disableTime);

        _collider.enabled = true;
    }
}
