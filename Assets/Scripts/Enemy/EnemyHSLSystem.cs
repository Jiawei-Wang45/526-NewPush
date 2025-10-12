using UnityEngine;

public class EnemyHSLSystem : MonoBehaviour
{
    [Header("HSL Color Settings")]
    [Range(0f, 360f)]
    public float hue = 0f; // 色相，可以根据需要调整
    
    [Range(0f, 100f)]
    public float saturation = 100f; // 饱和度，固定为100（敌人不消耗弹药）
    
    [Range(0f, 100f)]
    public float lightness = 50f; // 亮度代表血量 (0-100)
    
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    
    [Header("Visual Settings")]
    public SpriteRenderer enemySpriteRenderer;
    public UnityEngine.UI.Image healthbarImage;
    // 记录敌人出生时的初始HSL（用于掉落恢复初始色）
    [HideInInspector]
    public float initialHue;
    [HideInInspector]
    public float initialSaturation;
    [HideInInspector]
    public float initialLightness;
    
    private void Start()
    {
        // 初始化HSL值
        currentHealth = maxHealth;
        lightness = 50f; // 初始亮度为50
        saturation = 100f; // 饱和度固定为100（敌人不消耗弹药）
        
        // 如果没有指定SpriteRenderer，尝试获取
        if (enemySpriteRenderer == null)
        {
            enemySpriteRenderer = GetComponent<SpriteRenderer>();
        }

        // 保存初始HSL值
        initialHue = hue;
        initialSaturation = saturation;
        initialLightness = lightness;
        
        UpdateColor();
    }
    
    private void Update()
    {
        UpdateColor();
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        
        // 计算新的亮度值 (血量越低，亮度越高)
        lightness = 50f + (1f - (currentHealth / maxHealth)) * 50f;
        
        // 确保亮度在0-100范围内
        lightness = Mathf.Clamp(lightness, 0f, 100f);
        
        // 检查是否死亡（亮度达到100）
        if (lightness >= 100f)
        {
            OnEnemyDeath();
        }
        
        UpdateColor();
    }
    
    public void Heal(float healAmount)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        
        // 重新计算亮度
        lightness = 50f + (1f - (currentHealth / maxHealth)) * 50f;
        lightness = Mathf.Clamp(lightness, 0f, 100f);
        
        UpdateColor();
    }
    
    
    private void UpdateColor()
    {
        // 将HSL转换为RGB
        Color hslColor = HSLToRGB(hue / 360f, saturation / 100f, lightness / 100f);
        
        // 应用到敌人精灵
        if (enemySpriteRenderer != null)
        {
            enemySpriteRenderer.color = hslColor;
        }
        
        // 应用到血条
        if (healthbarImage != null)
        {
            healthbarImage.color = hslColor;
        }
    }
    
    private Color HSLToRGB(float h, float s, float l)
    {
        float r, g, b;
        
        if (s == 0f)
        {
            // 无饱和度，灰色
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
    
    private void OnEnemyDeath()
    {
        // 敌人死亡时的处理
        Debug.Log("Enemy died - Lightness reached 100!");
        
        // 敌人死亡逻辑已经在EnemyStats中处理
        // 这里可以添加额外的死亡效果
    }
    
    // 获取当前HSL值的公共方法
    public Vector3 GetHSLValues()
    {
        return new Vector3(hue, saturation, lightness);
    }
    
    // 获取当前血量的公共方法
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
}
