using UnityEngine;

public class MovableSquare : MonoBehaviour
{
    public RectTransform rectTransform;

    public void Move(Vector2 movement)
    {
        rectTransform.anchoredPosition += movement;
    }

    public void SetPosition(Vector2 position)
    {
        rectTransform.anchoredPosition = position;
    }
}
