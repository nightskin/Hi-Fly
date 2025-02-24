using UnityEngine;

public class Voxel 
{
    public Color color = new Color();
    public bool active = false;
    public float value = 0;
    public Vector3 position = Vector3.zero;
    public int x = 0;
    public int y = 0;
    public int z = 0;

    public Voxel()
    {

    }

    public static int GetState(Voxel[] points)
    {
        int state = 0;
        if (points[0].active) state |= 1;
        if (points[1].active) state |= 2;
        if (points[2].active) state |= 4;
        if (points[3].active) state |= 8;
        if (points[4].active) state |= 16;
        if (points[5].active) state |= 32;
        if (points[6].active) state |= 64;
        if (points[7].active) state |= 128;
        return state;
    }
    
    public static Vector3 GetMidPoint(Voxel point1, Voxel point2)
    {
        return (point1.position + point2.position) / 2;
    }
}