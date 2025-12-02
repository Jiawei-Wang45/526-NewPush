using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeInAndOut : MonoBehaviour
{
    public static FadeInAndOut instance;
    [NonSerialized] private Image image;
    [SerializeField] private float fadeTime = 1.0f;
    private IEnumerator fadeNumerator = null;
    //private bool shouldBroadcast = false;
    public delegate void FadeFinished();
    public event FadeFinished OnFadeFinished;
    private void Awake()
    {
        image = GetComponent<Image>();
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }
    public void FadeToBlack()
    {
        if (fadeNumerator!=null)
        {
            StopCoroutine(fadeNumerator);
        }
        fadeNumerator = FadeCoroutine(1);
        StartCoroutine(fadeNumerator);
    }
    public void FadeToClear()
    {
        if (fadeNumerator != null)
        {
            StopCoroutine(fadeNumerator);
        }
        fadeNumerator= FadeCoroutine(0);
        StartCoroutine(fadeNumerator);
    }
    private IEnumerator FadeCoroutine(float targetAlpha)
    {
        float elapsedTime = 0;
        Color previousColor = image.color;
        while (elapsedTime < fadeTime)
        {
            float aValue = Mathf.Lerp(previousColor.a, targetAlpha, Mathf.Clamp01(elapsedTime / fadeTime));
            image.color = new Color(previousColor.r, previousColor.g, previousColor.b, aValue);
            elapsedTime+= Time.deltaTime;
            yield return null;
        }
        OnFadeFinished?.Invoke();
    }
}
