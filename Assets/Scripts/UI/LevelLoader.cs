using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader instance { get; private set; }

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

    void Start()
    {
        animator = GetComponent<Animator>();
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator LoadLevel(string name, float duration = 1)
    {
        animator.SetTrigger("start");

        yield return new WaitForSeconds(duration);

        SceneManager.LoadScene(name);
    }

    public IEnumerator LoadLevel(int index, float duration = 1) 
    {
        animator.SetTrigger("start");

        yield return new WaitForSeconds(duration);

        SceneManager.LoadScene(index);
    }
}
