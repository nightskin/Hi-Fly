using UnityEngine;

public abstract class PlayerWeapon
{
    public PlayerShip owner;
    public string name;
    public abstract void FireReleased();
    public abstract void FirePressed();
    public abstract void FireDown();
    public abstract void Init();
    public abstract void Update();
}
