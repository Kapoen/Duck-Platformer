using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SurroundingsCheck : MonoBehaviour
{
    [Header("Ground Check")] 
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.9f, 0.05f);
    private bool _grounded;
    
    [Header("Wall Check")] 
    [SerializeField] private Transform wallCheckLeft;
    [SerializeField] private Transform wallCheckRight;
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.05f, 0.8f);
    private int _wallDirection;
    
    [Header("Layers")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask platformLayer;

    private BoxCollider2D _collider;

    private void Awake()
    {
        _collider = gameObject.GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        GroundedCheck();
        WallDirectionCheck();
    }

    private void GroundedCheck()
    {
        ContactFilter2D filter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = platformLayer,
        };

        // Stricter for platforms, since you can be inside of those
        List<ContactPoint2D> contacts = new List<ContactPoint2D>();
        _collider.GetContacts(filter, contacts);
        bool onPlatform = contacts.Any(point => point.otherCollider.bounds.min.y > point.collider.bounds.max.y);
        
        RaycastHit2D hit = Physics2D.BoxCast(
            groundCheck.position,
            groundCheckSize,
            0f,
            Vector2.down,
            0.025f,
            groundLayer
        );
        
        // Check for normal, to avoid clipping collider
        _grounded = (hit.collider && Mathf.Abs(hit.normal.y) > 0.01f) || onPlatform;
    }
    
    private bool IsOnWallLeft()
    {
        return Physics2D.OverlapBox(wallCheckLeft.position, wallCheckSize, 0f, groundLayer);
    }

    private bool IsOnWallRight()
    {
        return Physics2D.OverlapBox(wallCheckRight.position, wallCheckSize, 0f, groundLayer);
    }

    private void WallDirectionCheck()
    {
        if (IsOnWallLeft())
        {
            _wallDirection = -1;
        }
        else if (IsOnWallRight())
        {
            _wallDirection = 1;
        }
        else
        {
            _wallDirection = 0;
        }
    }

    public bool IsGrounded()
    {
        return _grounded;
    }

    public int WallDirection()
    {
        return _wallDirection;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }

        if (wallCheckLeft != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(wallCheckLeft.position, wallCheckSize);
        }

        if (wallCheckRight != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(wallCheckRight.position, wallCheckSize);
        }
    }
}
