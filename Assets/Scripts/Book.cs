using UnityEngine;

[CreateAssetMenu(
    fileName = "Book",
    menuName = "Books/Book"
)]
public class Book : ScriptableObject
{
    [SerializeField] private string title;
    [SerializeField] private float price;

    public string Title => title;
    public float Price => price;
}