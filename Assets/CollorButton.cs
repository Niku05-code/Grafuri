using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollorButton : MonoBehaviour
{
    public Button button;
    public float fadeDuration;
    private Image buttonImage;

    void Start()
    {
        buttonImage = button.GetComponent<Image>();
        StartCoroutine(FadeRainbowColors());
    }

    IEnumerator FadeRainbowColors()
    {
        while (true)
        {
            for (float t = 0; t < 1; t += Time.deltaTime / fadeDuration)
            {
                Color rainbowColor = Color.HSVToRGB(t, 1f, 1f);
                buttonImage.color = rainbowColor;
                yield return null;
            }
        }
    }
}
