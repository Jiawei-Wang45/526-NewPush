using UnityEngine;

[System.Serializable]
public class HSLColor
{
    public float H = 0f; // 色相 (0-360)
    public float S = 100f; // 饱和度 (0-100)
    public float L = 50f; // 亮度 (0-100)

    // 默认构造函数
    public HSLColor()
    {
        H = 0f;
        S = 100f;
        L = 50f;
    }

    // 带参数的构造函数
    public HSLColor(float h, float s, float l)
    {
        H = h;
        S = s;
        L = l;
    }

    // 将HSL颜色转换为RGB颜色
    public Color ToRGB()
    {
        // 将HSL值归一化
        float h = Mathf.Clamp(H, 0f, 360f) / 360f;
        float s = Mathf.Clamp(S, 0f, 100f) / 100f;
        float l = Mathf.Clamp(L, 0f, 100f) / 100f;

        // HSL转RGB算法
        float r, g, b;

        if (s == 0f)
        {
            r = g = b = l; // 灰色
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

    // 辅助函数：将色相转换为RGB分量
    private float HueToRGB(float p, float q, float t)
    {
        if (t < 0f) t += 1f;
        if (t > 1f) t -= 1f;
        if (t < 1f / 6f) return p + (q - p) * 6f * t;
        if (t < 1f / 2f) return q;
        if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
        return p;
    }

    // 重写ToString方法便于调试
    public override string ToString()
    {
        return $"HSL({H:F1}, {S:F1}, {L:F1})";
    }
}
