using UnityEngine;

public class PromptArrow : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public delegate void PromptArrowDelegate();
    public event PromptArrowDelegate OnAnimationEnd;
   public void DestroyItself()
    {
        OnAnimationEnd?.Invoke();
    }
}
