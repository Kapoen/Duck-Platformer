using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

namespace Utils
{
    public class CameraAdjuster : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera cineCamera;
        [SerializeField] private CinemachinePositionComposer composer;
        [SerializeField] private Transform player;
        [SerializeField] private Vector3 playerOffset = new Vector3(0, 2, 0);
        [SerializeField] private Transform cameraPosition;
        [SerializeField] private float transitionSpeed = 0.5f;
        [SerializeField] private float smoothingDuration = 0.5f;
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                cineCamera.Follow = null;
                StopAllCoroutines();
                StartCoroutine(MoveCameraToPosition(cameraPosition.position));
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                StopAllCoroutines();
                StartCoroutine(MoveCameraToPlayer());
            }
        }

        private IEnumerator MoveCameraToPosition(Vector3 targetPosition)
        {
            while (Vector3.Distance(cineCamera.transform.position, targetPosition) > 0.1f)
            {
                cineCamera.transform.position = Vector3.MoveTowards(
                    cineCamera.transform.position,
                    targetPosition,
                    transitionSpeed * Time.deltaTime
                );
                yield return new WaitForEndOfFrame();
            }
        }
        
        private IEnumerator MoveCameraToPlayer()
        {
            Vector3 targetPosition = player.position + playerOffset;
            targetPosition.z = -10;
            
            composer.Damping = Vector3.zero;
            composer.Lookahead = new LookaheadSettings
            {
                Enabled = false
            };
            
            while (Vector3.Distance(cineCamera.transform.position, targetPosition) > 0.05f)
            {
                targetPosition = player.position + playerOffset;
                targetPosition.z = -10;
                
                cineCamera.transform.position = Vector3.MoveTowards(
                    cineCamera.transform.position,
                    targetPosition,
                    transitionSpeed * Time.deltaTime
                );
                yield return null;
            }
            
            cineCamera.Follow = player;
            
            float elapsedTime = 0f;
            while (elapsedTime < smoothingDuration)
            {
                float t = elapsedTime / smoothingDuration;
                
                composer.Damping = Vector3.Lerp(Vector3.zero, new Vector3(1, 1, 0), t);
                composer.Lookahead = new LookaheadSettings
                {
                    Enabled = true,
                    Time = Mathf.Lerp(0f, 0.4f, t),
                    Smoothing = Mathf.Lerp(0f, 4f, t)
                };
        
                elapsedTime += Time.deltaTime;
                
                yield return null;
            }
            
            composer.Damping = new Vector3(1, 1, 0);
            composer.Lookahead = new LookaheadSettings
            {
                Enabled = true,
                Time = 0.4f,
                Smoothing = 4
            };
        }
    }
}
