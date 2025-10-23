using UnityEngine;
using UnityEngine.UI;

public class UIPanelManager : MonoBehaviour
{
    [Header("Панели и Управление")]
    [Tooltip("Панель с настройками звука, кнопкой 'Закрыть' и т.д.")]
    [SerializeField] private CanvasGroup settingsPanel;
    [Tooltip("Все остальные игровые панели (диалоги, подсказки), которые нужно скрыть при паузе.")]
    [SerializeField] private CanvasGroup[] gameUIPanels;

    [Header("Кнопки")]
    [Tooltip("Кнопка 'Настройки' в основном интерфейсе")]
    [SerializeField] private Button settingsButton;
    [Tooltip("Кнопка 'Закрыть' внутри панели настроек")]
    [SerializeField] private Button closeSettingsButton;
    [Tooltip("Кнопка 'Mute' в основном интерфейсе")]
    [SerializeField] private Button muteButton;

    [Header("Компоненты для Mute")]
    [SerializeField] private AudioSettingsUI audioSettings;
    [SerializeField] private Image muteButtonImage;
    [SerializeField] private Color muteActiveColor = Color.red;

    private Color _originalMuteButtonColor;
    private bool _isMuted = false;
    private float _lastMasterVolume;
    private bool _isPanelOpen = false;

    void Awake()
    {
        // Скрываем панель настроек при старте
        settingsPanel.alpha = 0f;
        settingsPanel.interactable = false;
        settingsPanel.blocksRaycasts = false;
        
        // Сохраняем оригинальный цвет кнопки Mute
        if (muteButtonImage != null)
        {
            _originalMuteButtonColor = muteButtonImage.color;
        }

        // Назначаем события на кнопки
        settingsButton.onClick.AddListener(ToggleSettingsPanel);
        closeSettingsButton.onClick.AddListener(ToggleSettingsPanel);
        muteButton.onClick.AddListener(ToggleMute);
    }

    void OnDestroy()
    {
        // Отписываемся от событий
        settingsButton.onClick.RemoveListener(ToggleSettingsPanel);
        closeSettingsButton.onClick.RemoveListener(ToggleSettingsPanel);
        muteButton.onClick.RemoveListener(ToggleMute);
    }

    public void ToggleSettingsPanel()
    {
        _isPanelOpen = !_isPanelOpen;

        if (_isPanelOpen)
        {
            // Открываем панель и ставим паузу
            Time.timeScale = 0f;
            settingsPanel.alpha = 1f;
            settingsPanel.interactable = true;
            settingsPanel.blocksRaycasts = true;
            SetGameUIVisibility(false); // Скрываем игровой UI
        }
        else
        {
            // Закрываем панель и снимаем паузу
            Time.timeScale = 1f;
            settingsPanel.alpha = 0f;
            settingsPanel.interactable = false;
            settingsPanel.blocksRaycasts = false;
            SetGameUIVisibility(true); // Показываем игровой UI
        }
    }

    // Вспомогательный метод для управления видимостью всех игровых панелей
    private void SetGameUIVisibility(bool visible)
    {
        foreach (var panel in gameUIPanels)
        {
            if (panel != null)
            {
                panel.alpha = visible ? 1f : 0f;
                panel.interactable = visible;
                panel.blocksRaycasts = visible;
            }
        }
    }

    public void ToggleMute()
    {
        _isMuted = !_isMuted;

        if (_isMuted)
        {
            _lastMasterVolume = audioSettings.masterSlider.value;
            audioSettings.OnMasterVolumeChanged(0.0001f);
            audioSettings.masterSlider.value = 0.0001f;
            if (muteButtonImage != null) muteButtonImage.color = muteActiveColor;
        }
        else
        {
            audioSettings.OnMasterVolumeChanged(_lastMasterVolume);
            audioSettings.masterSlider.value = _lastMasterVolume;
            if (muteButtonImage != null) muteButtonImage.color = _originalMuteButtonColor;
        }
    }
}