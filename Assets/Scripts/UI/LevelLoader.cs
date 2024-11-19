using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    void Awake()
    {
        for(int c = 0; c < transform.childCount; c++)
        {
            if(transform.GetChild(c).name == HubWorld.levelId)
            {
                transform.GetChild(c).gameObject.SetActive(true);
            }
            else
            {
                transform.GetChild(c).gameObject.SetActive(false);
            }
        }
    }
}
