using UnityEngine;

public class MovableCube : MonoBehaviour
{
    public void Move(Vector3 movement)
    {
        transform.position += movement;
    }

    public void SetColor(Color color)
    {
        Material newMaterial = new Material(gameObject.GetComponent<MeshRenderer>().material);
        newMaterial.color = color;
        gameObject.GetComponent<MeshRenderer>().material = newMaterial;
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }
}
