using Cinemachine;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

[Serializable]
public class RoomContent
{
    public string roomName;
    public GameObject root;
    public CinemachineVirtualCamera camera;
    
    [Tooltip("Имя музыки из SoundLibrary")]
    public string musicName;

    [Header("Диалоги")]
    public DialogueNode beforeDialogue;
    public DialogueNode afterDialogue;

    [Header("Геймплей")]
    public PuzzleManager puzzle;
    public PuzzleInput puzzleInput;
    public RailSequenceController railSequence;

    [Header("Финальный бой")]
    public FinalBattleManager finalBattle;
}

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    [Header("Основные компоненты")]
    [SerializeField] private LoadingScreen loadingScreen;
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private FinalScreenManager finalScreen;
    [SerializeField] private EnvironmentController environmentController;
    
    [Header("Настройки")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private RoomContent[] rooms;
    
    private int _currentRoomIndex = -1;
    private CinemachineVirtualCamera _lastActiveCamera;
    private bool wasAfterDialoguePlayed = false;

    private void Awake()
    {
        ResetAllTimelines();
        InitializeInputsOnly();
        
        if (loadingScreen != null) 
        { 
            loadingScreen.gameObject.SetActive(true); 
            loadingScreen.OnLoadingFinished += OnLoadingFinished; 
        }
        else 
        { 
            StartCoroutine(StartWithEnvironment()); 
        }
    }

    private void OnDestroy() 
    { 
        if (loadingScreen != null) 
            loadingScreen.OnLoadingFinished -= OnLoadingFinished; 
    }

    private void ResetAllTimelines()
    {
        PlayableDirector[] allDirectors = FindObjectsOfType<PlayableDirector>(true);
        
        foreach (var director in allDirectors)
        {
            if (director != null && director.playableAsset != null)
            {
                director.time = 0;
                director.Evaluate();
                director.Stop();
            }
        }
    }

    private void InitializeInputsOnly()
    {
        if (rooms == null) return;
        
        foreach (var room in rooms)
        {
            if (room.puzzleInput != null) 
            {
                room.puzzleInput.enabled = false;
            }
        }
        
        PuzzleInput[] allInputs = FindObjectsOfType<PuzzleInput>(true);
        foreach (var input in allInputs) 
        {
            input.enabled = false;
        }
    }

    private void OnLoadingFinished() 
    { 
        StartCoroutine(StartWithEnvironment()); 
    }
    
    private IEnumerator StartWithEnvironment() 
    { 
        environmentController?.SetState(0); 
        DynamicGI.UpdateEnvironment(); 
        
        yield return null;
        
        StartCoroutine(GameFlow()); 
    }

    private IEnumerator GameFlow()
    {
        for (_currentRoomIndex = 0; _currentRoomIndex < rooms.Length; _currentRoomIndex++)
        {
            var room = rooms[_currentRoomIndex];

            ActivateRoom(room);

            if (_lastActiveCamera != null && _lastActiveCamera != room.camera)
            {
                _lastActiveCamera.Priority = 0;
            }
            _lastActiveCamera = room.camera;

            yield return null;

            yield return PlayDialogue(room.beforeDialogue);
            
            if (room.puzzle != null)
            {
                if (room.puzzleInput != null) room.puzzleInput.enabled = true;
                yield return room.puzzle.PlayPuzzle();
                if (room.puzzleInput != null) room.puzzleInput.enabled = false;
            }

            yield return PlayOptionalDialogue(room.afterDialogue);

            if (room.railSequence != null)
            {
                if (room.camera != null) room.camera.Priority = 0;

                if (room.railSequence.sequenceCamera != null)
                {
                    room.railSequence.sequenceCamera.Priority = 20;
                    _lastActiveCamera = room.railSequence.sequenceCamera;
                }

                yield return null;
                yield return room.railSequence.PlaySequence();
            }
        }
        
        if (finalScreen != null) finalScreen.Show();
    }
    
    private void ActivateRoom(RoomContent room)
    {
        wasAfterDialoguePlayed = false;
        
        if (room.root != null) 
        {
            room.root.SetActive(true);
        }
        
        if (room.camera != null) room.camera.Priority = 10;
        if (room.puzzleInput != null) room.puzzleInput.enabled = false;
        
        if (!string.IsNullOrEmpty(room.musicName))
        {
            AudioManager.Instance.CrossfadeMusic(room.musicName, fadeDuration);
        }
    }

    private IEnumerator PlayOptionalDialogue(DialogueNode node)
    {
        if (node == null || wasAfterDialoguePlayed) yield break;
        AudioManager.Instance.FadeMusicVolume(AudioManager.Instance.dialogueMusicVolume, fadeDuration * 0.5f);
        yield return PlayDialogue(node);
        AudioManager.Instance.FadeMusicVolume(AudioManager.Instance.defaultMusicVolume, fadeDuration * 0.5f);
    }
    
    private IEnumerator PlayDialogue(DialogueNode node)
    {
        if (node == null) yield break;
        
        while (HintManager.Instance != null && HintManager.Instance.IsHintOpen) yield return null;

        var finished = false;
        UnityAction onEnd = () => finished = true;

        dialogueManager.OnDialogueEnd += onEnd;
        dialogueManager.StartDialogue(node);

        yield return new WaitUntil(() => finished);

        dialogueManager.OnDialogueEnd -= onEnd;
    }

    public void TriggerDialogue(DialogueNode node)
    {
        if (node == null) return;
        if (_currentRoomIndex >= 0 && _currentRoomIndex < rooms.Length)
        {
            if (rooms[_currentRoomIndex].afterDialogue == node) wasAfterDialoguePlayed = true;
        }
        StartCoroutine(PlayDialogue(node));
    }

    public FinalBattleManager GetCurrentBattleManager()
    {
        if (_currentRoomIndex >= 0 && _currentRoomIndex < rooms.Length)
        {
            return rooms[_currentRoomIndex].finalBattle;
        }
        return null;
    }
}