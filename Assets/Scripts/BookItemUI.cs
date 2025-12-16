using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class BookItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button sellButton;

    public event Action<Book> BuyClicked;
    public event Action<Book> SellClicked;

    public void Setup(Book book, bool isShop)
    {
        if (book == null)
            return;

        titleText.text = book.Title;
        priceText.text = book.Price.ToString("0.00");

        buyButton.gameObject.SetActive(isShop);
        sellButton.gameObject.SetActive(!isShop);

        buyButton.onClick.RemoveAllListeners();
        sellButton.onClick.RemoveAllListeners();

        if (isShop)
            buyButton.onClick.AddListener(() => BuyClicked?.Invoke(book));
        else
            sellButton.onClick.AddListener(() => SellClicked?.Invoke(book));
    }

    private void OnDestroy()
    {
        if (buyButton != null)
            buyButton.onClick.RemoveAllListeners();

        if (sellButton != null)
            sellButton.onClick.RemoveAllListeners();
    }
}