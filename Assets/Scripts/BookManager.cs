using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.IO;
using System;
using System.Runtime.Serialization.Formatters.Binary;

public class BookListManager : MonoBehaviour
{
    [Header("UI")]
    public RectTransform content;
    public GameObject bookItemPrefab;
    public TMP_Text messageText;
    public TMP_Text balanceText;

    [Header("Refs")]
    public InventoryManager inventoryManager;

    [Header("Data")]
    public List<Book> books;
    public float playerBalance = 50f;

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, "player_profile.dat");

    void Start()
    {
        UpdateBalanceText();

        if (messageText != null)
            messageText.gameObject.SetActive(false);

        Populate();
    }

    #region Shop UI
    public void Populate()
    {
        if (content == null || bookItemPrefab == null)
        {
            Debug.LogError("BookListManager: Content или Prefab не назначены!");
            return;
        }

        foreach (Transform t in content)
            Destroy(t.gameObject);

        foreach (var b in books)
        {
            var go = Instantiate(bookItemPrefab, content);
            var ui = go.GetComponent<BookItemUI>();

            if (ui != null)
                ui.Setup(b, OnBuy);
            else
                Debug.LogError("BookItemPrefab не содержит BookItemUI!");
        }

        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    void OnBuy(Book book, GameObject itemGO)
    {
        if (playerBalance >= book.price)
        {
            playerBalance -= book.price;
            UpdateBalanceText();

            if (inventoryManager == null)
                inventoryManager = FindFirstObjectByType<InventoryManager>();

            if (inventoryManager != null)
                inventoryManager.AddBook(book);

            ShowMessage($"Куплено: {book.title}");
        }
        else
        {
            ShowMessage("Недостаточно средств!");
        }
    }
    #endregion

    #region Messages / Balance
    public void AddToBalance(float amount, string message = null)
    {
        playerBalance += amount;
        UpdateBalanceText();

        if (!string.IsNullOrEmpty(message))
            ShowMessage(message);
    }

    void ShowMessage(string msg)
    {
        if (messageText == null) return;

        messageText.text = msg;
        messageText.gameObject.SetActive(true);

        CancelInvoke(nameof(HideMessage));
        Invoke(nameof(HideMessage), 2f);
    }

    void HideMessage()
    {
        if (messageText != null)
            messageText.gameObject.SetActive(false);
    }

    void UpdateBalanceText()
    {
        if (balanceText != null)
            balanceText.text = $"Баланс: {playerBalance:0.00}";
    }
    #endregion

    #region Save / Load Profile
    [Serializable]
    private class SaveData
    {
        public float playerBalance;
        public List<Book> inventory;
    }

    // Сохраняем текущий профиль
    public void SaveProfile()
    {
        try
        {
            if (inventoryManager == null)
                inventoryManager = FindFirstObjectByType<InventoryManager>();

            SaveData data = new SaveData
            {
                playerBalance = this.playerBalance,
                inventory = new List<Book>(inventoryManager.inventory)
            };

            BinaryFormatter bf = new BinaryFormatter();
            using (FileStream file = File.Open(SaveFilePath, FileMode.Create))
            {
                bf.Serialize(file, data);
            }

            ShowMessage("Профиль сохранён!");
        }
        catch (Exception ex)
        {
            Debug.LogError("Ошибка сохранения профиля: " + ex.Message);
            ShowMessage("Ошибка при сохранении профиля");
        }
    }

    // Загружаем профиль
    public void LoadProfile()
    {
        try
        {
            if (!File.Exists(SaveFilePath))
            {
                CreateNewProfile();
                return;
            }

            BinaryFormatter bf = new BinaryFormatter();
            using (FileStream file = File.Open(SaveFilePath, FileMode.Open))
            {
                SaveData data = bf.Deserialize(file) as SaveData;

                if (data == null)
                    throw new Exception("Файл повреждён");

                playerBalance = data.playerBalance;
                UpdateBalanceText();

                if (inventoryManager == null)
                    inventoryManager = FindFirstObjectByType<InventoryManager>();

                if (inventoryManager != null)
                {
                    inventoryManager.inventory = new List<Book>(data.inventory);
                    inventoryManager.Populate();
                }

                ShowMessage("Профиль загружен!");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Ошибка загрузки профиля: " + ex.Message);
            CreateNewProfile();
            ShowMessage("Ошибка загрузки — создан новый профиль");
        }
    }

   
    public void CreateNewProfile(float baseBalance = 50f)
    {
        playerBalance = baseBalance;
        UpdateBalanceText();

        if (inventoryManager == null)
            inventoryManager = FindFirstObjectByType<InventoryManager>();

        if (inventoryManager != null)
        {
            inventoryManager.inventory = new List<Book>();
            inventoryManager.Populate();
        }

        ShowMessage("Новый профиль создан!");
    }
    #endregion
}
