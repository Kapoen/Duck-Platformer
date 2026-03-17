namespace Utils
{
    using System.Collections;
    using Unity.Cinemachine;
    using UnityEngine;

    /// <summary>
    /// Handles fixing the camera in place.
    /// </summary>
    public class CameraAdjuster : MonoBehaviour
    {
        [SerializeField]
        private CinemachineCamera cineCamera;
        [SerializeField]
        private CinemachinePositionComposer composer;
        [SerializeField]
        private Transform player;
        [SerializeField]
        private Vector3 playerOffset = new Vector3(0, 2, 0);
        [SerializeField]
        private Transform cameraPosition;
        [SerializeField]
        private float transitionSpeed = 0.5f;
        [SerializeField]
        private float smoothingDuration = 0.5f;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                this.cineCamera.Follow = null;
                this.StopAllCoroutines();
                this.StartCoroutine(this.MoveCameraToPosition(this.cameraPosition.position));
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                this.StopAllCoroutines();
                this.StartCoroutine(this.MoveCameraToPlayer());
            }
        }

        private IEnumerator MoveCameraToPosition(Vector3 targetPosition)
        {
            while (Vector3.Distance(this.cineCamera.transform.position, targetPosition) > 0.1f)
            {
                this.cineCamera.transform.position = Vector3.MoveTowards(
                    this.cineCamera.transform.position,
                    targetPosition,
                    this.transitionSpeed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }
        }

        private IEnumerator MoveCameraToPlayer()
        {
            Vector3 targetPosition = this.player.position + this.playerOffset;
            targetPosition.z = -10;

            this.composer.Damping = Vector3.zero;
            this.composer.Lookahead = new LookaheadSettings
            {
                Enabled = false,
            };

            while (Vector3.Distance(this.cineCamera.transform.position, targetPosition) > 0.05f)
            {
                targetPosition = this.player.position + this.playerOffset;
                targetPosition.z = -10;

                this.cineCamera.transform.position = Vector3.MoveTowards(
                    this.cineCamera.transform.position,
                    targetPosition,
                    this.transitionSpeed * Time.deltaTime);
                yield return null;
            }

            this.cineCamera.Follow = this.player;

            float elapsedTime = 0f;
            while (elapsedTime < this.smoothingDuration)
            {
                float t = elapsedTime / this.smoothingDuration;

                this.composer.Damping = Vector3.Lerp(Vector3.zero, new Vector3(1, 1, 0), t);
                this.composer.Lookahead = new LookaheadSettings
                {
                    Enabled = true,
                    Time = Mathf.Lerp(0f, 0.4f, t),
                    Smoothing = Mathf.Lerp(0f, 4f, t),
                };

                elapsedTime += Time.deltaTime;

                yield return null;
            }

            this.composer.Damping = new Vector3(1, 1, 0);
            this.composer.Lookahead = new LookaheadSettings
            {
                Enabled = true,
                Time = 0.4f,
                Smoothing = 4,
            };
        }
    }
}
