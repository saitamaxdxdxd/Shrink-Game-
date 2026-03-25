using System.Collections.Generic;
using Shrink.Core;
using Shrink.Player;
using UnityEngine;

namespace Shrink.Maze
{
    /// <summary>
    /// Renderiza un MazeData como GameObjects 2D en escena.
    /// Crea tiles visuales para cada tipo de celda y expone el diccionario de migajas.
    /// </summary>
    public class MazeRenderer : MonoBehaviour
    {
        // ──────────────────────────────────────────────────────────────────────
        // Config
        // ──────────────────────────────────────────────────────────────────────

        [SerializeField] private float cellSize = 1f;

        [Header("Colores")]
        [SerializeField] private Color colorWall       = new Color(0.13f, 0.13f, 0.15f);
        [SerializeField] private Color colorFloor      = new Color(0.92f, 0.92f, 0.94f);
        [SerializeField] private Color colorDoor       = new Color(0.95f, 0.60f, 0.10f);
        [SerializeField] private Color colorNarrow06   = new Color(0.30f, 0.65f, 1.00f);
        [SerializeField] private Color colorNarrow04   = new Color(0.10f, 0.35f, 0.90f);
        [SerializeField] private Color colorStart      = new Color(0.20f, 0.88f, 0.35f);
        [SerializeField] private Color colorExit       = new Color(0.90f, 0.20f, 0.20f);
        [SerializeField] private Color colorCrumb      = new Color(1.00f, 0.85f, 0.30f);

        // ──────────────────────────────────────────────────────────────────────
        // Estado
        // ──────────────────────────────────────────────────────────────────────

        public float CellSize => cellSize;
        public MazeData Data  { get; private set; }

        /// <summary>Migajas activas indexadas por celda.</summary>
        public Dictionary<Vector2Int, Crumb> Crumbs { get; } = new();

        private Transform _wallParent;
        private Transform _floorParent;
        private Transform _crumbParent;

        // ──────────────────────────────────────────────────────────────────────
        // API pública
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Limpia la escena anterior y renderiza el nuevo MazeData.
        /// </summary>
        public void Render(MazeData data)
        {
            Clear();
            Data = data;

            _wallParent  = CreateParent("Walls");
            _floorParent = CreateParent("Floors");
            _crumbParent = CreateParent("Crumbs");

            Sprite square = ShapeFactory.GetSquare();

            for (int x = 0; x < data.Width;  x++)
            for (int y = 0; y < data.Height; y++)
            {
                Vector3 pos = CellToWorld(new Vector2Int(x, y));
                CellType ct = data.Grid[x, y];

                if (ct == CellType.WALL)
                {
                    CreateTile($"W{x}_{y}", square, colorWall, _wallParent, pos, cellSize, 0);
                    continue;
                }

                // Suelo base
                CreateTile($"F{x}_{y}", square, colorFloor, _floorParent, pos, cellSize, 0);

                // Overlays según tipo
                switch (ct)
                {
                    case CellType.DOOR:
                        CreateTile($"D{x}_{y}", square, colorDoor, _floorParent, pos, cellSize * 0.9f, 1);
                        break;

                    case CellType.NARROW_06:
                        CreateNarrowOverlay($"N6_{x}_{y}", x, y, data, square, colorNarrow06, 0.6f, pos);
                        break;

                    case CellType.NARROW_04:
                        CreateNarrowOverlay($"N4_{x}_{y}", x, y, data, square, colorNarrow04, 0.4f, pos);
                        break;

                    case CellType.START:
                        CreateTile($"S{x}_{y}", square, colorStart, _floorParent, pos, cellSize * 0.5f, 1);
                        break;

                    case CellType.EXIT:
                        CreateTile($"E{x}_{y}", square, colorExit, _floorParent, pos, cellSize * 0.8f, 1);
                        break;
                }
            }
        }

        /// <summary>
        /// Deposita una migaja visual en la celda indicada.
        /// </summary>
        public void SpawnCrumb(Vector2Int cell, float sizeStored)
        {
            if (Crumbs.ContainsKey(cell)) return;

            Vector3 pos = CellToWorld(cell);
            var go      = ShapeFactory.CreateSprite($"Crumb_{cell.x}_{cell.y}",
                              ShapeFactory.GetCircle(), colorCrumb, _crumbParent, sortingOrder: 2);
            go.transform.position   = pos;
            go.transform.localScale = Vector3.one * cellSize * 0.25f;

            var crumb = go.AddComponent<Crumb>();
            crumb.Initialize(cell, sizeStored);
            Crumbs[cell] = crumb;
        }

        /// <summary>
        /// Elimina y devuelve el tamaño almacenado en la migaja de la celda indicada.
        /// Devuelve 0 si no hay migaja.
        /// </summary>
        public float AbsorbCrumb(Vector2Int cell)
        {
            if (!Crumbs.TryGetValue(cell, out Crumb crumb)) return 0f;

            float stored = crumb.SizeStored;
            Crumbs.Remove(cell);
            Destroy(crumb.gameObject);
            return stored;
        }

        /// <summary>
        /// Convierte coordenadas de celda a posición world.
        /// </summary>
        public Vector3 CellToWorld(Vector2Int cell) =>
            transform.position + new Vector3(cell.x * cellSize, cell.y * cellSize, 0f);

        /// <summary>
        /// Destruye todos los GameObjects generados y limpia el estado.
        /// </summary>
        public void Clear()
        {
            if (_wallParent  != null) Destroy(_wallParent.gameObject);
            if (_floorParent != null) Destroy(_floorParent.gameObject);
            if (_crumbParent != null) Destroy(_crumbParent.gameObject);
            Crumbs.Clear();
            Data = null;
        }

        // ──────────────────────────────────────────────────────────────────────
        // Helpers privados
        // ──────────────────────────────────────────────────────────────────────

        private Transform CreateParent(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            return go.transform;
        }

        private void CreateTile(string name, Sprite sprite, Color color,
                                Transform parent, Vector3 pos, float size, int order)
        {
            var go = ShapeFactory.CreateSprite(name, sprite, color, parent, order);
            go.transform.position   = pos;
            go.transform.localScale = Vector3.one * size;
        }

        /// <summary>
        /// Dibuja la "ranura" estrecha como dos paredes a los lados del corredor.
        /// </summary>
        private void CreateNarrowOverlay(string name, int x, int y, MazeData data,
                                          Sprite square, Color color, float widthRatio, Vector3 pos)
        {
            bool horizontal = IsHorizontalCorridor(x, y, data);

            float wallThickness = cellSize * ((1f - widthRatio) * 0.5f);
            float wallLength    = cellSize;

            // Dos paredes perpendiculares al eje del corredor
            for (int side = -1; side <= 1; side += 2)
            {
                var go  = ShapeFactory.CreateSprite(name + $"_s{side}", square, color, _floorParent, 1);
                float ox = horizontal ? 0f : side * (cellSize * 0.5f - wallThickness * 0.5f);
                float oy = horizontal ? side * (cellSize * 0.5f - wallThickness * 0.5f) : 0f;

                go.transform.position = pos + new Vector3(ox, oy, 0f);
                go.transform.localScale = horizontal
                    ? new Vector3(wallLength, wallThickness, 1f)
                    : new Vector3(wallThickness, wallLength, 1f);
            }
        }

        private bool IsHorizontalCorridor(int x, int y, MazeData data)
        {
            bool n = !data.InBounds(x, y + 1) || data.Grid[x, y + 1] == CellType.WALL;
            bool s = !data.InBounds(x, y - 1) || data.Grid[x, y - 1] == CellType.WALL;
            return n && s;
        }
    }
}
