using UnityEngine;

public class SceneNodeManager : MonoBehaviour
{
    [SerializeField] GameObject[] sceneNodes;

    public GameObject FindObjectInSceneNode(string sceneName, string objectName)
    {
        foreach (GameObject sceneNode in sceneNodes)
        {
            if (sceneNode.name == sceneName)
            {
                return sceneNode.transform.Find(objectName).gameObject;
            }
        }
        return null;
    }

    public GameObject GetSceneNode(string name)
    {
        foreach(GameObject sceneNode in sceneNodes)
        {
            if(sceneNode.name == name) return sceneNode;
        }
        return null;
    }

    public GameObject GetSceneNode(int index)
    {
        for(int i = 0; i < sceneNodes.Length; i++)
        {
            if (i == index) return sceneNodes[i];
        }
        return null;
    }

    public void SetActiveSceneNode(string sceneName)
    {
        foreach (GameObject sceneNode in sceneNodes)
        {
            if (sceneNode.name == sceneName)
            {
                sceneNode.SetActive(true);
            }
            else
            {
                sceneNode.SetActive(false);
            }
        }
    }
    
    public void SetActiveSceneNode(int index)
    {
        if (index >= sceneNodes.Length || index < 0)
        {
            Debug.Log("Scene At Index " + index + " Does Not Exist");
            return;
        }

        for (int i = 0; i < sceneNodes.Length; i++)
        {
            if (i == index)
            {
                sceneNodes[i].SetActive(true);
            }
            else
            {
                sceneNodes[i].SetActive(false);
            }
        }
    }

    public void SetSceneNode(string sceneName, bool active)
    {
        foreach(GameObject sceneNode in sceneNodes)
        {
            if(sceneNode.name == sceneName)
            {
                sceneNode.SetActive(active);
                return;
            }
        }
    }

    public void SetSceneNode(int index, bool active)
    {
        if (index >= sceneNodes.Length || index < 0)
        {
            Debug.Log("Scene At Index " + index + " Does Not Exist");
            return;
        }

        for(int i = 0; i < sceneNodes.Length; i++) 
        {
            if(i == index) 
            {
                sceneNodes[i].SetActive(active);
                return;
            }
        }
    }

}
