using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sky : MonoBehaviour
{
    [SerializeField] Color color;
    [SerializeField] float alpha;
    [SerializeField] float skySize = 500;
    
    bool playerInside;
    Transform player;
    Planet planet;
    MeshRenderer meshRenderer;

    void Start()
    {
        planet = transform.parent.GetComponent<Planet>();
        transform.localScale = Vector3.one * (planet.radius * 2 + skySize);
        meshRenderer = transform. GetComponent<MeshRenderer>();

        alpha = 0;
        color = planet.waterColor;
        meshRenderer.material.SetColor("_SkyColor", color);
        meshRenderer.material.SetColor("_CloudColor", color * 2);
        meshRenderer.material.SetFloat("_Alpha", alpha);
    }

    void FixedUpdate()
    {
        if(playerInside)
        {
            alpha = Mathf.Lerp(alpha, 1, 10 * Time.fixedDeltaTime);
            meshRenderer.material.SetFloat("_Alpha", alpha);
        }
        else
        {
            alpha = Mathf.Lerp(alpha, 0, 10 * Time.fixedDeltaTime);
            meshRenderer.material.SetFloat("_Alpha", alpha);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            player = other.transform;
            playerInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            playerInside = false;
        }
    }


}
