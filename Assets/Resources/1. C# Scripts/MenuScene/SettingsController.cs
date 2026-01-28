using UnityEngine;
using NovaSamples.UIControls;

public class SettingsController : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    void Start()
    {
        // Initialize audio settings
        masterSlider.Value = PlayerPrefs.GetFloat("masterVolume", 1.0f);
        musicSlider.Value = PlayerPrefs.GetFloat("musicVolume", 1.0f);
        sfxSlider.Value = PlayerPrefs.GetFloat("sfxVolume", 1.0f);
    }

    public void OnMasterVolumeChanged()
    {
        audioManager.SetMasterVolume(masterSlider.Value);
    }

    public void OnMusicVolumeChanged()
    {
        audioManager.SetMusicVolume(musicSlider.Value);
    }

    public void OnSFXVolumeChanged()
    {
        audioManager.SetSFXVolume(sfxSlider.Value);
    }
}
