using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioMixer audioMixer;
    [Header("MUSIC")]
    [SerializeField] private List<string> musicKeyList;
    [SerializeField] private List<AudioClip> musicAudioClipList;
    [SerializeField] private AudioMixerGroup musicGroup;
    [Header("SFX")]
    [SerializeField] private List<string> sfxKeyList;
    [SerializeField] private List<AudioClip> sfxAudioClipList;
    [SerializeField] private AudioMixerGroup sfxGroup;
    [SerializeField] private int sfxPoolSize = 10;

    private Dictionary<string, AudioClip> musicDictionary = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioClip> sfxDictionary = new Dictionary<string, AudioClip>();
    private AudioSource musicSource;
    private List<AudioSource> sfxPool;
    private AudioSource currentSFXLooping; // Hold the current SFX that is looping

    private void Awake()
    {
        if(Instance == null) Instance = this;
        else{
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        InitDictionairies();
        InitMusicSource();
        InitSFXPool();
        LoadVolumeSettings();
    }

    void InitDictionairies()
    {
        // Check if all the list is inputed correctly
        if (musicKeyList.Count != musicAudioClipList.Count)
        {
            Debug.Log("musicKeyList and musicAudioClipList does not match");
            return;
        }
        if (sfxKeyList.Count != sfxAudioClipList.Count)
        {
            Debug.Log("sfxKeyList and sfxAudioClipList does not match");
            return;
        }
        // Match all list to the dictionary
        for(int index = 0; index < musicKeyList.Count; index++)
        {
            musicDictionary.Add(musicKeyList[index], musicAudioClipList[index]);
        }
        for(int index = 0; index < sfxKeyList.Count; index++)
        {
            sfxDictionary.Add(sfxKeyList[index], sfxAudioClipList[index]);
        }
    }

    void InitMusicSource()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.outputAudioMixerGroup = musicGroup;
        musicSource.loop = true;
    }

    void InitSFXPool()
    {
        sfxPool = new List<AudioSource>();
        for (int i = 0; i < sfxPoolSize; i++)
        {
            AudioSource src = gameObject.AddComponent<AudioSource>();
            src.outputAudioMixerGroup = sfxGroup;
            src.playOnAwake = false;
            sfxPool.Add(src);
        }
    }

    public void PlayMusic(string key)
    {
        // Check if key exist
        if (!musicDictionary.ContainsKey(key))
        {
            Debug.Log("Music key: " + key + ", does not exist");
            return;
        }
        // Play music
        musicSource.clip = musicDictionary[key];
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    AudioSource GetAvailableSFXSource()
    {
        foreach (AudioSource src in sfxPool)
        {
            if (!src.isPlaying)
                return src;
        }
        return sfxPool[0];
    }

    public void PlaySFX(string key)
    {
        // Check if key exist
        if (!sfxDictionary.ContainsKey(key))
        {
            Debug.Log("SFX key: " + key + ", does not exist");
            return;
        }
        // Play SFX
        AudioSource src = GetAvailableSFXSource();
        src.clip = sfxDictionary[key];
        src.loop = false;
        src.Play();
    }

    // Plays looping SFX if there is no current looping SFX
    public void PlayLoopingSFX(string key, bool overrideCurrent)
    {
        // Check if key exist
        if (!musicDictionary.ContainsKey(key))
        {
            Debug.Log("SFX key: " + key + ", does not exist");
            return;
        }
        // Check if there is a looping SFX and not overriding
        if (currentSFXLooping && !overrideCurrent) return;
        // Play SFX
        AudioSource src = GetAvailableSFXSource();
        src.clip = sfxDictionary[key];
        src.loop = true;
        currentSFXLooping = src; // Tracks the looping SFX
        src.Play();
    }

    // Stops any looping SFX
    public void StopLoopingSFX()
    {
        if(currentSFXLooping) currentSFXLooping.Stop();
        currentSFXLooping = null;
    }

    public void StopAllSFX()
    {
        foreach (AudioSource src in sfxPool)
            src.Stop();
    }

    public void LoadVolumeSettings()
    {
        float masterVol = PlayerPrefs.GetFloat("masterVolume", 1f);
        float musicVol = PlayerPrefs.GetFloat("musicVolume", 1f);
        float sfxVol = PlayerPrefs.GetFloat("sfxVolume", 1f);

        float master = Mathf.Log10(Mathf.Clamp(masterVol, 0.0001f, 1f)) * 20f;
        float music = Mathf.Log10(Mathf.Clamp(musicVol, 0.0001f, 1f)) * 20f;
        float sfx = Mathf.Log10(Mathf.Clamp(sfxVol, 0.0001f, 1f)) * 20f;

        audioMixer.SetFloat("MasterVolume", master);
        audioMixer.SetFloat("MusicVolume", music);
        audioMixer.SetFloat("SFXVolume", sfx);
    }

    public void SetMasterVolume(float value)
    {
        float v = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat("MasterVolume", v);
        PlayerPrefs.SetFloat("masterVolume", value);
    }

    public void SetMusicVolume(float value)
    {
        float v = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat("MusicVolume", v);
        PlayerPrefs.SetFloat("musicVolume", value);
    }

    public void SetSFXVolume(float value)
    {
        float v = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat("SFXVolume", v);
        PlayerPrefs.SetFloat("sfxVolume", value);
    }

}
