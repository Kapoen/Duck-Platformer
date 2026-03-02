using UnityEngine;

public class PlayerHonk : MonoBehaviour
{
    [Header("Charge settings")]
    [SerializeField] private float maxChargeTime = 0.4f;
    [SerializeField] private float cooldownTime = 0.15f;

    [Header("Recoil")]
    [SerializeField] private float boostFactor = 0.25f;
    [SerializeField] private float baseBoost = 3.25f;
    [SerializeField] private float airBoost = 13f;
    [SerializeField] private float recoilDuration = 0.15f;

    private float _cooldownCounter;
    
    private bool _isCharging;
    private float _chargeTime;
    private bool _canAirHonk;
    
    private SurroundingsCheck _surroundingsCheck;
    private PlayerSpriteManager _spriteManager;
    private PlayerController _playerController;

    private void Awake()
    {
        _surroundingsCheck = gameObject.GetComponent<SurroundingsCheck>();
        _spriteManager = gameObject.GetComponent<PlayerSpriteManager>();
        _playerController = gameObject.GetComponent<PlayerController>();
    }

    private void Update()
    {
        _cooldownCounter -= Time.deltaTime;
        
        if (_isCharging)
        {
            _chargeTime += Time.deltaTime;
            _chargeTime = Mathf.Clamp(_chargeTime, 0f, maxChargeTime);
        }

        if (_surroundingsCheck.IsGrounded())
        {
            _canAirHonk = true;
        }
    }

    public void OnHonkStart()
    {
        if (!_surroundingsCheck.IsGrounded() && !_canAirHonk || _cooldownCounter > 0f)
        {
            return;
        }

        _isCharging = true;
        _chargeTime = 0f;
    }

    public void OnHonkCanceled()
    {
        if (!_isCharging)
        {
            return;
        }

        float chargePercent = _chargeTime / maxChargeTime;

        FireHonk(chargePercent);

        if (!_surroundingsCheck.IsGrounded())
        {
            _canAirHonk = false;
        }

        _isCharging = false;
    }
    
    private void FireHonk(float chargePercent)
    {
        _cooldownCounter = cooldownTime;
        Debug.Log($"Honk: {chargePercent}%");

        if (_surroundingsCheck.IsGrounded())
        {
            Vector2 movementSpeed = _playerController.GetMovementSpeed();
            
            Vector2 recoil = new Vector2(-1 * _spriteManager.FacingDirection() * (Mathf.Abs(movementSpeed.x) * boostFactor + baseBoost), 0);
            _playerController.ApplyRecoil(recoil, recoilDuration);
        }
        else
        {
            Vector2 recoil = new Vector2(-1 * _spriteManager.FacingDirection() * airBoost, 0);
            _playerController.ApplyRecoil(recoil, recoilDuration);
        }
    }
}
