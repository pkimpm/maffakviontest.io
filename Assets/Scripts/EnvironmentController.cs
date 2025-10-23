using UnityEngine;

public class EnvironmentController : MonoBehaviour
{
    [Header("Состояния окружения")]
    [SerializeField] private EnvironmentState[] states;

    private int currentIndex = -1;

    [System.Serializable]
    public struct EnvironmentState
    {
        public GameObject sceneRoot;
        public Material skyboxMaterial;
    }

    private void Awake()
    {
        foreach (var state in states)
            if (state.sceneRoot != null)
                state.sceneRoot.SetActive(false);
    }

    public void SetState(int index)
    {
        if (index < 0 || index >= states.Length || states[index].sceneRoot == null)
        {
            return;
        }

        if (currentIndex != -1 && states[currentIndex].sceneRoot != null)
            states[currentIndex].sceneRoot.SetActive(false);

        states[index].sceneRoot.SetActive(true);
        RenderSettings.skybox = states[index].skyboxMaterial;
        DynamicGI.UpdateEnvironment();

        currentIndex = index;
    }

    public int GetCurrentStateIndex() => currentIndex;
}