using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerSelect : MonoBehaviour
{
    [SerializeField] Slider[] playerBodySliders;
    [SerializeField] Slider[] playerStripeSliders;
    [SerializeField] GameObject playerPreview;
    [SerializeField] EventSystem eventSystem;

    void Start()
    {
        playerBodySliders[0].value = GameSettings.playerBodyColor.r;
        playerBodySliders[1].value = GameSettings.playerBodyColor.g;
        playerBodySliders[2].value = GameSettings.playerBodyColor.b;
        playerPreview.GetComponent<MeshRenderer>().materials[0].SetColor("_MainColor", GameSettings.playerBodyColor);
        playerStripeSliders[0].value = GameSettings.playerStripeColor.r;
        playerStripeSliders[1].value = GameSettings.playerStripeColor.g;
        playerStripeSliders[2].value = GameSettings.playerStripeColor.b;
        playerPreview.GetComponent<MeshRenderer>().materials[1].SetColor("_MainColor", GameSettings.playerStripeColor);
    }

    public void Next()
    {
        eventSystem.gameObject.SetActive(false);
        StartCoroutine(SceneLoader.instance.Load("GameMode"));
    }

    public void Back()
    {
        eventSystem.gameObject.SetActive(false);
        StartCoroutine(SceneLoader.instance.Load("Menu"));
    }

    public void ChangeBodyColor()
    {

        float r = playerBodySliders[0].value;
        float g = playerBodySliders[1].value;
        float b = playerBodySliders[2].value;

        GameSettings.playerBodyColor = new Color(r, g, b);
        playerPreview.GetComponent<MeshRenderer>().materials[0].SetColor("_MainColor", new Color(r, g, b));
    }

    public void ChangeStripeColor()
    {

        float r = playerStripeSliders[0].value;
        float g = playerStripeSliders[1].value;
        float b = playerStripeSliders[2].value;

        GameSettings.playerStripeColor = new Color(r, g, b);
        playerPreview.GetComponent<MeshRenderer>().materials[1].SetColor("_MainColor", new Color(r, g, b));
    }

}
