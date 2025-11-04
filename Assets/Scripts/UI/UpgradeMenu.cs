using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Net.NetworkInformation;

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
                btn.onClick.AddListener(ImproveBaseFirePower);
                btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Base Fire Power+";
            }
        }
        //Improve Ranged Weapons
        else if (c == 1)
        {
            if (GameManager.Get().playerShip.rangedWeapon == PlayerShip.RangedWeapon.CHARGE_MISSILE)
            {
                int r = Random.Range(0, 3);
                if(r== 0)
                {
                    btn.onClick.AddListener(ImproveChargeSpeed);
                    btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Charge Rate +";
                }
                else if(r == 1)
                {
                    btn.onClick.AddListener(ImproveMissilePower);
                    btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Charge Shot Power +";
                }
                else if(r == 2)
                {
                    btn.onClick.AddListener(ImproveBlastRadius);
                    btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Blast Radius +";
                }
            }
            else if(GameManager.Get().playerShip.rangedWeapon == PlayerShip.RangedWeapon.RAVER_LAZER)
            {
                if(Util.RandomBool())
                {
                    btn.onClick.AddListener(ImproveLazerPower);
                    btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Lazer Power +";
                }
                else
                {
                    btn.onClick.AddListener(ImproveLazerSpeed);
                    btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Lazer Speed +";
                }
            }
            else if (GameManager.Get().playerShip.rangedWeapon == PlayerShip.RangedWeapon.MULTI_SHOT)
            {
                int i = Random.Range(0, 4);
                if(i == 0)
                {
                    if(GameManager.Get().playerShip.explodingBullets)
                    {
                        btn.onClick.AddListener(ImproveBlastRadius);
                        btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Blast Radius +";
                    }
                    else
                    {
                        btn.onClick.AddListener(GetExplodingBullets);
                        btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Exploding Bullets";
                    }
                }
                else
                {
                    btn.onClick.AddListener(ImproveMultiShot);
                    btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Max Targets +";
                }

            }
        }
        //Get new Weapon
        else if(c == 2)
        {
            int turretType = Random.Range(0, 4);
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
            else if(turretType == 3)
            {
                btn.onClick.AddListener(GetDrillDasher);
                btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Drill Dasher";
            }
        }
    }
    
    void ImproveMissilePower()
    {
        GameManager.Get().playerShip.missileMult++;
        GameManager.Get().CloseUpgradeMenu();
    }

    void ImproveBaseFirePower()
    {
        GameManager.Get().playerShip.baseFirePower++;
        GameManager.Get().CloseUpgradeMenu();
    }
    
    void ImproveBlastRadius()
    {
        GameManager.Get().playerShip.blastRadius += 5;
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

    void ImproveLazerPower()
    {
        GameManager.Get().playerShip.lazerPower++;
        GameManager.Get().CloseUpgradeMenu();
    }
    
    void ImproveLazerSpeed()
    {
        GameManager.Get().playerShip.lazerSpeed += 0.01f;
        GameManager.Get().CloseUpgradeMenu();
    }

    void ImproveMultiShot()
    {
        GameManager.Get().playerShip.maxTargets++;
        GameManager.Get().CloseUpgradeMenu();
    }

    void GetExplodingBullets()
    {
        GameManager.Get().playerShip.explodingBullets = true;
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
