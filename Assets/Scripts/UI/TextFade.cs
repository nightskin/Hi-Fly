using UnityEngine;
using UnityEngine.UI;

public class TextFade : MonoBehaviour
{
    Text text;

    [SerializeField] Color[] fadeColors;
    [SerializeField] float fadeSpeed = 10;

    int index = 0;

    void Start()
    {
        text = GetComponent<Text>();
    }


    void Update()
    {

        if (fadeColors.Length == 0) return;
        else if(fadeColors.Length == 1)
        {
            text.color = Color.Lerp(text.color, fadeColors[0], fadeSpeed * Time.deltaTime);
        }
        else if(fadeColors.Length == 2)
        {
            if(text)
            {
                text.color = Color.LerpUnclamped(text.color, fadeColors[index], fadeSpeed * Time.deltaTime);
                if (text.color == fadeColors[index])
                {
                    if (index == 0) index = 1;
                    else if (index == 1) index = 0;
                }
            }
        }
        else
        {
            if(text)
            {
                text.color = Color.Lerp(text.color, fadeColors[index], fadeSpeed * Time.deltaTime);

                if(text.color == fadeColors[index])
                {
                    if(index == fadeColors.Length - 1)
                    {
                        index = 0;
                    }
                    else
                    {
                        index++;
                    }
                }

            }
        }
    }
}
