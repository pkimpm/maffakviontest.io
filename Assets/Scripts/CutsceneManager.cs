using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance;

    [System.Serializable]
    public class CutsceneEntry
    {
        public string id;
        public PlayableDirector director;
    }

    [SerializeField] private List<CutsceneEntry> cutscenes = new List<CutsceneEntry>();
    private Dictionary<string, PlayableDirector> cutsceneMap = new Dictionary<string, PlayableDirector>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        cutsceneMap.Clear();
        foreach (var entry in cutscenes)
        {
            if (entry != null && entry.director != null && !string.IsNullOrEmpty(entry.id))
                cutsceneMap[entry.id] = entry.director;
        }
    }

    public PlayableDirector PlayCutscene(string id)
    {
        if (cutsceneMap.TryGetValue(id, out var director))
        {
            director.time = 0;
            director.Evaluate();
            director.Play();
            return director;
        }
        return null;
    }

    public PlayableDirector GetDirector(string id)
    {
        cutsceneMap.TryGetValue(id, out var director);
        return director;
    }
}