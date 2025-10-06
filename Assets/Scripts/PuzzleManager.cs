using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public GameObject boxDoor;
 
    private int totalPoints;
    private int coveredPoints;
    private bool puzzleCompleted = false;
    
    private TargetPoint[] allTargetPoints;
    
    void Start()
    {
        allTargetPoints = FindObjectsOfType<TargetPoint>();
        totalPoints = allTargetPoints.Length;
    }
    
    public void CheckPuzzleComplete()
    {
        if (puzzleCompleted) return;
        
        coveredPoints = 0;
        
        foreach (TargetPoint point in allTargetPoints)
        {
            if (point.IsCovered())
            {
                coveredPoints++;
            }
        }
                
        if (coveredPoints >= totalPoints && totalPoints > 0)
        {
            CompletePuzzle();
        }
    }
    
    void CompletePuzzle()
    {
        if (puzzleCompleted) return;
        
        puzzleCompleted = true;
        
        Destroy(boxDoor);
    }
    
}