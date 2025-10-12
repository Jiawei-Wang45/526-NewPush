using UnityEngine;
using UnityEngine.UI;

// Attach this to a UI GameObject. Assign the gradientBar (RawImage),
// baseSwatch/currentSwatch (Image) and optional markers (RectTransform) in the Inspector.
public class HueDisplay : MonoBehaviour
{
    [Tooltip("RawImage that will show the generated hue gradient")]
    public RawImage gradientBar;

    [Tooltip("Small Image that shows player's base hue")]
    public Image baseSwatch;

    [Tooltip("Small Image that shows player's current hue")]
    public Image currentSwatch;

    [Tooltip("Optional marker to indicate base hue position on the gradient bar")]
    public RectTransform baseMarker;

    [Tooltip("Optional marker to indicate current hue position on the gradient bar")]
    public RectTransform currentMarker;

    [Tooltip("PlayerHSLSystem source (if not set will try to find in scene)")]
    public PlayerHSLSystem playerHSL;

    [Header("Gradient settings")]
    public int textureWidth = 360;
    public int textureHeight = 16;
    public float markerYOffset = 0f;

    private Texture2D gradientTexture;

    void Start()
    {
        if (playerHSL == null)
        {
            playerHSL = FindFirstObjectByType<PlayerHSLSystem>();
        }

        if (gradientBar != null)
        {
            CreateGradientTexture();
        }

        // markers should be assigned in the Inspector (optional)
    }

    void Update()
    {
        if (playerHSL == null) return;

        float baseHueDeg = playerHSL.originalHue; // 0-360
        float currentHueDeg = playerHSL.hue; // already original + shift in script

        // update swatches
        if (baseSwatch != null)
        {
            baseSwatch.color = HSLToRGB(baseHueDeg / 360f, playerHSL.saturation / 100f, playerHSL.lightness / 100f);
        }
        if (currentSwatch != null)
        {
            currentSwatch.color = HSLToRGB(currentHueDeg / 360f, playerHSL.saturation / 100f, playerHSL.lightness / 100f);
        }

        // update markers along gradient bar
        if (gradientBar != null && (baseMarker != null || currentMarker != null))
        {
            RectTransform barRect = gradientBar.rectTransform;
            float barWidth = barRect.rect.width;

            // Robust marker placement: map a point along the bar (world space) to marker parent's local coords
            Canvas canvas = gradientBar.canvas;
            Vector3[] corners = new Vector3[4];
            barRect.GetWorldCorners(corners); // 0=bottom-left, 3=top-right

            if (baseMarker != null)
            {
                float t = Mathf.Repeat(baseHueDeg / 360f, 1f);
                Vector3 worldPos = Vector3.Lerp(corners[0], corners[3], t);
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, worldPos);

                RectTransform markerParent = baseMarker.parent as RectTransform;
                if (markerParent == null) markerParent = barRect.parent as RectTransform;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(markerParent, screenPoint, canvas.worldCamera, out Vector2 localPoint))
                {
                    baseMarker.anchoredPosition = localPoint + new Vector2(0f, markerYOffset);
                }
            }

            if (currentMarker != null)
            {
                float t = Mathf.Repeat(currentHueDeg / 360f, 1f);
                Vector3 worldPos = Vector3.Lerp(corners[0], corners[3], t);
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, worldPos);

                RectTransform markerParent = currentMarker.parent as RectTransform;
                if (markerParent == null) markerParent = barRect.parent as RectTransform;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(markerParent, screenPoint, canvas.worldCamera, out Vector2 localPoint))
                {
                    currentMarker.anchoredPosition = localPoint + new Vector2(0f, markerYOffset);
                }
            }
        }
    }

    private void CreateGradientTexture()
    {
        gradientTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        gradientTexture.wrapMode = TextureWrapMode.Clamp;
        for (int x = 0; x < textureWidth; x++)
        {
            float h = (float)x / (textureWidth - 1);
            Color c = HSLToRGB(h, 1f, 0.5f);
            for (int y = 0; y < textureHeight; y++)
            {
                gradientTexture.SetPixel(x, y, c);
            }
        }
        gradientTexture.Apply();

        gradientBar.texture = gradientTexture;
        gradientBar.uvRect = new Rect(0, 0, 1, 1);
    }

    private Color HSLToRGB(float h, float s, float l)
    {
        float r, g, b;
        if (s == 0f)
        {
            r = g = b = l;
        }
        else
        {
            float q = l < 0.5f ? l * (1f + s) : l + s - l * s;
            float p = 2f * l - q;
            r = HueToRGB(p, q, h + 1f / 3f);
            g = HueToRGB(p, q, h);
            b = HueToRGB(p, q, h - 1f / 3f);
        }
        return new Color(r, g, b, 1f);
    }

    private float HueToRGB(float p, float q, float t)
    {
        if (t < 0f) t += 1f;
        if (t > 1f) t -= 1f;
        if (t < 1f / 6f) return p + (q - p) * 6f * t;
        if (t < 1f / 2f) return q;
        if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
        return p;
    }
}
