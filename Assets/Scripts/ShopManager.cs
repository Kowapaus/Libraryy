using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    private const float DefaultBalance = 3000f;
    private const float MessageDuration = 2f;

    [Header("UI")]
    [SerializeField] private RectTransform content;
    [SerializeField] private BookItemUI bookItemPrefab;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text balanceText;

    [Header("References")]
    [SerializeField] private InventoryManager inventoryManager;

    [Header("Data")]
    [SerializeField] private List<Book> books = new();
    [SerializeField] private float playerBalance = DefaultBalance;

    private readonly List<BookItemUI> spawnedItems = new();
    private Coroutine messageCoroutine;

    private string SaveFilePath =>
        Path.Combine(Application.persistentDataPath, "player_profile.dat");

    private void Start()
    {
        UpdateBalanceText();
        messageText.gameObject.SetActive(false);
        Populate();
    }

    #region Shop UI
    private void Populate()
    {
        ClearItems();

        foreach (var book in books)
        {
            var item = Instantiate(bookItemPrefab, content);
            item.Setup(book, isShop: true);
            item.BuyClicked += OnBuyClicked;

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
                item.BuyClicked -= OnBuyClicked;
                Destroy(item.gameObject);
            }
        }

        spawnedItems.Clear();
    }

    private void OnBuyClicked(Book book)
    {
        if (playerBalance < book.Price)
        {
            ShowMessage("Недостаточно средств!");
            return;
        }

        playerBalance -= book.Price;
        UpdateBalanceText();

        inventoryManager.AddBook(book);
        ShowMessage($"Куплено: {book.Title}");
    }
    #endregion

    #region Balance & Messages
    public void AddToBalance(float amount, string message = null)
    {
        playerBalance += amount;
        UpdateBalanceText();

        if (!string.IsNullOrEmpty(message))
            ShowMessage(message);
    }

    private void ShowMessage(string text)
    {
        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);

        messageCoroutine = StartCoroutine(MessageRoutine(text));
    }

    private IEnumerator MessageRoutine(string text)
    {
        messageText.text = text;
        messageText.gameObject.SetActive(true);

        yield return new WaitForSeconds(MessageDuration);

        messageText.gameObject.SetActive(false);
    }

    private void UpdateBalanceText()
    {
        balanceText.text = $"Баланс: {playerBalance:0.00}";
    }
    #endregion

    #region Save / Load
    public void SaveProfile()
    {
        try
        {
            var data = new SaveData
            {
                PlayerBalance = playerBalance,
                InventoryIds = inventoryManager.Inventory
                    .Where(b => b != null)
                    .Select(b => b.Title)
                    .ToList()
            };

            var formatter = new BinaryFormatter();
            using var file = File.Open(SaveFilePath, FileMode.Create);
            formatter.Serialize(file, data);

            ShowMessage("Профиль сохранён");
        }
        catch
        {
            ShowMessage("Ошибка сохранения");
        }
    }

    public void LoadProfile()
    {
        if (!File.Exists(SaveFilePath))
        {
            CreateNewProfile("Нет сохранённого профиля. Создан новый.");
            return;
        }

        try
        {
            var formatter = new BinaryFormatter();
            using var file = File.Open(SaveFilePath, FileMode.Open);
            var data = (SaveData)formatter.Deserialize(file);

            playerBalance = data.PlayerBalance;
            var loadedBooks = RebuildInventory(data.InventoryIds);
            inventoryManager.SetInventory(loadedBooks);

            UpdateBalanceText();
            ShowMessage("Профиль загружен");
        }
        catch
        {
            CreateNewProfile("Ошибка загрузки. Создан новый профиль.");
        }
    }

    private List<Book> RebuildInventory(List<string> inventoryIds)
    {
        if (inventoryIds == null || inventoryIds.Count == 0)
            return new List<Book>();

        var result = new List<Book>();
        foreach (var title in inventoryIds)
        {
            var match = books.FirstOrDefault(b => b.Title == title);
            if (match != null)
                result.Add(match);
        }

        return result;
    }

    private void CreateNewProfile(string messageOverride = "Создан новый профиль")
    {
        playerBalance = DefaultBalance;
        inventoryManager.Clear();

        UpdateBalanceText();
        ShowMessage(messageOverride);
    }
    #endregion
}