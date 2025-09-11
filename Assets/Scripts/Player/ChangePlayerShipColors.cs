using UnityEngine;

public class ChangePlayerShipColors : MonoBehaviour
{
    void Start()
    {
        GetComponent<MeshRenderer>().materials[0].SetColor("_MainColor", GameSettings.playerBodyColor);
        GetComponent<MeshRenderer>().materials[1].SetColor("_MainColor", GameSettings.playerStripeColor);
    }

}
