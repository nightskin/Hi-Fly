using UnityEngine;

public struct Voxel 
{
    public Color color;
    public float value;
    public Vector3 position;
    public Vector3Int index3D;
    public int index;
    
    public static int GetState(Voxel[] voxels)
    {
        int state = 0;
        if (voxels[0].value >= GameManager.isoLevel) state |= 1;
        if (voxels[1].value >= GameManager.isoLevel) state |= 2;
        if (voxels[2].value >= GameManager.isoLevel) state |= 4;
        if (voxels[3].value >= GameManager.isoLevel) state |= 8;
        if (voxels[4].value >= GameManager.isoLevel) state |= 16;
        if (voxels[5].value >= GameManager.isoLevel) state |= 32;
        if (voxels[6].value >= GameManager.isoLevel) state |= 64;
        if (voxels[7].value >= GameManager.isoLevel) state |= 128;
        return state;
    }

    public static int GetState(Voxel[] voxels, float isoLevel)
    {
        int state = 0;
        if (voxels[0].value >= isoLevel) state |= 1;
        if (voxels[1].value >= isoLevel) state |= 2;
        if (voxels[2].value >= isoLevel) state |= 4;
        if (voxels[3].value >= isoLevel) state |= 8;
        if (voxels[4].value >= isoLevel) state |= 16;
        if (voxels[5].value >= isoLevel) state |= 32;
        if (voxels[6].value >= isoLevel) state |= 64;
        if (voxels[7].value >= isoLevel) state |= 128;
        return state;
    }

    public static Vector3 MidPoint(Voxel v1, Voxel v2)
    {
        return (v1.position + v2.position) / 2;
    }

    public static Vector3 LerpPoint(Voxel v1, Voxel v2, float isoLevel)
    {
        float t = (isoLevel - v1.value) / (v2.value - v1.value);
        return Vector3.Lerp(v1.position, v2.position, t);
    }

}