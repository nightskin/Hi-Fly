using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [SerializeField] Slider[] playerBodySliders;
    [SerializeField] Slider[] playerStripeSliders;
    [SerializeField] InputField seedInput;

    [SerializeField] GameObject playerPreview;
    [SerializeField] Galaxy galaxyPreview;

    [SerializeField] EventSystem eventSystem;

    static SceneNodeManager sceneNodeManager;
    
    
    void Start()
    {
        sceneNodeManager = GetComponent<SceneNodeManager>();
    }

    public void ChangeSeed()
    {
        GameManager.seed = seedInput.text;
    }

    public void ExitSettings()
    {
        StartCoroutine(LevelLoader.LoadLevel("Menu"));
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

    public void OpenWorldSettings()
    {
        sceneNodeManager.SetActiveSceneNode("WorldSettings");
        GameObject obj = sceneNodeManager.GetSceneNode("WorldSettings").transform.Find("GenerateButton").gameObject;
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

        GameManager.playerBodyColor = new Color(r, g, b);
        playerPreview.GetComponent<MeshRenderer>().materials[0].color = new Color(r, g, b);
    }

    public void ChangeStripeColor()
    {

        float r = playerStripeSliders[0].value;
        float g = playerStripeSliders[1].value;
        float b = playerStripeSliders[2].value;

        GameManager.playerStripeColor = new Color(r, g, b);
        playerPreview.GetComponent<MeshRenderer>().materials[1].color = new Color(r, g, b);
    }

}
