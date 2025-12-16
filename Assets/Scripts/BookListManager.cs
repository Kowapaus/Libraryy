using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private RectTransform content;
    [SerializeField] private BookItemUI bookItemPrefab;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text balanceText;

    [Header("References")]
    [SerializeField] private InventoryManager inventoryManager;

    [Header("Data")]
    [SerializeField] private List<Book> books = new();
    [SerializeField] private float playerBalance = 50f;

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
                Destroy(item.gameObject);
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

        yield return new WaitForSeconds(2f);

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
                Inventory = new List<Book>(inventoryManager.Inventory)
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
            CreateNewProfile();
            return;
        }

        try
        {
            var formatter = new BinaryFormatter();
            using var file = File.Open(SaveFilePath, FileMode.Open);
            var data = (SaveData)formatter.Deserialize(file);

            playerBalance = data.PlayerBalance;
            inventoryManager.SetInventory(data.Inventory);

            UpdateBalanceText();
            ShowMessage("Профиль загружен");
        }
        catch
        {
            CreateNewProfile();
            ShowMessage("Ошибка загрузки");
        }
    }

    private void CreateNewProfile()
    {
        playerBalance = 50f;
        inventoryManager.Clear();

        UpdateBalanceText();
        ShowMessage("Создан новый профиль");
    }

    #endregion
}
