using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] Image characterIcon;
    [SerializeField] TextMeshProUGUI dialogueTextBox;
    [SerializeField] TextMeshProUGUI characterNameTextBox;


    static GameObject dialogueManagerGameObject;
    static Image currentCharacter;
    static TextMeshProUGUI currentDialogue;
    static TextMeshProUGUI currentCharacterName;

    void Awake()
    {
        currentCharacter = characterIcon;
        currentDialogue = dialogueTextBox;
        currentCharacterName = characterNameTextBox;
        dialogueManagerGameObject = gameObject;
    }

    public static void ShowDialogue(DialogueObject dialogue)
    {
        currentCharacter.sprite = dialogue.speakingCharacterIcon;
        currentCharacterName.text = dialogue.speakingCharacterName;
        currentDialogue.text = dialogue.whatCharacterIsSaying; 
        dialogueManagerGameObject.SetActive(true);
    }

    public static void HideDialogue()
    {
        dialogueManagerGameObject.SetActive(false);
    }
    

}
