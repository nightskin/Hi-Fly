using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    static Animator animator;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        DontDestroyOnLoad(gameObject);
    }

    public static IEnumerator LoadLevel(string name, float duration = 1)
    {
        animator.SetTrigger("start");

        yield return new WaitForSeconds(duration);

        SceneManager.LoadScene(name);
    }

}
