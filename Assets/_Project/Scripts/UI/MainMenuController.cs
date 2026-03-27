using Shrink.Audio;
using UnityEngine;

namespace Shrink.UI
{
    /// <summary>
    /// Controla la navegación entre paneles del menú principal.
    /// Vive en el Canvas de MenuScene.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Paneles")]
        [SerializeField] private GameObject _mainPanel;
        [SerializeField] private GameObject _levelSelectPanel;
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private GameObject _storePanel;

        [Header("Controladores de panel")]
        [SerializeField] private LevelSelectController _levelSelect;
        [SerializeField] private SettingsController    _settings;
        [SerializeField] private StoreController       _store;

        // ──────────────────────────────────────────────────────────────────────
        // Ciclo de vida
        // ──────────────────────────────────────────────────────────────────────

        private void Start()
        {
            ShowPanel(_mainPanel);
            AudioManager.Instance?.PlayMenuMusic();
        }

        // ──────────────────────────────────────────────────────────────────────
        // Botones del MainPanel — conectar en Inspector
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>Abre el panel de selección de nivel.</summary>
        public void OnPlayPressed()
        {
            AudioManager.Instance?.PlayButtonTap();
            _levelSelect.Refresh();
            ShowPanel(_levelSelectPanel);
        }

        /// <summary>Abre el panel de ajustes.</summary>
        public void OnSettingsPressed()
        {
            AudioManager.Instance?.PlayButtonTap();
            _settings.Refresh();
            ShowPanel(_settingsPanel);
        }

        /// <summary>Abre el panel de tienda.</summary>
        public void OnStorePressed()
        {
            AudioManager.Instance?.PlayButtonTap();
            _store.Refresh();
            ShowPanel(_storePanel);
        }

        /// <summary>Vuelve al panel principal desde cualquier sub-panel.</summary>
        public void OnBackPressed()
        {
            AudioManager.Instance?.PlayButtonTap();
            ShowPanel(_mainPanel);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Helper
        // ──────────────────────────────────────────────────────────────────────

        private void ShowPanel(GameObject target)
        {
            _mainPanel.SetActive(_mainPanel        == target);
            _levelSelectPanel.SetActive(_levelSelectPanel == target);
            _settingsPanel.SetActive(_settingsPanel    == target);
            _storePanel.SetActive(_storePanel        == target);
        }
    }
}
