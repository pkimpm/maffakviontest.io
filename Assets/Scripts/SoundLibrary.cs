using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
}

[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Audio/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    public Sound[] music;
    public Sound[] ambience;
    public Sound[] ui;
    public Sound[] sfx;
    public Sound[] footsteps;

    private Dictionary<string, AudioClip> _soundDictionary;

    public void Initialize()
    {
        _soundDictionary = new Dictionary<string, AudioClip>();
        AddToDictionary(music);
        AddToDictionary(ambience);
        AddToDictionary(ui);
        AddToDictionary(sfx);
        AddToDictionary(footsteps);
    }

    private void AddToDictionary(Sound[] sounds)
    {
        foreach (var sound in sounds)
        {
            if (!_soundDictionary.ContainsKey(sound.name))
            {
                _soundDictionary.Add(sound.name, sound.clip);
            }
        }
    }

    public AudioClip GetClip(string name)
    {
        if (_soundDictionary.TryGetValue(name, out AudioClip clip))
        {
            return clip;
        }
        return null;
    }
}