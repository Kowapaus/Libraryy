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

    [Header("References")]
    [SerializeField] private ShopManager shop;

    public IReadOnlyList<Book> Inventory => inventory;

    private void Awake()
    {
        // Fallback in case the reference is not set in the Inspector
        if (shop == null)
            shop = FindFirstObjectByType<ShopManager>();
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

        // Store reference to the ScriptableObject; duplicates are allowed
        inventory.Add(book);

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
            {
                item.SellClicked -= OnSellClicked;
                Destroy(item.gameObject);
            }
        }

        spawnedItems.Clear();
    }

    private void OnSellClicked(Book book)
    {
        if (!inventory.Remove(book))
            return;

        if (shop == null)
            return;

        float refund = book.Price * SELL_REFUND_PERCENT;

        shop.AddToBalance(refund, $"Продано: {book.Title} (+{refund:0.00})");

        Populate();
    }
    #endregion
}