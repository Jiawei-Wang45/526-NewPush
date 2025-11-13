using UnityEngine;

public class TutorialArea : MonoBehaviour
{
    public enum Area
    {
        frontPart,
        rearPart,
        None
    }
    
    public Area areaType;
    public BaseRoom.TutorialRoom tutorialRoom;
    //private DefenseTutorialRoom defenseTutorialRoom;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("player"))
        {
            switch(tutorialRoom)
            {
                case BaseRoom.TutorialRoom.DefenseTutorialRoom:
                    FindFirstObjectByType<DefenseTutorialRoom>().OnAreaEntered(areaType);
                    break;
                case BaseRoom.TutorialRoom.AbilityTutorialRoom:
                    FindFirstObjectByType<AbilityTutorialRoom>().OnAreaEntered(areaType);
                    break;
                case BaseRoom.TutorialRoom.TestTutorialRoom:
                    FindFirstObjectByType<TestTutorialRoom>().OnAreaEntered(areaType);
                    break;
            }
            
            GetComponent<Collider2D>().enabled = false;
        }
    }
}
