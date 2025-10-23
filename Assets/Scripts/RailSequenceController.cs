using Cinemachine;
using System;
using System.Collections;
using UnityEngine;

public class RailSequenceController : MonoBehaviour
{
    [Header("Камеры")]
    [SerializeField] public CinemachineVirtualCamera sequenceCamera;
    [SerializeField] private CinemachineDollyCart cart;

    [Header("Настройки")]
    [SerializeField] private float duration;
    [SerializeField] private float musicFadeDuration = 1f;
    [SerializeField] private GameObject objectToActivate;
    [SerializeField] private GameObject objectToDeactivate;

    [Header("Музыка")]
    [Tooltip("Имя музыки из SoundLibrary для проигрывания во время поездки")]
    [SerializeField] private string musicToPlay;

    public Action OnSequenceFinished;
    private bool _sequenceFinished;

    public IEnumerator PlaySequence()
    {
        _sequenceFinished = false;
        
        if (cart != null) cart.m_Position = 0; 

        if (objectToActivate != null) objectToActivate.SetActive(true);
        if (objectToDeactivate != null) objectToDeactivate.SetActive(false);

        if (!string.IsNullOrEmpty(musicToPlay))
        {
            AudioManager.Instance.CrossfadeMusic(musicToPlay, musicFadeDuration);
        }

        LeanTween.value(gameObject, 0, 1, duration)
            .setOnUpdate(val => { if (cart != null) cart.m_Position = val; })
            .setOnComplete(() => 
            {
                _sequenceFinished = true;
                OnSequenceFinished?.Invoke();
            });
            
        yield return new WaitUntil(() => _sequenceFinished);
    }
}