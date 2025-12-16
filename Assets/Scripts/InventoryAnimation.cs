using UnityEngine;
using System.Collections;

public class InventoryAnimation : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private RectTransform panel;

    [Header("Animation")]
    [SerializeField] private float slideTime = 0.3f;
    [SerializeField] private Vector2 hiddenPosition;
    [SerializeField] private Vector2 shownPosition;

    private bool isShown;
    private Coroutine slideCoroutine;

    private void Awake()
    {
        if (panel != null)
            panel.anchoredPosition = hiddenPosition;
    }

    public void ToggleInventory()
    {
        Vector2 targetPosition = isShown ? hiddenPosition : shownPosition;
        isShown = !isShown;

        if (slideCoroutine != null)
            StopCoroutine(slideCoroutine);

        slideCoroutine = StartCoroutine(SlideRoutine(targetPosition));
    }

    private IEnumerator SlideRoutine(Vector2 target)
    {
        Vector2 start = panel.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < slideTime)
        {
            elapsed += Time.deltaTime;
            panel.anchoredPosition = Vector2.Lerp(start, target, elapsed / slideTime);
            yield return null;
        }

        panel.anchoredPosition = target;
        slideCoroutine = null;
    }
}
