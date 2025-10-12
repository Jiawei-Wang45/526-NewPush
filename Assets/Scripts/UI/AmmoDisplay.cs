using UnityEngine;
using UnityEngine.UI;

// Simple ammo UI: shows numeric text and a fill image representing ammo percentage.
public class AmmoDisplay : MonoBehaviour
{
    public Text ammoText; // optional
    public Image ammoFillImage; // optional, should be Image.Type.Filled with Fill Method = Horizontal
    public PlayerHSLSystem playerHSL;

    void Start()
    {
        if (playerHSL == null) playerHSL = FindFirstObjectByType<PlayerHSLSystem>();
    }

    void Update()
    {
        if (playerHSL == null) return;
        int current = playerHSL.currentAmmo;
        int max = playerHSL.maxAmmo;

        if (ammoText != null)
        {
            ammoText.text = current + " / " + max;
        }

        if (ammoFillImage != null)
        {
            float v = max > 0 ? (float)current / max : 0f;
            ammoFillImage.fillAmount = Mathf.Clamp01(v);
        }
    }
}
