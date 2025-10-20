using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    string lastMenuOpen = "";
    [SerializeField] Slider aimSlider;
    [SerializeField] Text aimText;

    [SerializeField] Slider musicVolume;
    [SerializeField] Slider sfxVolume;

    [SerializeField] Toggle invertSteerY;
    [SerializeField] Toggle invertLookY;

    [SerializeField] Button difficultyBtn;
    [SerializeField] EventSystem eventSystem;

    static SceneNodeManager sceneNodeManager;
    
    
    void Start()
    {
        sceneNodeManager = GetComponent<SceneNodeManager>();
        aimSlider.value = GameSettings.aimSensitivy;
        difficultyBtn.transform.GetChild(0).GetComponent<Text>().text = GameSettings.difficulty.ToString();
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
        GameObject obj = sceneNodeManager.GetSceneNode("SettingsMenu").transform.Find(lastMenuOpen).gameObject;
        eventSystem.SetSelectedGameObject(obj);
    }

    public void OpenSoundSettings()
    {
        lastMenuOpen = "Sound";
        sceneNodeManager.SetActiveSceneNode("SoundSettings");
        GameObject obj = sceneNodeManager.GetSceneNode("SoundSettings").transform.Find("MusicVolume").gameObject;
        eventSystem.SetSelectedGameObject(obj);
    }

    public void OpenGameSettings()
    {
        lastMenuOpen = "Game";
        sceneNodeManager.SetActiveSceneNode("GameSettings");
        GameObject obj = sceneNodeManager.GetSceneNode("GameSettings").transform.GetChild(2).gameObject;
        eventSystem.SetSelectedGameObject(obj);
    }

    public void OpenControlSettings()
    {
        lastMenuOpen = "Control";
        sceneNodeManager.SetActiveSceneNode("ControlSettings");
        GameObject obj = sceneNodeManager.GetSceneNode("ControlSettings").transform.Find("AimSensitivity").gameObject;
        eventSystem.SetSelectedGameObject(obj);
    }
    
    public void OpenGraphicSettings()
    {
        lastMenuOpen = "Graphics";
        sceneNodeManager.SetActiveSceneNode("GraphicSettings");
        GameObject obj = sceneNodeManager.GetSceneNode("GraphicSettings").transform.GetChild(0).gameObject;
        eventSystem.SetSelectedGameObject(obj);
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
