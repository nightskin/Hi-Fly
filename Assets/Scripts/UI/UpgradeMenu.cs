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
            btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Fire Power+";
        }
        else if (c == 1)
        {
            btn.onClick.AddListener(ImproveShipDurability);
            btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Ship Durability+";

        }
        else if (c == 2)
        {
            if (GameManager.Get().mainWeapon == GameManager.RangedWeapon.NORMAL_BULLET)
            {
                btn.onClick.AddListener(ImproveFireRate);
                btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Fire Rate+";
            }
            else
            {
                btn.onClick.AddListener(ImproveChargeSpeed);
                btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Charge Rate+";
            }
        }
        else if(c == 3)
        {
            if(Util.RandomBool())
            {
                int i = Random.Range(0, 3);
                if (i == 0)
                {
                    btn.onClick.AddListener(AddBombOrbiter);
                    btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Add Bomb Orbiter";
                }
                else if (i == 1)
                {
                    btn.onClick.AddListener(AddLazerOrbiter);
                    btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Add Lazer Orbiter";
                }
                else if (i == 2)
                {
                    btn.onClick.AddListener(AddNormalOrbiter);
                    btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Add Turret Orbiter";
                }
            }
            else
            {
                if (GameManager.Get().mainWeapon == GameManager.RangedWeapon.NORMAL_BULLET)
                {
                    if (Util.RandomBool())
                    {
                        btn.onClick.AddListener(ChangeWeaponToLazer);
                        btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Equip Raver Lazer";
                    }
                    else
                    {
                        btn.onClick.AddListener(ChangeWeaponToChargeBlaster);
                        btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Equip Charge Blaster";
                    }
                }
                else if (GameManager.Get().mainWeapon == GameManager.RangedWeapon.CHARGE_BOMB)
                {
                    btn.onClick.AddListener(ChangeWeaponToLazer);
                    btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Equip Raver Lazer";
                }
                else if(GameManager.Get().mainWeapon == GameManager.RangedWeapon.RAVER_LAZER)
                {
                    btn.onClick.AddListener(ChangeWeaponToChargeBlaster);
                    btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Equip Charge Blaster";
                }
            }
        }
    }
    
    void ImproveFirePower()
    {
        PlayerShip.firePower++;
        GameManager.Get().CloseUpgradeMenu();
    }

    void ImproveFireRate()
    {
        PlayerShip.fireRate++;
        GameManager.Get().CloseUpgradeMenu();
    }

    void ImproveShipDurability()
    {
        GameManager.Get().playerShip.health.IncreaseMaxHP(5);
        GameManager.Get().CloseUpgradeMenu();
    }

    void ImproveChargeSpeed()
    {
        PlayerShip.chargeSpeed++;
        GameManager.Get().CloseUpgradeMenu();
    }

    void ChangeWeaponToLazer()
    {
        GameManager.Get().mainWeapon = GameManager.RangedWeapon.RAVER_LAZER;
        GameManager.Get().CloseUpgradeMenu();
    }
    
    void ChangeWeaponToBackToNormal()
    {
        GameManager.Get().mainWeapon = GameManager.RangedWeapon.NORMAL_BULLET;
        GameManager.Get().CloseUpgradeMenu();
    }

    void ChangeWeaponToChargeBlaster()
    {
        GameManager.Get().mainWeapon = GameManager.RangedWeapon.CHARGE_BOMB;
        GameManager.Get().CloseUpgradeMenu();
    }

    void AddLazerOrbiter()
    {
        
        GameManager.Get().CloseUpgradeMenu();
    }
    
    void AddBombOrbiter()
    {
        
        GameManager.Get().CloseUpgradeMenu();
    }

    void AddNormalOrbiter()
    {
        
        GameManager.Get().CloseUpgradeMenu();
    }

}
