using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    private PlayerController _playerController;
    private PlayerSpriteManager _spriteManager;
    private PlayerHonk _playerHonk;
    
    public void Start()
    {
        _playerController = gameObject.GetComponent<PlayerController>();
        _spriteManager = gameObject.GetComponent<PlayerSpriteManager>();
        _playerHonk = gameObject.GetComponent<PlayerHonk>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        
        _playerController.OnMove(input);
        _spriteManager.OnMove(input);
    }
    
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _playerController.SetJumpHeld(true);
        }
        else if (context.performed)
        {
            _playerController.ResetJumpBufferCounter();
        }
        else if (context.canceled)
        {
            _playerController.MultiplySpeed(new Vector2(1f, 0.5f));
            _playerController.SetJumpHeld(false);
        }
    }

    public void OnHonk(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _playerHonk.OnHonkStart();
        }

        if (context.canceled)
        {
            _playerHonk.OnHonkCanceled();
        }
    }
}
