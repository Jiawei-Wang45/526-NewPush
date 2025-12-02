using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class AreaExit : MonoBehaviour
{
    [SerializeField] private Transform TargetTransitionPos;
    [SerializeField] private BaseRoom RoomToGo;
    [SerializeField] private BaseRoom PreviousRoom;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject==PlayerController.instance.gameObject)
        {
            PlayerController.instance.playerInput.Default.Disable();
            FadeInAndOut.instance.OnFadeFinished += OnFadeOutFinished;
            FadeInAndOut.instance.FadeToBlack();
        }
    }
    public void OnFadeOutFinished()
    {
        FadeInAndOut.instance.OnFadeFinished -= OnFadeOutFinished;
        FadeInAndOut.instance.OnFadeFinished += OnFadeInFinished;
        PreviousRoom.PlayerExited();
        PlayerController.instance.transform.position = TargetTransitionPos.position;
        StartCoroutine(WaitCoroutine());
    }
    public void OnFadeInFinished()
    {
        FadeInAndOut.instance.OnFadeFinished -= OnFadeInFinished;
        PlayerController.instance.playerInput.Default.Enable();
        RoomToGo.PlayerEntered();
        
    }
    private IEnumerator WaitCoroutine()
    {
        yield return new WaitForSeconds(1);
        FadeInAndOut.instance.FadeToClear();
    }
}
