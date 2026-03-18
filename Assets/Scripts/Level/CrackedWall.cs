namespace Level
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Tilemaps;

    /// <summary>
    /// Handle the logic for a cracked wall.
    /// </summary>
    public class CrackedWall : MonoBehaviour
    {
        private TilemapCollider2D _collider;
        private Tilemap _tilemap;
        private Tilemap _originalMap;

        /// <summary>
        /// Crack the wall, by removing all adjacent cells using flood fill.
        /// </summary>
        /// <param name="hit">Where the wall was cracked.</param>
        public void CrackWall(Vector2 hit)
        {
            this._collider.enabled = false;

            Vector3Int hitCell = this._tilemap.WorldToCell(hit);
            this._tilemap.SetTile(hitCell, null);

            Vector3Int[] directions = { Vector3Int.up, Vector3Int.right, Vector3Int.left, Vector3Int.down };

            Queue<Vector3Int> queue = new Queue<Vector3Int>();
            queue.Enqueue(hitCell);

            while (queue.Count > 0)
            {
                Vector3Int currentCell = queue.Dequeue();

                foreach (Vector3Int direction in directions)
                {
                    Vector3Int cell = currentCell + direction;

                    if (this._tilemap.HasTile(cell))
                    {
                        this._tilemap.SetTile(cell, null);
                        queue.Enqueue(cell);
                    }
                }
            }

            this._collider.enabled = true;

            // Trigger an update on the collider.
            this._collider.compositeOperation = Collider2D.CompositeOperation.Merge;
        }

        /// <summary>
        /// Reset the cracked walls.
        /// </summary>
        public void ResetWalls()
        {
            this.CopyTilemap(this._originalMap, this._tilemap);
        }

        /// <summary>
        /// Copies all tiles from the source to the destination map.
        /// </summary>
        /// <param name="source">The source map.</param>
        /// <param name="dest">The map to copy to.</param>
        private void CopyTilemap(Tilemap source, Tilemap dest)
        {
            dest.ClearAllTiles();
            BoundsInt bounds = source.cellBounds;
            TileBase[] tiles = source.GetTilesBlock(bounds);
            dest.SetTilesBlock(bounds, tiles);
        }

        private void Awake()
        {
            this._collider = this.gameObject.GetComponent<TilemapCollider2D>();
            this._tilemap = this.gameObject.GetComponent<Tilemap>();

            // Make a backup of the tilemap.
            GameObject originalObject = new GameObject("OriginalTilemap");
            this._originalMap = originalObject.AddComponent<Tilemap>();
            this._originalMap.enabled = false;

            this.CopyTilemap(this._tilemap, this._originalMap);
        }
    }
}
