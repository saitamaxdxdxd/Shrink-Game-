using UnityEngine;

namespace Shrink.Player
{
    /// <summary>
    /// Migaja depositada por la esfera al pasar por una celda.
    /// Almacena el tamaño exacto que se perdió al depositarla.
    /// La gestión visual (spawn/destroy) la hace MazeRenderer.
    /// </summary>
    public class Crumb : MonoBehaviour
    {
        /// <summary>Celda del maze a la que pertenece esta migaja.</summary>
        public Vector2Int Cell { get; private set; }

        /// <summary>Tamaño que recuperará la esfera al absorberla.</summary>
        public float SizeStored { get; private set; }

        /// <summary>Inicializa la migaja con su celda y tamaño almacenado.</summary>
        public void Initialize(Vector2Int cell, float sizeStored)
        {
            Cell       = cell;
            SizeStored = sizeStored;
        }
    }
}
