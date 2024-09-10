using UnityEngine;
using UnityEngine.UI;

public class ImageFade : MonoBehaviour
{
    Image image;

    [SerializeField] Color[] fadeColors;
    [SerializeField] float fadeSpeed = 10;

    int index = 0;
    void Start()
    {
        image = GetComponent<Image>();
    }

    
    void Update()
    {
        if (fadeColors.Length == 0) return;
        if(fadeColors.Length == 1)
        {
            image.color = Color.Lerp(image.color, fadeColors[0], fadeSpeed * Time.deltaTime);
        }
        else if (fadeColors.Length == 2)
        {
            if (image)
            {
                image.color = Color.Lerp(image.color, fadeColors[index], fadeSpeed * Time.deltaTime);
                if (image.color == fadeColors[index])
                {
                    if (index == 0) index = 1;
                    else if (index == 1) index = 0;
                }
            }
        }
        else
        {
            if (image)
            {
                image.color = Color.Lerp(image.color, fadeColors[index], fadeSpeed * Time.deltaTime);

                if (image.color == fadeColors[index])
                {
                    if (index == fadeColors.Length - 1)
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
