using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("Слайдеры")]
    public Slider masterSlider; 
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider voiceSlider;
    [SerializeField] private Slider vfxSlider;

    private const string MasterVolumeKey = "MasterVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string VoiceVolumeKey = "VoiceVolume";
    private const string VfxVolumeKey = "VfxVolume";

    private void Start()
    {
        masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        voiceSlider.onValueChanged.AddListener(OnVoiceVolumeChanged);
        vfxSlider.onValueChanged.AddListener(OnVfxVolumeChanged);
        LoadSettings();
    }

    private void OnDestroy()
    {
        masterSlider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        voiceSlider.onValueChanged.RemoveListener(OnVoiceVolumeChanged);
        vfxSlider.onValueChanged.RemoveListener(OnVfxVolumeChanged);
    }

    private void LoadSettings()
    {
        float masterVol = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        masterSlider.value = masterVol;
        OnMasterVolumeChanged(masterVol);

        float musicVol = PlayerPrefs.GetFloat(MusicVolumeKey, 0.8f);
        musicSlider.value = musicVol;
        OnMusicVolumeChanged(musicVol);

        float voiceVol = PlayerPrefs.GetFloat(VoiceVolumeKey, 1f);
        voiceSlider.value = voiceVol;
        OnVoiceVolumeChanged(voiceVol);

        float vfxVol = PlayerPrefs.GetFloat(VfxVolumeKey, 1f);
        vfxSlider.value = vfxVol;
        OnVfxVolumeChanged(vfxVol);
    }

    public void OnMasterVolumeChanged(float value)
    {
        AudioManager.Instance.SetMasterVolume(value);
        PlayerPrefs.SetFloat(MasterVolumeKey, value);
    }

    public void OnMusicVolumeChanged(float value)
    {
        AudioManager.Instance.SetMusicVolume(value);
        PlayerPrefs.SetFloat(MusicVolumeKey, value);
    }
    
    public void OnVoiceVolumeChanged(float value)
    {
        AudioManager.Instance.SetVoiceVolume(value);
        PlayerPrefs.SetFloat(VoiceVolumeKey, value);
    }

    public void OnVfxVolumeChanged(float value)
    {
        AudioManager.Instance.SetVFXVolume(value);
        PlayerPrefs.SetFloat(VfxVolumeKey, value);
    }
}