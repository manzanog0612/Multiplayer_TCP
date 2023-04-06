using TMPro;
using UnityEngine;

public class MovableSquare : MonoBehaviour
{
    public RectTransform rectTransform;
    public TextMeshProUGUI text = null;

    public void Move(Vector2 movement)
    {
        rectTransform.anchoredPosition += movement;
    }

    public void SetText(string txt)
    {
        text.text = txt;
    }

    public void SetPosition(Vector2 position)
    {
        rectTransform.anchoredPosition = position;
    }
}
