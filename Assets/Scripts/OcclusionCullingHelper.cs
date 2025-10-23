using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Помогает настроить Unity Occlusion Culling автоматически
/// </summary>
public class OcclusionCullingHelper : MonoBehaviour
{
    [Header("Occlusion Settings")]
    [Tooltip("Минимальный размер объекта для Occluder")]
    public float minOccluderSize = 5f;
    
    [Tooltip("Минимальный размер объекта для Occludee")]
    public float minOccludeeSize = 1f;
    
    [Header("Auto Setup")]
    [Tooltip("Автоматически пометить статичные объекты")]
    public bool autoMarkStatic = true;
    
    [Tooltip("Искать в дочерних объектах")]
    public bool includeChildren = true;

#if UNITY_EDITOR
    [ContextMenu("Setup Occlusion Culling")]
    public void SetupOcclusionCulling()
    {
        Transform[] transforms = includeChildren 
            ? GetComponentsInChildren<Transform>() 
            : new Transform[] { transform };
        
        int occluderCount = 0;
        int occludeeCount = 0;
        int staticCount = 0;
        
        foreach (var t in transforms)
        {
            if (t == transform) continue;
            
            MeshRenderer renderer = t.GetComponent<MeshRenderer>();
            if (renderer == null) continue;
            
            // Получаем размер объекта
            Bounds bounds = renderer.bounds;
            float size = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            
            // Помечаем как static
            if (autoMarkStatic && !t.gameObject.isStatic)
            {
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, 
                    StaticEditorFlags.OccluderStatic | 
                    StaticEditorFlags.OccludeeStatic);
                staticCount++;
            }
            
            // Настраиваем Occluder/Occludee
            if (size >= minOccluderSize)
            {
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, 
                    GameObjectUtility.GetStaticEditorFlags(t.gameObject) | 
                    StaticEditorFlags.OccluderStatic);
                occluderCount++;
            }
            
            if (size >= minOccludeeSize)
            {
                GameObjectUtility.SetStaticEditorFlags(t.gameObject, 
                    GameObjectUtility.GetStaticEditorFlags(t.gameObject) | 
                    StaticEditorFlags.OccludeeStatic);
                occludeeCount++;
            }
        }
        
        Debug.Log($"Occlusion Culling Setup Complete!\n" +
                  $"Static: {staticCount} | Occluders: {occluderCount} | Occludees: {occludeeCount}");
    }
    
    [ContextMenu("Bake Occlusion Culling")]
    public void BakeOcclusion()
    {
        UnityEditor.StaticOcclusionCulling.Compute();
        Debug.Log("Occlusion Culling baked!");
    }
#endif
}