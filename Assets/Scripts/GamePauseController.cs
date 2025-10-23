using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class GamePauseController : MonoBehaviour
{
    [Header("Главные элементы управления")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button openSettingsButton;
    [SerializeField] private Button closeSettingsButton;
    [SerializeField] private Button muteButton;
    [SerializeField] private Button exitButton;

    [Header("Диалог подтверждения выхода")]
    [SerializeField] private ConfirmExitPanel confirmExitPanel;

    [Header("UI для управления")]
    [SerializeField] private List<GameObject> uiElementsToManage;

    [Header("Слайдеры громкости")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider voiceVolumeSlider;
    [SerializeField] private Slider vfxVolumeSlider;

    [Header("Настройки кнопки Mute")]
    [SerializeField] private Image muteButtonImage;
    [SerializeField] private Color muteActiveColor = Color.red;

    [Header("Звук нажатия кнопки")]
    [SerializeField] private AudioClip menuClickSound;

    [Header("Затемнение для выхода")]
    [SerializeField] private CanvasGroup blackoutPanel;
    [SerializeField] private float blackoutDuration = 1.5f;

    private List<GameObject> _activeUiBeforePause;
    private AudioSource _uiAudioSource;
    private bool isMuted = false;
    private Color originalMuteColor;
    private bool isPaused = false;
    private bool _dialogueContinueButtonWasInteractable = false;

    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string VoiceVolumeKey = "VoiceVolume";
    private const string VfxVolumeKey = "VfxVolume";

    private void Awake()
    {
        _uiAudioSource = GetComponent<AudioSource>();
        _uiAudioSource.ignoreListenerPause = true;
        _activeUiBeforePause = new List<GameObject>();

        if (blackoutPanel != null)
        {
            blackoutPanel.alpha = 0f;
            blackoutPanel.blocksRaycasts = false;
        }
    }

    private void Start()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        isMuted = false;

        if (muteButtonImage != null)
        {
            originalMuteColor = muteButtonImage.color;
        }

        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        if (voiceVolumeSlider != null) voiceVolumeSlider.onValueChanged.AddListener(OnVoiceVolumeChanged);
        if (vfxVolumeSlider != null) vfxVolumeSlider.onValueChanged.AddListener(OnVfxVolumeChanged);

        if (openSettingsButton != null) openSettingsButton.onClick.AddListener(PauseGame);
        if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(ResumeGame);
        if (muteButton != null) muteButton.onClick.AddListener(ToggleMute);
        if (exitButton != null) exitButton.onClick.AddListener(ShowConfirmExit);
    }

    private void OnDestroy()
    {
        if (openSettingsButton != null) openSettingsButton.onClick.RemoveListener(PauseGame);
        if (closeSettingsButton != null) closeSettingsButton.onClick.RemoveListener(ResumeGame);
        if (muteButton != null) muteButton.onClick.RemoveListener(ToggleMute);
        if (exitButton != null) exitButton.onClick.RemoveListener(ShowConfirmExit);

        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        if (voiceVolumeSlider != null) voiceVolumeSlider.onValueChanged.RemoveListener(OnVoiceVolumeChanged);
        if (vfxVolumeSlider != null) vfxVolumeSlider.onValueChanged.RemoveListener(OnVfxVolumeChanged);
    }

    private void PauseGame()
    {
        if (isPaused) return;
        isPaused = true;
        
        PlayMenuSound();
        SaveDialogueContinueButtonState();

        _activeUiBeforePause.Clear();
        foreach (var ui in uiElementsToManage)
        {
            if (ui != null && ui.activeSelf)
            {
                _activeUiBeforePause.Add(ui);
                ui.SetActive(false);
            }
        }

        Time.timeScale = 0f;
        AudioListener.pause = true;

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }

        UpdateSlidersWithCurrentVolume();
    }

    private void ResumeGame()
    {
        if (!isPaused) return;
        isPaused = false;
        
        PlayMenuSound();

        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        foreach (var ui in _activeUiBeforePause)
        {
            if (ui != null)
            {
                ui.SetActive(true);
            }
        }
        _activeUiBeforePause.Clear();

        RestoreDialogueContinueButtonState();
    }

    private void SaveDialogueContinueButtonState()
    {
        var dialogueView = FindObjectOfType<DialogueView>();
        if (dialogueView != null)
        {
            var continueButton = dialogueView.GetComponentInChildren<Button>();
            if (continueButton != null)
            {
                _dialogueContinueButtonWasInteractable = continueButton.interactable;
            }
        }
    }

    private void RestoreDialogueContinueButtonState()
    {
        var dialogueView = FindObjectOfType<DialogueView>();
        if (dialogueView != null)
        {
            var continueButton = dialogueView.GetComponentInChildren<Button>();
            if (continueButton != null)
            {
                if (!DialogueManager.IsDialogueActive)
                {
                    return;
                }
                
                continueButton.interactable = _dialogueContinueButtonWasInteractable;
            }
        }
    }

    private void ToggleMute()
    {
        PlayMenuSound();

        isMuted = !isMuted;

        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>(true);
        
        foreach (AudioSource source in allAudioSources)
        {
            if (source == _uiAudioSource) continue;
            
            source.mute = isMuted;
        }

        if (muteButtonImage != null)
        {
            muteButtonImage.color = isMuted ? muteActiveColor : originalMuteColor;
        }
    }

    private void ShowConfirmExit()
    {
        PlayMenuSound();

        if (confirmExitPanel != null)
        {
            confirmExitPanel.OnConfirm = () => StartCoroutine(ExitGame());
            confirmExitPanel.OnCancel = () => PlayMenuSound();
            confirmExitPanel.Show();
        }
        else
        {
            StartCoroutine(ExitGame());
        }
    }

    private IEnumerator ExitGame()
    {
        PlayMenuSound();

        if (blackoutPanel != null)
        {
            blackoutPanel.transform.SetAsLastSibling();
            blackoutPanel.blocksRaycasts = true;

            LeanTween.cancel(blackoutPanel.gameObject);
            blackoutPanel.alpha = 0f;

            bool finished = false;
            LeanTween.alphaCanvas(blackoutPanel, 1f, blackoutDuration)
                .setEase(LeanTweenType.easeInOutQuad)
                .setIgnoreTimeScale(true)
                .setOnComplete(() => finished = true);

            yield return new WaitUntil(() => finished);
        }
        else
        {
            yield return new WaitForSecondsRealtime(0.5f);
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void UpdateSlidersWithCurrentVolume()
    {
        if (AudioManager.Instance != null)
        {
            if (masterVolumeSlider != null) masterVolumeSlider.value = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
            if (musicVolumeSlider != null) musicVolumeSlider.value = PlayerPrefs.GetFloat(MusicVolumeKey, 0.8f);
            if (voiceVolumeSlider != null) voiceVolumeSlider.value = PlayerPrefs.GetFloat(VoiceVolumeKey, 1f);
            if (vfxVolumeSlider != null) vfxVolumeSlider.value = PlayerPrefs.GetFloat(VfxVolumeKey, 1f);
        }
    }

    private void PlayMenuSound()
    {
        if (menuClickSound != null && _uiAudioSource != null)
        {
            _uiAudioSource.PlayOneShot(menuClickSound);
        }
    }

    private void OnMasterVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(value);
            PlayerPrefs.SetFloat(MasterVolumeKey, value);
        }
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
            PlayerPrefs.SetFloat(MusicVolumeKey, value);
        }
    }

    private void OnVoiceVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetVoiceVolume(value);
            PlayerPrefs.SetFloat(VoiceVolumeKey, value);
        }
    }

    private void OnVfxVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetVFXVolume(value);
            PlayerPrefs.SetFloat(VfxVolumeKey, value);
        }
    }
}