using UnityEngine;

namespace Shrink.Camera
{
    /// <summary>
    /// Cámara ortográfica 2D que sigue suavemente al jugador.
    /// Mantiene el zoom calibrado según el tamaño del maze visible.
    /// </summary>
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CameraFollow : MonoBehaviour
    {
        // ──────────────────────────────────────────────────────────────────────
        // Config
        // ──────────────────────────────────────────────────────────────────────

        [SerializeField] private float smoothSpeed     = 6f;
        [SerializeField] private float orthographicSize = 7f;

        // ──────────────────────────────────────────────────────────────────────
        // Estado
        // ──────────────────────────────────────────────────────────────────────

        private Transform            _target;
        private UnityEngine.Camera   _cam;

        // ──────────────────────────────────────────────────────────────────────
        // Inicialización
        // ──────────────────────────────────────────────────────────────────────

        public void Initialize(Transform target, float orthoSize)
        {
            _cam = GetComponent<UnityEngine.Camera>();
            _cam.orthographic     = true;
            _cam.orthographicSize = orthoSize;
            _cam.backgroundColor  = new Color(0.08f, 0.08f, 0.10f);

            _target = target;
            orthographicSize = orthoSize;

            // Snap inmediato al inicio
            SnapToTarget();
        }

        // ──────────────────────────────────────────────────────────────────────
        // Update
        // ──────────────────────────────────────────────────────────────────────

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 desired  = new Vector3(_target.position.x, _target.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Utilidades
        // ──────────────────────────────────────────────────────────────────────

        private void SnapToTarget()
        {
            if (_target == null) return;
            transform.position = new Vector3(_target.position.x, _target.position.y, -10f);
        }
    }
}
