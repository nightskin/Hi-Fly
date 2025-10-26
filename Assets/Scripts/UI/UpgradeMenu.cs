using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UpgradeMenu : MonoBehaviour
{
    [SerializeField] Transform verticalGroup;
    [SerializeField] Button[] upgradeBtns;

    void OnEnable()
    {
        //Randomize Choices
        for (int i = 0; i < upgradeBtns.Length; i++)
        {
            ChooseUpgrade(upgradeBtns[i], i);
        }

        GameManager.Get().eventSystem.firstSelectedGameObject = verticalGroup.GetChild(0).gameObject;
    }

    void OnDisable()
    {
        for (int i = 0; i < upgradeBtns.Length; i++)
        {
            upgradeBtns[i].onClick.RemoveAllListeners();
        }
    }

    void ChooseUpgrade(Button btn, int c)
    {
        if (c == 0)
        {
            btn.onClick.AddListener(ImproveFirePower);
            btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Fire Power +";
        }
        else if (c == 1)
        {
            btn.onClick.AddListener(ImproveFireRate);
            btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Fire Rate+";
        }
        else if (c == 2)
        {
            btn.onClick.AddListener(ImproveShipDurability);
            btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Ship Durability+";
        }
    }
    
    public void ImproveFirePower()
    {
        PlayerShip.firePower++;
        GameManager.Get().CloseUpgradeMenu();
    }

    public void ImproveFireRate()
    {
        if (PlayerShip.fireRate > 0.01f)
        {
            PlayerShip.fireRate -= 0.01f;
        }
        GameManager.Get().CloseUpgradeMenu();
    }

    public void ImproveShipDurability()
    {
        GameManager.Get().playerShip.health.IncreaseMaxHP(5);
        GameManager.Get().CloseUpgradeMenu();
    }

    public void AddRaverLazerOrbiter()
    {
        
        GameManager.Get().CloseUpgradeMenu();
    }
    public void AddChargeBlasterOrbiter()
    {
        
        GameManager.Get().CloseUpgradeMenu();
    }
    public void AddNormalOrbiter()
    {
        
        GameManager.Get().CloseUpgradeMenu();
    }

}
