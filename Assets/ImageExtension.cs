using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class ImageExtension
{
    [MenuItem("CONTEXT/Image/Adjust Size")]
    public static void AdjustSize(MenuCommand cmd)
    {
        var image = cmd.context as Image;
        var rectTransform = image.GetComponent<RectTransform>();
        var newHeight = rectTransform.rect.width / (image.sprite.rect.width / image.sprite.rect.height);
        rectTransform.sizeDelta = new Vector2(rectTransform.rect.width, newHeight);
    }
}
