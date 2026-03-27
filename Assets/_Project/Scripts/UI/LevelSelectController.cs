using Shrink.Audio;
using Shrink.Core;
using Shrink.Level;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Shrink.UI
{
    /// <summary>
    /// Gestiona el panel de selección de nivel con paginación de 6 en 6.
    /// </summary>
    public class LevelSelectController : MonoBehaviour
    {
        [Header("Slots del grid (6 fijos)")]
        [SerializeField] private LevelButtonUI[] _slots;   // arrastrar los 6 LevelButton del grid

        [Header("Navegación")]
        [SerializeField] private Button   _prevButton;
        [SerializeField] private Button   _nextButton;
        [SerializeField] private TMP_Text _pageLabel;

        [Header("Escena de juego")]
        [SerializeField] private string _gameSceneName = "GameScene";

        private const int SlotsPerPage = 6;
        private int _currentPage = 0;
        private int TotalPages => Mathf.CeilToInt((float)LevelManager.Instance.TotalLevels / SlotsPerPage);

        // ──────────────────────────────────────────────────────────────────────
        // API pública
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>Refresca el panel con el progreso actual. Llamar al abrir el panel.</summary>
        public void Refresh()
        {
            _currentPage = 0;
            ShowPage(_currentPage);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Botones de navegación — conectar en Inspector
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>Página anterior.</summary>
        public void OnPrevPressed()
        {
            AudioManager.Instance?.PlayButtonTap();
            _currentPage--;
            ShowPage(_currentPage);
        }

        /// <summary>Página siguiente.</summary>
        public void OnNextPressed()
        {
            AudioManager.Instance?.PlayButtonTap();
            _currentPage++;
            ShowPage(_currentPage);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Lógica de paginación
        // ──────────────────────────────────────────────────────────────────────

        private void ShowPage(int page)
        {
            if (LevelManager.Instance == null || _slots == null)
            {
                Debug.LogWarning("[LevelSelectController] LevelManager no encontrado. ¿Está en BootScene?");
                return;
            }

            int total      = LevelManager.Instance.TotalLevels;
            int startIndex = page * SlotsPerPage;

            for (int i = 0; i < _slots.Length; i++)
            {
                int levelIndex = startIndex + i;
                if (levelIndex >= total)
                {
                    _slots[i].Hide();
                    continue;
                }

                bool unlocked = LevelManager.Instance.IsUnlocked(levelIndex);
                int  stars    = SaveManager.Instance != null
                    ? SaveManager.Instance.Data.levels[levelIndex].stars
                    : 0;

                _slots[i].Setup(levelIndex, stars, unlocked, OnLevelSelected);
            }

            _pageLabel.text = $"{page + 1} / {TotalPages}";
            _prevButton.interactable = page > 0;
            _nextButton.interactable = page < TotalPages - 1;
        }

        private void OnLevelSelected(int levelIndex)
        {
            AudioManager.Instance?.PlayButtonTap();
            LevelManager.Instance.SetLevel(levelIndex);
            SceneManager.LoadScene(_gameSceneName);
        }
    }
}
