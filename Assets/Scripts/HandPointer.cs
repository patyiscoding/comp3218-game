using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public Texture2D handCursor;

    public void HandCursor()
    {
        Cursor.SetCursor(handCursor, Vector2.zero, CursorMode.Auto);
    }

    public void DefaultCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}