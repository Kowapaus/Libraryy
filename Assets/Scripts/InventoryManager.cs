using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    private const float SELL_REFUND_PERCENT = 0.5f;

    [Header("UI")]
    [SerializeField] private RectTransform content;
    [SerializeField] private BookItemUI bookItemPrefab;

    private readonly List<Book> inventory = new();
    private readonly List<BookItemUI> spawnedItems = new();

    private BookListManager shop;

    public IReadOnlyList<Book> Inventory => inventory;

    private void Awake()
    {
        shop = FindFirstObjectByType<BookListManager>();
    }

    private void Start()
    {
        Populate();
    }

    #region Inventory Logic

    public void AddBook(Book book)
    {
        if (book == null)
            return;

        // создаём копию, чтобы покупки были независимыми
        inventory.Add(new Book
        {
            Title = book.Title,
            Price = book.Price
        });

        Populate();
    }

    public void SetInventory(List<Book> books)
    {
        inventory.Clear();
        inventory.AddRange(books);
        Populate();
    }

    public void Clear()
    {
        inventory.Clear();
        ClearItems();
    }

    #endregion

    #region UI

    private void Populate()
    {
        ClearItems();

        foreach (var book in inventory)
        {
            var item = Instantiate(bookItemPrefab, content);
            item.Setup(book, isShop: false);
            item.SellClicked += OnSellClicked;

            spawnedItems.Add(item);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    private void ClearItems()
    {
        foreach (var item in spawnedItems)
        {
            if (item != null)
                Destroy(item.gameObject);
        }

        spawnedItems.Clear();
    }

    private void OnSellClicked(Book book)
    {
        if (!inventory.Remove(book))
            return;

        float refund = book.Price * SELL_REFUND_PERCENT;

        shop.AddToBalance(refund, $"Продано: {book.Title} (+{refund:0.00})");

        Populate();
    }

    #endregion
}
