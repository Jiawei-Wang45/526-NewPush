using UnityEngine;

public class WinTriggering : MonoBehaviour
{
    public GameManager gameManager;
    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            gameManager.ShowWinMenu();
            Destroy(gameObject);
        }

    }
}
