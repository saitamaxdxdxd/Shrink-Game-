using System.Collections.Generic;
using Shrink.Maze;
using UnityEngine;

namespace Shrink.Enemies
{
    /// <summary>
    /// Enemigo que persigue al jugador directamente usando BFS.
    /// Más rápido que el TrailEnemy y no depende de las migajas —
    /// el jugador debe moverse constantemente para sobrevivir.
    /// </summary>
    public class ChaserEnemy : EnemyController
    {
        private static readonly Vector2Int[] _dirs =
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        // ──────────────────────────────────────────────────────────────────────
        // Comportamiento
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Da un paso BFS directo hacia la posición actual del jugador.
        /// Si no hay ruta (maze bloqueado), se queda quieto.
        /// </summary>
        protected override Vector2Int ChooseNextCell()
        {
            if (_player == null) return CurrentCell;
            return BfsNextStep(CurrentCell, _player.CurrentCell);
        }

        // ──────────────────────────────────────────────────────────────────────
        // BFS
        // ──────────────────────────────────────────────────────────────────────

        private Vector2Int BfsNextStep(Vector2Int from, Vector2Int to)
        {
            if (from == to) return from;

            var visited = new HashSet<Vector2Int> { from };
            var queue   = new Queue<Vector2Int>();
            var parent  = new Dictionary<Vector2Int, Vector2Int>();

            queue.Enqueue(from);

            while (queue.Count > 0)
            {
                Vector2Int current = queue.Dequeue();

                foreach (Vector2Int dir in _dirs)
                {
                    Vector2Int neighbor = current + dir;

                    if (visited.Contains(neighbor)) continue;
                    if (!CanEnter(neighbor))         continue;

                    visited.Add(neighbor);
                    parent[neighbor] = current;

                    if (neighbor == to)
                    {
                        Vector2Int step = neighbor;
                        while (parent[step] != from)
                            step = parent[step];
                        return step;
                    }

                    queue.Enqueue(neighbor);
                }
            }

            return from;
        }
    }
}
