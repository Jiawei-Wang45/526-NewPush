using UnityEngine;

public class AnimationEventForwarder : MonoBehaviour
{
   public void OnAnimationEnd()
    {
        GetComponentInParent<WieldEffect>().DestroyItself();
    }
}
