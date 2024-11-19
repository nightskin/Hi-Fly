using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public static Color playerBodyColor = Color.red;
    public static Color playerStripeColor = new Color(1, 1, 0);
    public static float aimSense = 1000;

    [SerializeField] Slider[] playerBodySliders;
    [SerializeField] Slider[] playerStripeSliders;
    [SerializeField] Slider aimSlider;
    [SerializeField] Slider musicVolume;
    [SerializeField] Slider sfxVolume;

    [SerializeField] GameObject playerPreview;
    [SerializeField] Galaxy galaxyPreview;

    [SerializeField] EventSystem eventSystem;

    static SceneNodeManager sceneNodeManager;
    
    
    void Start()
    {
        sceneNodeManager = GetComponent<SceneNodeManager>();
    }

    public void ChangeAimSense()
    {
        aimSense = aimSlider.value;
    }

    public void ExitSettings()
    {
        StartCoroutine(SceneLoader.instance.LoadLevel("Menu"));
    }

    public void BackToGeneralSettings()
    {
        sceneNodeManager.SetActiveSceneNode("SettingsMenu");
        GameObject obj = sceneNodeManager.GetSceneNode("SettingsMenu").transform.Find("Sound").gameObject;
        eventSystem.SetSelectedGameObject(obj);
    }

    public void OpenSoundSettings()
    {
        sceneNodeManager.SetActiveSceneNode("SoundSettings");
        GameObject obj = sceneNodeManager.GetSceneNode("SoundSettings").transform.Find("MusicVolume").gameObject;
        eventSystem.SetSelectedGameObject(obj);
    }

    public void OpenControlSettings()
    {
        sceneNodeManager.SetActiveSceneNode("ControlSettings");
        GameObject obj = sceneNodeManager.GetSceneNode("ControlSettings").transform.Find("AimSensitivity").gameObject;
        eventSystem.SetSelectedGameObject(obj);
    }
    
    public void OpenPlayerSettings()
    {
        sceneNodeManager.SetActiveSceneNode("PlayerSettings");
        GameObject obj = sceneNodeManager.GetSceneNode("PlayerSettings").transform.Find("PlayerBodyRed").gameObject;
        eventSystem.SetSelectedGameObject(obj);
    }

    public void ChangeBodyColor()
    {

        float r = playerBodySliders[0].value;
        float g = playerBodySliders[1].value;
        float b = playerBodySliders[2].value;

        playerBodyColor = new Color(r, g, b);
        playerPreview.GetComponent<MeshRenderer>().materials[0].color = new Color(r, g, b);
    }

    public void ChangeStripeColor()
    {

        float r = playerStripeSliders[0].value;
        float g = playerStripeSliders[1].value;
        float b = playerStripeSliders[2].value;

        playerStripeColor = new Color(r, g, b);
        playerPreview.GetComponent<MeshRenderer>().materials[1].color = new Color(r, g, b);
    }

    public void ChangeBGMVolume()
    {

    }

    public void ChangeSFXVolume()
    {
        
    }
}
