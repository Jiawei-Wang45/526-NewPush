using UnityEngine;

public class PlayerReloadPrompt : MonoBehaviour
{
    public PlayerStats playerStats;
    public GameObject reloadPrompt; // UI element (Text/Image) that shows "Press R to Reload!!!"

    private void Start()
    {
        if (playerStats == null)
        {
            // try to find player stats from scene
            PlayerControllerTest p = FindFirstObjectByType<PlayerControllerTest>();
            if (p != null)
                playerStats = p.GetComponent<PlayerStats>();
        }

        if (reloadPrompt == null)
        {
            // assume the prompt is a child UI element
            Transform t = transform.Find("ReloadPrompt");
            if (t != null)
                reloadPrompt = t.gameObject;
        }

        if (reloadPrompt != null)
            reloadPrompt.SetActive(false);
    }

    private void Update()
    {
        if (playerStats == null || reloadPrompt == null) return;

        bool shouldShow = playerStats.currentAmmo <= 0 && !playerStats.IsReloading;
        if (reloadPrompt.activeSelf != shouldShow)
            reloadPrompt.SetActive(shouldShow);
    }
}
