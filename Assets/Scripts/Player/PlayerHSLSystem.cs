using UnityEngine;

public class PlayerHSLSystem : MonoBehaviour
{
    [Header("HSL Color Settings")]
    [Range(0f, 360f)]
    public float hue = 0f; // 色相，可以根据需要调整
    
    [Range(0f, 100f)]
    public float saturation = 100f; // 饱和度代表弹药量 (0-100)
    
    [Range(0f, 100f)]
    public float lightness = 50f; // 亮度代表血量 (0-100)
    
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    
    [Header("Ammo Settings")]
    public int maxAmmo = 100;
    public int currentAmmo = 100;
    
    [Header("Hue Shift Settings")]
    public float originalHue = 0f; // 原始色相
    public float currentHueShift = 0f; // 当前色相偏移
    public float maxHueShift = 180f; // 最大允许的色相偏移（180度）
    public float hueShiftDecayRate = 10f; // 色相偏移衰减率（每秒度数）
    
    [Header("Visual Settings")]
    public SpriteRenderer playerSpriteRenderer;
    public UnityEngine.UI.Image healthbarImage;
    
    private void Start()
    {
        // 初始化HSL值
        currentHealth = maxHealth;
        currentAmmo = maxAmmo;
        lightness = 50f; // 初始亮度为50
        saturation = 100f; // 初始饱和度为100（满弹药）
        hue = 0f; // 初始色相为0
        originalHue = 0f; // 记录原始色相
        currentHueShift = 0f; // 初始偏移为0
        
        // 如果没有指定SpriteRenderer，尝试获取
        if (playerSpriteRenderer == null)
        {
            playerSpriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        UpdateColor();
    }
    
    private void Update()
    {
        // 色相偏移衰减
        if (Mathf.Abs(currentHueShift) > 0f)
        {
            float decayAmount = hueShiftDecayRate * Time.deltaTime;
            if (currentHueShift > 0f)
            {
                currentHueShift = Mathf.Max(0f, currentHueShift - decayAmount);
            }
            else
            {
                currentHueShift = Mathf.Min(0f, currentHueShift + decayAmount);
            }
        }
        
        // 更新实际色相（原始色相 + 偏移）
        hue = originalHue + currentHueShift;
        hue = Mathf.Repeat(hue, 360f); // 确保色相在0-360范围内
        
        // 更新饱和度（基于弹药量）
        saturation = (float)currentAmmo / maxAmmo * 100f;
        
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
            OnPlayerDeath();
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
    
    public void ConsumeAmmo(int amount)
    {
        currentAmmo -= amount;
        currentAmmo = Mathf.Clamp(currentAmmo, 0, maxAmmo);
        
        // 饱和度会在Update中自动更新
        UpdateColor();
    }
    
    public void ReloadAmmo(int amount)
    {
        currentAmmo += amount;
        currentAmmo = Mathf.Clamp(currentAmmo, 0, maxAmmo);
        
        // 饱和度会在Update中自动更新
        UpdateColor();
    }
    
    public void SetMaxAmmo(int newMaxAmmo)
    {
        maxAmmo = newMaxAmmo;
        currentAmmo = Mathf.Clamp(currentAmmo, 0, maxAmmo);
        
        // 饱和度会在Update中自动更新
        UpdateColor();
    }
    
    public void ApplyHueShift(float enemyHue, float shiftAmount)
    {
        // 计算色相偏移（向敌人色相偏移）
        float targetHue = enemyHue;
        float shift = (targetHue - originalHue) * shiftAmount;
        
        // 处理色相环绕（0-360度）
        if (Mathf.Abs(shift) > 180f)
        {
            shift = shift > 0 ? shift - 360f : shift + 360f;
        }
        
        // 应用偏移
        currentHueShift += shift;
        currentHueShift = Mathf.Clamp(currentHueShift, -maxHueShift, maxHueShift);
    }
    
    public bool CanFire()
    {
        // 如果色相偏移过大，不能开火
        return Mathf.Abs(currentHueShift) < maxHueShift * 0.8f;
    }
    
    public float GetHueShiftPercentage()
    {
        return Mathf.Abs(currentHueShift) / maxHueShift;
    }
    
    private void UpdateColor()
    {
        // 将HSL转换为RGB
        Color hslColor = HSLToRGB(hue / 360f, saturation / 100f, lightness / 100f);
        
        // 应用到玩家精灵
        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.color = hslColor;
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
    
    private void OnPlayerDeath()
    {
        // 玩家死亡时的处理
        Debug.Log("Player died - Lightness reached 100!");
        
        // 可以在这里添加死亡逻辑，比如触发GameManager的PlayerDead方法
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.PlayerDead();
        }
    }
    
    // 获取当前HSL值的公共方法
    public Vector3 GetHSLValues()
    {
        return new Vector3(hue, saturation, lightness);
    }
    
    // 获取当前血量和弹药量的公共方法
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
    
    public float GetAmmoPercentage()
    {
        return (float)currentAmmo / maxAmmo;
    }
}
