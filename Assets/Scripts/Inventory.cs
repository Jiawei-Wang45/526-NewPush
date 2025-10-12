using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Inventory: MonoBehaviour
{
    public List<PlayerWeapon> weapons;
    private PlayerControllerTest pcTest;
    public bool hasKey = false;

    private void Start()
    {
        pcTest = GetComponent<PlayerControllerTest>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // treasure_2 (key) behavior: fallback to name match for existing setup
        if (other.gameObject.name == "treasure_2" || other.gameObject.name.StartsWith("treasure_2"))
        {
            hasKey = true;
            Destroy(other.gameObject);
            return;
        }

        // Prefer component-based detection: if the collided object has a PropsList, treat it as a treasure that grants weapons
        if (other.gameObject.TryGetComponent<PropsList>(out var props))
        {
            if (props.playerWeapon != null)
            {
                weapons.Add(props.playerWeapon);
                pcTest.currentWeapon = weapons.Last();
            }

            // also apply hue from TreasureProps if present (spawned treasure will have this)
            if (other.gameObject.TryGetComponent<TreasureProps>(out var treasureProps))
            {
                var hsl = GetComponent<PlayerHSLSystem>();
                if (hsl != null)
                {
                    hsl.originalHue = treasureProps.hue;
                    hsl.currentHueShift = 0f;
                    // refill ammo to full on pickup
                    hsl.currentAmmo = hsl.maxAmmo;
                }
            }

            Destroy(other.gameObject);
            return;
        }

        // Backward compatibility: some placed treasures may be named treasure_1 without a PropsList component
        if (other.gameObject.name.StartsWith("treasure_1"))
        {
            var propsFallback = other.gameObject.GetComponent<PropsList>();
            if (propsFallback != null && propsFallback.playerWeapon != null)
            {
                weapons.Add(propsFallback.playerWeapon);
                pcTest.currentWeapon = weapons.Last();
            }

            var treasureProps2 = other.gameObject.GetComponent<TreasureProps>();
            if (treasureProps2 != null)
            {
                var hsl2 = GetComponent<PlayerHSLSystem>();
                if (hsl2 != null)
                {
                    hsl2.originalHue = treasureProps2.hue;
                    hsl2.currentHueShift = 0f;
                    // refill ammo to full on pickup
                    hsl2.currentAmmo = hsl2.maxAmmo;
                }
            }

            Destroy(other.gameObject);
            return;
        }
    }

    public bool HasKey()
    {
        return hasKey;
    }

}