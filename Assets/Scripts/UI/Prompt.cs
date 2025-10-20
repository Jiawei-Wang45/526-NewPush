using UnityEngine;

public class Prompt : MonoBehaviour
{
    public PromptArrow ImageComponent;
    private void OnEnable()
    {
        ImageComponent.OnAnimationEnd += DestroyItself;
    }
    private void DestroyItself()
    {
        Destroy(gameObject);
    }
}
