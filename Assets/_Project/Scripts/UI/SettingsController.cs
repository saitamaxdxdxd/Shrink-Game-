using Shrink.Audio;
using Shrink.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shrink.UI
{
    /// <summary>
    /// Controla el panel de ajustes: volumen SFX/música y modo de movimiento.
    /// </summary>
    public class SettingsController : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private Slider _sfxSlider;
        [SerializeField] private Slider _musicSlider;

        [Header("Movimiento")]
        [SerializeField] private Button   _movementButton;
        [SerializeField] private TMP_Text _movementButtonText;

        private static readonly string[] MovementLabels =
        {
            "◀  SLIDE  ▶",
            "◀  CONTINUO  ▶",
            "◀  PASO A PASO  ▶",
        };

        private int _currentMode;

        // ──────────────────────────────────────────────────────────────────────
        // Ciclo de vida
        // ──────────────────────────────────────────────────────────────────────

        private void Awake()
        {
            _sfxSlider.onValueChanged.AddListener(OnSFXChanged);
            _musicSlider.onValueChanged.AddListener(OnMusicChanged);
            _movementButton.onClick.AddListener(OnMovementPressed);
        }

        // ──────────────────────────────────────────────────────────────────────
        // API pública
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>Carga los valores actuales guardados. Llamar al abrir el panel.</summary>
        public void Refresh()
        {
            if (AudioManager.Instance != null)
            {
                _sfxSlider.SetValueWithoutNotify(AudioManager.Instance.SFXVolume);
                _musicSlider.SetValueWithoutNotify(AudioManager.Instance.MusicVolume);
            }

            _currentMode = SaveManager.Instance?.Data.settings.movementMode ?? 0;
            UpdateMovementLabel();
        }

        // ──────────────────────────────────────────────────────────────────────
        // Handlers
        // ──────────────────────────────────────────────────────────────────────

        private void OnSFXChanged(float value)   => AudioManager.Instance?.SetSFXVolume(value);
        private void OnMusicChanged(float value) => AudioManager.Instance?.SetMusicVolume(value);

        public void OnMovementPressed()
        {
            AudioManager.Instance?.PlayButtonTap();
            _currentMode = (_currentMode + 1) % MovementLabels.Length;
            SaveManager.Instance?.SaveMovementMode(_currentMode);
            UpdateMovementLabel();
        }

        private void UpdateMovementLabel() => _movementButtonText.text = MovementLabels[_currentMode];
    }
}
