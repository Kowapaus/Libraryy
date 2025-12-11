using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class BookItemUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI priceText;
    public Button buyButton;
    public Button sellButton;

    private Book book;
    private Action<Book, GameObject> onBuy;
    private Action<Book, GameObject> onSell;

    public void Setup(Book b, Action<Book, GameObject> onBuyCallback)
    {
        book = b;
        onBuy = onBuyCallback;

        if (titleText != null) titleText.text = b.title;
        if (priceText != null) priceText.text = b.price.ToString("0.00");

        if (buyButton != null)
        {
            buyButton.gameObject.SetActive(true);
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => onBuy?.Invoke(book, gameObject));
        }

        if (sellButton != null) sellButton.gameObject.SetActive(false);
    }

    public void SetupInventory(Book b, Action<Book, GameObject> onSellCallback)
    {
        book = b;
        onSell = onSellCallback;

        if (titleText != null) titleText.text = b.title;
        if (priceText != null) priceText.text = b.price.ToString("0.00");

        if (buyButton != null) buyButton.gameObject.SetActive(false);

        if (sellButton != null)
        {
            sellButton.gameObject.SetActive(true);
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(() => onSell?.Invoke(book, gameObject));
        }
    }
}
