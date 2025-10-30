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
            if (GameManager.Get().playerShip.weapon == PlayerShip.Weapon.NORMAL_BULLET)
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
            //Get Drill Dasher
            if(Util.RandomBool() && GameManager.Get().playerShip.drillDasher.activeSelf == false)
            {
                btn.onClick.AddListener(GetDrillDasher);
                btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Get Drill Dasher";
            }
            //Get Extra Orbiter
            if(Util.RandomBool())
            {
                int i = Random.Range(0, 3);
                if (i == 0)
                {
                    btn.onClick.AddListener(AddBombOrbiter);
                    btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Add Missile Orbiter";
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
            // Change Primary Weapon
            else
            {
                //Change Primary Weapon
                if (GameManager.Get().playerShip.weapon == PlayerShip.Weapon.NORMAL_BULLET)
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
                else if (GameManager.Get().playerShip.weapon == PlayerShip.Weapon.CHARGE_BOMB)
                {
                    if(Util.RandomBool())
                    {
                        btn.onClick.AddListener(ChangeWeaponToLazer);
                        btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Equip Raver Lazer";
                    }
                    else
                    {
                        btn.onClick.AddListener(ChangeWeaponToBackToNormal);
                        btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Equip Starting Weapon";
                    }
                }
                else if(GameManager.Get().playerShip.weapon == PlayerShip.Weapon.RAVER_LAZER)
                {
                    if(Util.RandomBool())
                    {
                        btn.onClick.AddListener(ChangeWeaponToChargeBlaster);
                        btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Equip Charge Blaster";
                    }
                    else
                    {
                        btn.onClick.AddListener(ChangeWeaponToBackToNormal);
                        btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Equip Starting Weapon";
                    }
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
        GameManager.Get().playerShip.weapon = PlayerShip.Weapon.RAVER_LAZER;
        GameManager.Get().CloseUpgradeMenu();
    }
    
    void ChangeWeaponToBackToNormal()
    {
        GameManager.Get().playerShip.weapon = PlayerShip.Weapon.NORMAL_BULLET;
        GameManager.Get().CloseUpgradeMenu();
    }

    void ChangeWeaponToChargeBlaster()
    {
        GameManager.Get().playerShip.weapon = PlayerShip.Weapon.CHARGE_BOMB;
        GameManager.Get().CloseUpgradeMenu();
    }

    void GetDrillDasher()
    {
        GameManager.Get().playerShip.drillDasher.SetActive(true);
        GameManager.Get().CloseUpgradeMenu();
    }

    void AddLazerOrbiter()
    {
        GameManager.Get().playerShip.orbiters.AddOrbiter(Orbiter.Type.RAVER_LAZER);
        GameManager.Get().CloseUpgradeMenu();
    }
    
    void AddBombOrbiter()
    {
        GameManager.Get().playerShip.orbiters.AddOrbiter(Orbiter.Type.MISSILE);
        GameManager.Get().CloseUpgradeMenu();
    }

    void AddNormalOrbiter()
    {
        GameManager.Get().playerShip.orbiters.AddOrbiter(Orbiter.Type.TURRET);
        GameManager.Get().CloseUpgradeMenu();
    }

}
