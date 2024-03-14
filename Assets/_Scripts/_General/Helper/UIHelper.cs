using UnityEngine;

public static class UIHelper
{
    private static Camera _camera;
    public static Camera Camera
    {
        get
        {
            if (_camera == null)
                _camera = Camera.main;
            return _camera;
        }
    }

    public static bool IsInsideScreen(this RectTransform rect)
    {
        var globalPosition = rect.TransformPoint(rect.localPosition);
        if(globalPosition.x - (rect.sizeDelta.x * .5f) >= 0 &&
           globalPosition.y - (rect.sizeDelta.y * .5f) >= 0 &&
           globalPosition.x + (rect.sizeDelta.x * .5f) <= Screen.width &&
           globalPosition.y + (rect.sizeDelta.y * .5f) <= Screen.height)
        {
            return true;
        }
        return false;

    }

    public static void MoveIntoScreen(this RectTransform image, Vector3 referencePosition)
    {
        Vector2 center = new(Screen.width * .5f, Screen.height * .5f);
        Vector2 imagePosition = (Vector2)Camera.WorldToScreenPoint(referencePosition) - center;

        Vector2 direction = imagePosition - center;

        float maxY = (Screen.height * .5f) * Mathf.Sign(imagePosition.y);
        maxY -= (image.sizeDelta.y * .5f) * Mathf.Sign(imagePosition.y);
        float maxX = (Screen.width * .5f) * Mathf.Sign(imagePosition.x);
        maxX -= (image.sizeDelta.x * .5f) * Mathf.Sign(imagePosition.x);

        float scaler;
        if (maxX / imagePosition.x < maxY / imagePosition.y)
            scaler = maxX / imagePosition.x;
        else
            scaler = maxY / imagePosition.y;

        float y = Mathf.Lerp(0, imagePosition.y, scaler);
        float x = Mathf.Lerp(0, imagePosition.x, scaler);

        image.position = new Vector2(x, y) + center;
    }
}