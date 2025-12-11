using UnityEngine;
using System.Collections;

public class InventoryPanelController : MonoBehaviour
{
    public RectTransform panel;       
    public float slideTime = 0.3f;    
    public Vector2 hiddenPosition;     
    public Vector2 shownPosition;    

    private bool isShown = false;

    void Start()
    {
        if (panel != null)
            panel.anchoredPosition = hiddenPosition; 
    }

    public void ToggleInventory()
    {
        StopAllCoroutines();
        StartCoroutine(SlidePanel(isShown ? hiddenPosition : shownPosition));
        isShown = !isShown;
    }

    private IEnumerator SlidePanel(Vector2 target)
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
    }
}
