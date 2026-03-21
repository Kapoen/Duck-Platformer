namespace Player
{
    using UnityEngine;

    /// <summary>
    /// Handle the player honk interaction.
    /// </summary>
    public class PlayerHonk : MonoBehaviour
    {
        [Header("Charge settings")]
        [SerializeField]
        private float maxChargeTime = 0.4f;
        [SerializeField]
        private float cooldownTime = 0.15f;

        [Header("Recoil")]
        [SerializeField]
        private float boostFactor = 0.25f;
        [SerializeField]
        private float baseBoost = 2f;
        [SerializeField]
        private float airBoost = 19.5f;
        [SerializeField]
        private float recoilDuration = 0.25f;

        [Header("Sound waves")]
        [SerializeField]
        private GameObject soundWavePrefab;
        [SerializeField]
        private float minRadius = 1f;
        [SerializeField]
        private float maxRadius = 10f;

        private float _cooldownCounter;

        private bool _isCharging;
        private float _chargeTime;
        private bool _canAirHonk;

        private SurroundingsCheck _surroundingsCheck;
        private PlayerSpriteManager _spriteManager;
        private PlayerController _playerController;

        /// <summary>
        /// Start charging the honk.
        /// </summary>
        public void OnHonkStart()
        {
            this._isCharging = true;
            this._chargeTime = 0f;
        }

        /// <summary>
        /// Release and fire the honk.
        /// </summary>
        public void OnHonkCanceled()
        {
            if (!this._isCharging)
            {
                return;
            }

            if ((!this._surroundingsCheck.IsGrounded() && !this._canAirHonk) || this._cooldownCounter > 0f)
            {
                this._isCharging = false;
                return;
            }

            float chargePercent = this._chargeTime / this.maxChargeTime;

            this.FireHonk(chargePercent);

            if (!this._surroundingsCheck.IsGrounded())
            {
                this._canAirHonk = false;
            }

            this._isCharging = false;
        }

        /// <summary>
        /// Fire the sound wave and apply the recoil to the player.
        /// </summary>
        /// <param name="chargePercent">How far along the charge is.</param>
        private void FireHonk(float chargePercent)
        {
            this._cooldownCounter = this.cooldownTime;

            float radius = Mathf.Lerp(this.minRadius, this.maxRadius, chargePercent);
            GameObject wave = Instantiate(this.soundWavePrefab, this.transform.position, Quaternion.identity);
            wave.GetComponent<SoundWave>().Initialize(this.minRadius, radius, this._spriteManager.FacingDirection());

            // Check if grounded, with a safety check for platforms.
            if (this._surroundingsCheck.IsGrounded())
            {
                Vector2 movementSpeed = this._playerController.GetMovementSpeed();
                float boostDirection = -1 * this._spriteManager.FacingDirection();

                Vector2 recoil = new Vector2(
                    boostDirection * ((Mathf.Abs(movementSpeed.x) * this.boostFactor) + this.baseBoost),
                    movementSpeed.y);

                this._playerController.ApplyRecoil(recoil, 0f);
            }
            else
            {
                Vector2 recoil = new Vector2(-1 * this._spriteManager.FacingDirection() * this.airBoost, 0);
                this._playerController.ApplyRecoil(recoil, this.recoilDuration);
            }
        }

        private void Awake()
        {
            this._surroundingsCheck = this.gameObject.GetComponent<SurroundingsCheck>();
            this._spriteManager = this.gameObject.GetComponent<PlayerSpriteManager>();
            this._playerController = this.gameObject.GetComponent<PlayerController>();
        }

        private void Update()
        {
            this._cooldownCounter -= Time.deltaTime;

            if (this._isCharging)
            {
                this._chargeTime += Time.deltaTime;
                this._chargeTime = Mathf.Clamp(this._chargeTime, 0f, this.maxChargeTime);
            }

            if (this._surroundingsCheck.IsGrounded())
            {
                this._canAirHonk = true;
            }
        }
    }
}
