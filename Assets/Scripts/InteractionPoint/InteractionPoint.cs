using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionPoint : MonoBehaviour
{
    public string colorName;
    public Animator animator;
    public TextMeshProUGUI hintText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator=GetComponent<Animator>();
        hintText.gameObject.SetActive(false);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerControllerTest playerControllerTest=collision.GetComponent<PlayerControllerTest>();
            if (playerControllerTest != null)
            {
                playerControllerTest.interactionPoint = this;
                animator.SetBool("bHasEntered", true);
                hintText.gameObject.SetActive(true);

            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerControllerTest playerControllerTest = collision.GetComponent<PlayerControllerTest>();
            if (playerControllerTest != null)
            {
                playerControllerTest.interactionPoint = null;
                animator.SetBool("bHasEntered", false);
                hintText.gameObject.SetActive(false);
            }
        }
    }
}
