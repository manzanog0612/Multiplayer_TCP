using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MovableSquare : MonoBehaviour
{
    public RectTransform rectTransform;
    public TextMeshProUGUI text = null;

    public void Move(Vector2 movement)
    {
        rectTransform.anchoredPosition += movement;
    }

    public void SetColor(Color color)
    {
        rectTransform.GetComponent<Image>().color = color;
    }

    public void SetPosition(Vector2 position)
    {
        rectTransform.anchoredPosition = position;
    }
}
