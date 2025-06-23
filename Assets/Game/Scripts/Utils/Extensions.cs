using UnityEngine;
using System.Collections;

public static class Extensions
{
public static Bounds OrthographicBounds(this Camera camera)
    {
        float screenAspect = (float)Screen.width / (float)Screen.height;
        float cameraHeight = camera.orthographicSize * 2;
        Bounds bounds = new Bounds(
            camera.transform.position,
            new Vector3(cameraHeight * screenAspect, cameraHeight, 0));
        return bounds;
    }

    public static float Height(this SpriteRenderer s)
    {
        return s.sprite.bounds.size.y * s.transform.localScale.y;
    }

    public static float Width(this SpriteRenderer s)
    {
        return s.sprite.bounds.size.x * s.transform.localScale.x;
    }

}
