using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public float maxHealth;
    public float enemyMovementSpeed;
    public float enemyDamage;
    public float health;
    public string color;
    [Header("Treasure drop")]
    public GameObject treasurePrefab; // assign the treasure_1 prefab in inspector
    
    private EnemyHSLSystem hslSystem;

    private void Start()
    {
        hslSystem = GetComponent<EnemyHSLSystem>();
        
        // 如果HSL系统存在，让HSL系统管理血量
        if (hslSystem != null)
        {
            hslSystem.currentHealth = maxHealth;
            hslSystem.maxHealth = maxHealth;
            health = hslSystem.currentHealth; // 从HSL系统同步
        }
        else
        {
            health = maxHealth; // 如果没有HSL系统，使用默认值
        }
    }
    
    private void Update()
    {
        // 确保血量始终与HSL系统同步
        if (hslSystem != null)
        {
            health = hslSystem.currentHealth;
        }
    }
    
    public void takeDamage(float damage)
    {
        // 如果HSL系统存在，让HSL系统处理伤害
        if (hslSystem != null)
        {
            hslSystem.TakeDamage(damage);
            health = hslSystem.currentHealth; // 从HSL系统同步血量
        }
        else
        {
            health = Mathf.Clamp(health - damage, 0, maxHealth);
        }
        
        if (health <= 0)
        {
            // spawn treasure prefab if assigned
            if (treasurePrefab != null)
            {
                GameObject drop = Instantiate(treasurePrefab, transform.position, Quaternion.identity);
                // copy props (if the enemy has a PropsList or weapon data, try to copy)
                var enemyHSL = GetComponent<EnemyHSLSystem>();
                var treasureProps = drop.GetComponent<TreasureProps>();
                if (treasureProps != null && enemyHSL != null)
                {
                    // use enemy initial hue and set saturation/lightness to max for visibility
                    treasureProps.hue = enemyHSL.initialHue;

                    // compute color using enemy initial H/S/L so drop matches enemy's initial color
                    Color dropColor = HSLToRGB(enemyHSL.initialHue / 360f, enemyHSL.initialSaturation / 100f, enemyHSL.initialLightness / 100f);

                    // apply to SpriteRenderers
                    var srs = drop.GetComponentsInChildren<SpriteRenderer>(true);
                    foreach (var sr in srs) sr.color = dropColor;

                    // apply to generic Renderers (materials)
                    var rends = drop.GetComponentsInChildren<Renderer>(true);
                    foreach (var r in rends)
                    {
                        if (r is SpriteRenderer) continue;
                        if (r.sharedMaterial != null && r.sharedMaterial.HasProperty("_Color"))
                        {
                            r.material.color = dropColor;
                        }
                    }

                    // apply to UI Images
                    var imgs = drop.GetComponentsInChildren<UnityEngine.UI.Image>(true);
                    foreach (var img in imgs) img.color = dropColor;
                }

                // if enemy has a weapon to drop, and the prefab has a PropsList, copy it
                var enemyWeapon = GetComponent<EnemyController>()?.weapon;
                if (enemyWeapon != null)
                {
                    var propsListOnDrop = drop.GetComponent<PropsList>();
                    if (propsListOnDrop != null)
                    {
                        // create a PlayerWeapon instance reference by converting EnemyWeaponData if needed
                        // Here we attempt to reuse the existing weapon setup: if EnemyWeaponData can't be converted,
                        // the PropsList.playerWeapon should be assigned in the prefab or left null.
                        // (Project-specific mapping required for full conversion.)
                    }
                }

            }

            Destroy(GetComponent<EnemyController>().BoundHealthbar);
            Destroy(gameObject);
        }
    }
    
    public void Heal(float healAmount)
    {
        // 如果HSL系统存在，让HSL系统处理治疗
        if (hslSystem != null)
        {
            hslSystem.Heal(healAmount);
            health = hslSystem.currentHealth; // 从HSL系统同步血量
        }
        else
        {
            health += healAmount;
            health = Mathf.Clamp(health, 0f, maxHealth);
        }
        
    }

    // HSL -> RGB helpers
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

