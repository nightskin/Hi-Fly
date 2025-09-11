using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    [SerializeField] Slider[] playerBodySliders;
    [SerializeField] Slider[] playerStripeSliders;
    [SerializeField] Slider aimSlider;
    [SerializeField] Text aimText;

    [SerializeField] Slider musicVolume;
    [SerializeField] Slider sfxVolume;

    [SerializeField] Toggle invertSteerY;
    [SerializeField] Toggle invertLookY;

    [SerializeField] GameObject playerPreview;
    [SerializeField] Button difficultyBtn;
    [SerializeField] EventSystem eventSystem;

    static SceneNodeManager sceneNodeManager;
    
    
    void Start()
    {
        sceneNodeManager = GetComponent<SceneNodeManager>();
        aimSlider.value = GameSettings.aimSensitivy;
        difficultyBtn.transform.GetChild(0).GetComponent<Text>().text = GameSettings.difficulty.ToString();
        playerBodySliders[0].value = GameSettings.playerBodyColor.r;
        playerBodySliders[1].value = GameSettings.playerBodyColor.g;
        playerBodySliders[2].value = GameSettings.playerBodyColor.b;
        playerPreview.GetComponent<MeshRenderer>().materials[0].SetColor("_MainColor", GameSettings.playerBodyColor);
        playerStripeSliders[0].value = GameSettings.playerStripeColor.r;
        playerStripeSliders[1].value = GameSettings.playerStripeColor.g;
        playerStripeSliders[2].value = GameSettings.playerStripeColor.b;
        playerPreview.GetComponent<MeshRenderer>().materials[1].SetColor("_MainColor", GameSettings.playerStripeColor);
        musicVolume.value = SoundManager.audioSource.volume;
        sfxVolume.value = SoundManager.audioSource.volume;
    }

    public void ToggleDifficulty()
    {
        if(GameSettings.difficulty == GameSettings.Difficulty.EASY)
        {
            GameSettings.difficulty = GameSettings.Difficulty.NORMAL;
        }
        else if(GameSettings.difficulty == GameSettings.Difficulty.NORMAL)
        {
            GameSettings.difficulty = GameSettings.Difficulty.HARD;
        }
        else if(GameSettings.difficulty == GameSettings.Difficulty.HARD)
        {
            GameSettings.difficulty = GameSettings.Difficulty.EASY;
        }
        
        difficultyBtn.transform.GetChild(0).GetComponent<Text>().text = GameSettings.difficulty.ToString();
    }

    public void ChangeAimSense()
    {
        GameSettings.aimSensitivy = aimSlider.value;
        aimText.text = GameSettings.aimSensitivy.ToString();
    }

    public void ExitSettings()
    {
        eventSystem.gameObject.SetActive(false);
        StartCoroutine(SceneLoader.instance.Load("Menu"));
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

    public void OpenGameSettings()
    {
        sceneNodeManager.SetActiveSceneNode("GameSettings");
        GameObject obj = sceneNodeManager.GetSceneNode("GameSettings").transform.GetChild(2).gameObject;
        eventSystem.SetSelectedGameObject(obj);
    }

    public void OpenControlSettings()
    {
        sceneNodeManager.SetActiveSceneNode("ControlSettings");
        GameObject obj = sceneNodeManager.GetSceneNode("ControlSettings").transform.Find("AimSensitivity").gameObject;
        eventSystem.SetSelectedGameObject(obj);
    }
    
    public void OpenExtraSettings()
    {
        sceneNodeManager.SetActiveSceneNode("ExtraSettings");
        GameObject obj = sceneNodeManager.GetSceneNode("ExtraSettings").transform.Find("PlayerBodyRed").gameObject;
        eventSystem.SetSelectedGameObject(obj);
    }

    public void OpenGraphicSettings()
    {
        sceneNodeManager.SetActiveSceneNode("GraphicSettings");
        GameObject obj = sceneNodeManager.GetSceneNode("GraphicSettings").transform.GetChild(0).gameObject;
        eventSystem.SetSelectedGameObject(obj);
    }

    public void ChangeBodyColor()
    {

        float r = playerBodySliders[0].value;
        float g = playerBodySliders[1].value;
        float b = playerBodySliders[2].value;

        GameSettings.playerBodyColor = new Color(r, g, b);
        playerPreview.GetComponent<MeshRenderer>().materials[0].SetColor("_MainColor", new Color(r,g,b));
    }

    public void ChangeStripeColor()
    {

        float r = playerStripeSliders[0].value;
        float g = playerStripeSliders[1].value;
        float b = playerStripeSliders[2].value;

        GameSettings.playerStripeColor = new Color(r, g, b);
        playerPreview.GetComponent<MeshRenderer>().materials[1].SetColor("_MainColor", new Color(r,g,b));
    }


    public void ChangeBGMVolume()
    {
        SoundManager.audioSource.volume = musicVolume.value;
    }

    public void ChangeSFXVolume()
    {
        SoundManager.sfxVolume = sfxVolume.value;
    }
}
