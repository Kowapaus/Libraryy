using UnityEngine;
using UnityEngine.UI;

public class UIVisualizerDebug : MonoBehaviour
{
    public RectTransform content;

    void Start()
    {
        if (content == null)
        {
            Debug.LogError("UIVisualizerDebug: content не задан.");
            return;
        }

        Debug.Log("Content child count = " + content.childCount);

        for (int i = 0; i < content.childCount; i++)
        {
            var child = content.GetChild(i) as RectTransform;
            Debug.Log($"Child {i}: name={child.name}, active={child.gameObject.activeSelf}, localScale={child.localScale}, anchoredPos={child.anchoredPosition}, rect.size={child.rect.size}");

            // ¬ременно добавим светлый полупрозрачный фон, чтобы увидеть элемент
            var img = child.GetComponent<Image>();
            if (img == null) img = child.gameObject.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.15f); // очень бледный фон
        }
    }
}
