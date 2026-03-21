namespace Player
{
    using System.Collections.Generic;
    using Level;
    using UnityEngine;

    /// <summary>
    /// Handles the sound wave of the honk.
    /// </summary>
    public class SoundWave : MonoBehaviour
    {
        private readonly List<RingState> _rings = new List<RingState>();

        [Header("Wave Shape")]
        [SerializeField]
        private float expandSpeed = 5f;
        [SerializeField]
        private float thickness = 0.12f;
        [SerializeField]
        private float arcAngle = 110f;
        [SerializeField]
        private int rayCount = 72;

        [Header("Multi-Ring Honk")]
        [SerializeField]
        private int ringsPerHonk = 3;
        [SerializeField]
        private float ringDelay = 0.12f;

        [Header("Visuals")]
        [SerializeField]
        private float fadeStartFraction = 0.55f;
        [SerializeField]
        private Material lineMaterial;

        [Header("Layers")]
        [SerializeField]
        private LayerMask groundLayer;
        [SerializeField]
        private LayerMask enemyLayer;
        [SerializeField]
        private LayerMask objectLayer;

        private Vector2 _origin;
        private float _facingAngle;
        private float _minRadius;
        private float _maxRadius;

        private int _ringsSpawned;
        private float _ringTimer;
        private bool _initialized;

        /// <summary>
        /// Initialize the sound wave.
        /// </summary>
        /// <param name="minRadius">The minimum radius of the wave.</param>
        /// <param name="maxRadius">The maximum radius of the wave.</param>
        /// <param name="facingDirection">The direction the player is facing.</param>
        public void Initialize(float minRadius, float maxRadius, int facingDirection)
        {
            this._origin = this.transform.position;
            this._facingAngle = (facingDirection == 1) ? 0f : 180f;

            this._minRadius = minRadius;
            this._maxRadius = maxRadius;

            this.SpawnRing();

            this._ringsSpawned = 1;
            this._ringTimer = this.ringDelay;
            this._initialized = true;
        }

        /// <summary>
        /// Delete all fired rings.
        /// </summary>
        public void DeleteRings()
        {
            GameObject.Destroy(this.gameObject);
        }

        /// <summary>
        /// For each ring and each ray, raycast to see if it hits something. If it does, handle the hit and block that ray.
        /// </summary>
        private void CacheRayDistances()
        {
            float halfArc = this.arcAngle * 0.5f;
            for (int i = 0; i < this._ringsSpawned; i++)
            {
                RingState ring = this._rings[i];

                for (int j = 0; j < this.rayCount; j++)
                {
                    if (ring.RayBlocked[j])
                    {
                        continue;
                    }

                    float t = (float)j / (this.rayCount - 1);
                    float angleDeg = this._facingAngle - halfArc + (t * this.arcAngle);
                    float angleRad = angleDeg * Mathf.Deg2Rad;
                    Vector2 dir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
                    float dist = ring.Radius + 0.1f;

                    RaycastHit2D hit = Physics2D.Raycast(
                        this._origin,
                        dir,
                        dist,
                        this.groundLayer | this.enemyLayer | this.objectLayer);

                    if (hit.collider)
                    {
                        ring.RayBlocked[j] = true;
                        GameObject objectHit = hit.collider.gameObject;

                        if (objectHit.CompareTag("Enemy"))
                        {
                            objectHit.GetComponent<Enemy>().AngerEnemy(this._origin);
                        }
                        else if (objectHit.CompareTag("Cracked Wall"))
                        {
                            objectHit.GetComponent<CrackedWall>().CrackWall(hit.point);
                        }
                        else if (objectHit.CompareTag("Lever"))
                        {
                            objectHit.GetComponent<Lever>().ActivateLever();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Spawn a ring.
        /// </summary>
        private void SpawnRing()
        {
            GameObject ring = new GameObject("Ring");
            ring.transform.SetParent(this.transform);
            this._rings.Add(
                new RingState
                {
                    Segments = new List<LineRenderer>(),
                    RayBlocked = new bool[this.rayCount],
                    Radius = this._minRadius,
                    Dead = false,
                    RingRoot = ring.transform,
                });
        }

        /// <summary>
        /// Create a line segment.
        /// </summary>
        /// <param name="parent">The <see cref="Transform"/> of which the segment should be the child.</param>
        /// <returns>The line renderer of the segment.</returns>
        private LineRenderer CreateSegment(Transform parent)
        {
            GameObject segment = new GameObject("Segment");
            segment.transform.SetParent(parent);

            LineRenderer lr = segment.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.loop = false;
            lr.startWidth = this.thickness;
            lr.endWidth = this.thickness;
            lr.numCapVertices = 0;
            lr.numCornerVertices = 0;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.material = this.lineMaterial;

            return lr;
        }

        private void Update()
        {
            if (!this._initialized)
            {
                return;
            }

            // Calculate the raycasts
            this.CacheRayDistances();

            if (this._ringsSpawned < this.ringsPerHonk)
            {
                this._ringTimer -= Time.deltaTime;
                if (this._ringTimer <= 0f)
                {
                    this.SpawnRing();
                    this._ringsSpawned++;
                    this._ringTimer = this.ringDelay;
                }
            }

            bool anyAlive = false;

            // Check if the ring is still alive and expand it
            for (int r = 0; r < this._rings.Count; r++)
            {
                RingState ring = this._rings[r];
                if (ring.Dead)
                {
                    continue;
                }

                ring.Radius += this.expandSpeed * Time.deltaTime;

                if (ring.Radius >= this._maxRadius)
                {
                    ring.Dead = true;
                    Destroy(ring.RingRoot.gameObject);
                    this._rings[r] = ring;
                    continue;
                }

                float fadeStart = this.fadeStartFraction * this._maxRadius;
                float alpha = ring.Radius > fadeStart
                    ? Mathf.Lerp(1f, 0f, (ring.Radius - fadeStart) / (this._maxRadius - fadeStart))
                    : 1f;

                this.UpdateRingGeometry(ref ring, alpha);

                this._rings[r] = ring;
                anyAlive = true;
            }

            // If no ring alive, destroy the sound wave.
            if (!anyAlive && this._ringsSpawned >= this.ringsPerHonk)
            {
                Destroy(this.gameObject);
            }
        }

        /// <summary>
        /// Update the ring, with the new segments.
        /// </summary>
        /// <param name="ring">The ring to update.</param>
        /// <param name="alpha">The alpha value of the ring color.</param>
        private void UpdateRingGeometry(ref RingState ring, float alpha)
        {
            float halfArc = this.arcAngle * 0.5f;
            Color c = new Color(1, 1, 1, alpha);

            var segments = new List<List<Vector3>>();
            List<Vector3> current = null;

            // Loop through all the rays and make segments for it based on blocked rays
            for (int i = 0; i < this.rayCount; i++)
            {
                float t = this.rayCount > 1 ? (float)i / (this.rayCount - 1) : 0f;
                float angleDeg = this._facingAngle - halfArc + (t * this.arcAngle);
                float angleRad = angleDeg * Mathf.Deg2Rad;
                Vector2 dir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

                if (ring.RayBlocked[i])
                {
                    if (current != null)
                    {
                        segments.Add(current);
                        current = null;
                    }
                }
                else
                {
                    if (current == null)
                    {
                        current = new List<Vector3>();
                    }

                    Vector2 pos = this._origin + (dir * ring.Radius);
                    current.Add(new Vector3(pos.x, pos.y, 0f));
                }
            }

            // Add the last segment if needed
            if (current != null)
            {
                segments.Add(current);
            }

            // Update the ring segments amount to match the newly created segments
            while (ring.Segments.Count < segments.Count)
            {
                ring.Segments.Add(this.CreateSegment(ring.RingRoot));
            }

            while (ring.Segments.Count > segments.Count)
            {
                int last = ring.Segments.Count - 1;
                Destroy(ring.Segments[last].gameObject);
                ring.Segments.RemoveAt(last);
            }

            // For each segment set the points
            for (int s = 0; s < segments.Count; s++)
            {
                LineRenderer lr = ring.Segments[s];
                List<Vector3> pts = segments[s];
                lr.positionCount = pts.Count;
                lr.SetPositions(pts.ToArray());
                lr.startColor = c;
                lr.endColor = c;
            }
        }

        private struct RingState
        {
            public List<LineRenderer> Segments;
            public bool[] RayBlocked;
            public Transform RingRoot;
            public float Radius;
            public bool Dead;
        }
    }
}