using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader instance { get; private set; }

    Animator animator;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator LoadLevel(string name)
    {
        InputManager.input.Disable();
        animator.SetTrigger("start");

        yield return new WaitForSeconds(1);

        SceneManager.LoadScene(name);
    }

    public IEnumerator LoadLevel(int index) 
    {
        InputManager.input.Disable();
        animator.SetTrigger("start");

        yield return new WaitForSeconds(1);

        SceneManager.LoadScene(index);
    }
}
