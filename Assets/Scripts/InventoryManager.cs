using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public RectTransform content;
    public GameObject bookItemPrefab;
    public List<Book> inventory = new List<Book>();

    void Start()
    {
        Populate();
    }

    public void Populate()
    {
        if (content == null || bookItemPrefab == null)
        {
            Debug.LogError("InventoryManager: Content или Prefab не назначены!");
            return;
        }

        foreach (Transform t in content) Destroy(t.gameObject);

        foreach (var b in inventory)
        {
            var go = Instantiate(bookItemPrefab, content);
            var ui = go.GetComponent<BookItemUI>();
            if (ui != null)
            {
                ui.SetupInventory(b, OnSell);
            }
        }

        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    public void AddBook(Book book)
    {
        if (book == null) return;

     
        Book newBook = new Book { title = book.title, price = book.price };
        inventory.Add(newBook);
        Populate();
    }

    public void OnSell(Book book, GameObject itemGO)
    {
        if (book == null) return;

        float refund = book.price * 0.5f;

        var store = FindFirstObjectByType<BookListManager>();
        if (store != null)
        {
            store.AddToBalance(refund, $"Продано: {book.title} (+{refund:0.00})");
        }
        else
        {
            Debug.LogWarning("InventoryManager: BookListManager не найден — возврат не выполнен.");
        }

        if (inventory.Contains(book)) inventory.Remove(book);
        if (itemGO != null) Destroy(itemGO);

        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }
}
