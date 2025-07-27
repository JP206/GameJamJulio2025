using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField] private Texture2D customCursor;
    [SerializeField] private Vector2 hotspot = Vector2.zero;

    void Start()
    {
        Cursor.SetCursor(customCursor, hotspot, CursorMode.Auto);
    }
}
