using UnityEngine;
using UnityEngine.Rendering; // <-- ВОТ ЭТА СТРОКА БЫЛА ПРОПУЩЕНА

// Этот скрипт поможет нам узнать точные имена свойств материала.
public class MaterialPropertyDebugger : MonoBehaviour
{
    // Чтобы запустить этот метод, кликните правой кнопкой мыши
    // по компоненту MaterialPropertyDebugger в инспекторе и выберите "Log Material Properties".
    [ContextMenu("Log Material Properties")]
    private void LogMaterialProperties()
    {
        Renderer rend = GetComponentInChildren<Renderer>();
        if (rend == null)
        {
            Debug.LogError("На этом объекте и его дочерних объектах не найден Renderer!", this);
            return;
        }

        Material material = rend.sharedMaterial;
        if (material == null)
        {
            Debug.LogError("На Renderer не назначен материал!", this);
            return;
        }

        Shader shader = material.shader;
        Debug.Log($"--- Свойства материала '{material.name}' (Шейдер: '{shader.name}') ---", this);

        for (int i = 0; i < shader.GetPropertyCount(); i++)
        {
            string propertyName = shader.GetPropertyName(i);
            ShaderPropertyType propertyType = shader.GetPropertyType(i);
            
            // Нас интересуют только свойства типа "Цвет"
            if (propertyType == ShaderPropertyType.Color)
            {
                Debug.Log($"Найдено свойство цвета: Имя = '{propertyName}', Тип = {propertyType}", this);
            }
        }
        
        Debug.Log("--- Конец списка свойств ---", this);
    }
}