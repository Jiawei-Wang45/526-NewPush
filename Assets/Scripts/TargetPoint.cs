using UnityEngine;

public class TargetPoint : MonoBehaviour
{
    public Color normalColor;
    public Color coveredColor;
    
    private bool isCovered = false;
    private SpriteRenderer spriteRenderer;
    private PuzzleManager puzzleManager;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        puzzleManager = FindObjectOfType<PuzzleManager>();
        
        spriteRenderer.color = normalColor;
        
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Box"))
        {
            SetCovered(true);
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Box"))
        {
            SetCovered(false);
        }
    }
    
    void SetCovered(bool covered)
    {
        isCovered = covered;
        
        spriteRenderer.color = covered ? coveredColor : normalColor;
        if (isCovered)
        {
            puzzleManager.CheckPuzzleComplete();
        }

    }
    
    public bool IsCovered()
    {
        return isCovered;
    }
}