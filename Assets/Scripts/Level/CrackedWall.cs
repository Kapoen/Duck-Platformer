using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Level
{
    public class CrackedWall : MonoBehaviour
    {
        private TilemapCollider2D _collider;
        private Tilemap _tilemap;

        private void Awake()
        {
            _collider = gameObject.GetComponent<TilemapCollider2D>();
            _tilemap = gameObject.GetComponent<Tilemap>();
        }

        /// <summary>
        /// Crack the wall, by removing all adjacent cells using flood fill.
        /// </summary>
        /// <param name="hit">Where the wall was cracked.</param>
        public void CrackWall(Vector2 hit)
        {
            _collider.enabled = false;

            Vector3Int hitCell = _tilemap.WorldToCell(hit);
            _tilemap.SetTile(hitCell, null);

            Vector3Int[] directions = { Vector3Int.up, Vector3Int.right, Vector3Int.left, Vector3Int.down };
            
            Queue<Vector3Int> queue = new Queue<Vector3Int>();
            queue.Enqueue(hitCell);

            while (queue.Count > 0)
            {
                Vector3Int currentCell = queue.Dequeue();

                foreach (Vector3Int direction in directions)
                {
                    Vector3Int cell = currentCell + direction;

                    if (_tilemap.HasTile(cell))
                    {
                        _tilemap.SetTile(cell, null);
                        queue.Enqueue(cell);
                    }
                }
            }

            _collider.enabled = true;
            // Trigger an update on the collider.
            _collider.compositeOperation = Collider2D.CompositeOperation.Merge;
        }
    }
}
