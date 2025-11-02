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
            if(Util.RandomBool())
            {
                btn.onClick.AddListener(ImproveShipDurability);
                btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Ship Durability+";
            }
            else
            {
                btn.onClick.AddListener(RepairShip);
                btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Repair Ship";
            }
        }
        else if (c == 1)
        {
            bool b = Util.RandomBool();
            if (GameManager.Get().playerShip.weapon == PlayerShip.RangedWeapon.CHARGE_MISSILE && b)
            {
                btn.onClick.AddListener(ImproveChargeSpeed);
                btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Charge Rate+";
            }
            else if(GameManager.Get().playerShip.weapon == PlayerShip.RangedWeapon.CHARGE_MISSILE && !b)
            {
                btn.onClick.AddListener(ImproveMissilePower);
                btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Missile Power+";
            }
            else
            {
                btn.onClick.AddListener(ImproveFirePower);
                btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Fire Power+";
            }
        }
        else if(c == 2)
        {
            int r = Random.Range(0, 3);
            //Change Melee Weapon
            if(r == 1)
            {
                if(GameManager.Get().playerShip.meleeWeapon == PlayerShip.MeleeWeapon.NONE)
                {
                    btn.onClick.AddListener(GetDrillDasher);
                    btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Get Drill Dasher";
                }
                else
                {

                }

            }
            // Change Primary Weapon
            else if (r == 1)
            {
                //Change Primary Weapon
                if (GameManager.Get().playerShip.weapon == PlayerShip.RangedWeapon.MULTI_SHOT)
                {
                    if (Util.RandomBool())
                    {
                        btn.onClick.AddListener(ChangeRangedWeaponToLazer);
                        btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Equip Raver Lazer";
                    }
                    else
                    {
                        btn.onClick.AddListener(ChangeRangedWeaponToChargeBlaster);
                        btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Equip Charge Blaster";
                    }
                }
                else if (GameManager.Get().playerShip.weapon == PlayerShip.RangedWeapon.CHARGE_MISSILE)
                {
                    if (Util.RandomBool())
                    {
                        btn.onClick.AddListener(ChangeRangedWeaponToLazer);
                        btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Equip Raver Lazer";
                    }
                    else
                    {
                        btn.onClick.AddListener(ChangeRangedWeaponToBackToNormal);
                        btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Equip Starting Weapon";
                    }
                }
                else if (GameManager.Get().playerShip.weapon == PlayerShip.RangedWeapon.RAVER_LAZER)
                {
                    if (Util.RandomBool())
                    {
                        btn.onClick.AddListener(ChangeRangedWeaponToChargeBlaster);
                        btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Equip Charge Blaster";
                    }
                    else
                    {
                        btn.onClick.AddListener(ChangeRangedWeaponToBackToNormal);
                        btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Equip Starting Weapon";
                    }
                }
            }
            // Add Turret
            if (r == 2)
            {
                int turretType = Random.Range(0, 3);
                if (turretType == 0)
                {
                    btn.onClick.AddListener(AddBombTurret);
                    btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Add Missile Orbiter";
                }
                else if (turretType == 1)
                {
                    btn.onClick.AddListener(AddLazerTurret);
                    btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Add Lazer Orbiter";
                }
                else if (turretType == 2)
                {
                    btn.onClick.AddListener(AddNormalTurret);
                    btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Add Turret Orbiter";
                }
            }
        }
    }
    
    void ImproveMissilePower()
    {
        GameManager.Get().playerShip.missilePower++;
        GameManager.Get().CloseUpgradeMenu();
    }

    void ImproveFirePower()
    {
        GameManager.Get().playerShip.firePower++;
        GameManager.Get().CloseUpgradeMenu();
    }
    
    void RepairShip()
    {
        GameManager.Get().playerShip.health.Heal(GameManager.Get().playerShip.health.MaxHP());
        GameManager.Get().CloseUpgradeMenu();
    }

    void ImproveShipDurability()
    {
        GameManager.Get().playerShip.health.IncreaseMaxHP(5);
        GameManager.Get().CloseUpgradeMenu();
    }

    void ImproveChargeSpeed()
    {
        GameManager.Get().playerShip.chargeSpeed++;
        GameManager.Get().CloseUpgradeMenu();
    }

    void ChangeRangedWeaponToLazer()
    {
        GameManager.Get().playerShip.weapon = PlayerShip.RangedWeapon.RAVER_LAZER;
        GameManager.Get().CloseUpgradeMenu();
    }
    
    void ChangeRangedWeaponToBackToNormal()
    {
        GameManager.Get().playerShip.weapon = PlayerShip.RangedWeapon.MULTI_SHOT;
        GameManager.Get().CloseUpgradeMenu();
    }

    void ChangeRangedWeaponToChargeBlaster()
    {
        GameManager.Get().playerShip.weapon = PlayerShip.RangedWeapon.CHARGE_MISSILE;
        GameManager.Get().CloseUpgradeMenu();
    }

    void GetDrillDasher()
    {
        GameManager.Get().playerShip.drillDasher.SetActive(true);
        GameManager.Get().CloseUpgradeMenu();
    }

    void AddLazerTurret()
    {
        
        GameManager.Get().CloseUpgradeMenu();
    }
    
    void AddBombTurret()
    {
        
        GameManager.Get().CloseUpgradeMenu();
    }

    void AddNormalTurret()
    {
        
        GameManager.Get().CloseUpgradeMenu();
    }

}
