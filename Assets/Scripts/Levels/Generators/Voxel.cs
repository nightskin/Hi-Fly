using UnityEngine;


[System.Serializable] public struct Voxel
{
    public Color color;
    public float value;
    public Vector3 position;

    public static Vector3 IndexToPosition(int i, int voxelResolution, float voxelSize)
    {
        int x = i % voxelResolution;
        int y = i / voxelResolution % voxelResolution;
        int z = i / voxelResolution / voxelResolution % voxelResolution;
        return new Vector3(x, y, z) * voxelSize;
    }

    public static int PositionToIndex(Vector3 position, int voxelResolution ,float voxelSize)
    {
        return ((int)(position.x / voxelSize)) + ((int)(position.y / voxelSize)  * voxelResolution) + ((int)(position.z / voxelSize) * voxelResolution * voxelResolution);
    }


    public static Vector2[] GetUVs(Vector3 a, Vector3 b, Vector3 c, float voxelSize)
    {
        Vector3 s1 = b - a;
        Vector3 s2 = c - a;
        Vector3 norm = Vector3.Cross(s1, s2).normalized; // the normal

        norm.x = Mathf.Abs(norm.x);
        norm.y = Mathf.Abs(norm.y);
        norm.z = Mathf.Abs(norm.z);

        Vector2[] uvs = new Vector2[3];
        if (norm.x >= norm.z && norm.x >= norm.y) // x plane
        {
            uvs[0] = new Vector2(a.z, a.y) / voxelSize;
            uvs[1] = new Vector2(b.z, b.y) / voxelSize;
            uvs[2] = new Vector2(c.z, c.y) / voxelSize;
        }
        else if (norm.z >= norm.x && norm.z >= norm.y) // z plane
        {
            uvs[0] = new Vector2(a.x, a.y) / voxelSize;
            uvs[1] = new Vector2(b.x, b.y) / voxelSize;
            uvs[2] = new Vector2(c.x, c.y) / voxelSize;
        }
        else if (norm.y >= norm.x && norm.y >= norm.z) // y plane
        {
            uvs[0] = new Vector2(a.x, a.z) / voxelSize;
            uvs[1] = new Vector2(b.x, b.z) / voxelSize;
            uvs[2] = new Vector2(c.x, c.z) / voxelSize;
        }

        return uvs;
    }

    public static int GetState(Voxel[] voxels, float isoLevel)
    {
        int state = 0;
        if (voxels[0].value > isoLevel) state |= 1;
        if (voxels[1].value > isoLevel) state |= 2;
        if (voxels[2].value > isoLevel) state |= 4;
        if (voxels[3].value > isoLevel) state |= 8;
        if (voxels[4].value > isoLevel) state |= 16;
        if (voxels[5].value > isoLevel) state |= 32;
        if (voxels[6].value > isoLevel) state |= 64;
        if (voxels[7].value > isoLevel) state |= 128;
        return state;
    }

    public static int GetState(float[] values, float isoLevel)
    {
        int state = 0;
        if (values[0] > isoLevel) state |= 1;
        if (values[1] > isoLevel) state |= 2;
        if (values[2] > isoLevel) state |= 4;
        if (values[3] > isoLevel) state |= 8;
        if (values[4] > isoLevel) state |= 16;
        if (values[5] > isoLevel) state |= 32;
        if (values[6] > isoLevel) state |= 64;
        if (values[7] > isoLevel) state |= 128;
        return state;
    }

    public static int GetStateInverted(Voxel[] voxels, float isoLevel)
    {
        int state = 0;
        if (voxels[0].value < isoLevel) state |= 1;
        if (voxels[1].value < isoLevel) state |= 2;
        if (voxels[2].value < isoLevel) state |= 4;
        if (voxels[3].value < isoLevel) state |= 8;
        if (voxels[4].value < isoLevel) state |= 16;
        if (voxels[5].value < isoLevel) state |= 32;
        if (voxels[6].value < isoLevel) state |= 64;
        if (voxels[7].value < isoLevel) state |= 128;
        return state;
    }

    public static int GetStateInverted(float[] values, float isoLevel)
    {
        int state = 0;
        if (values[0] < isoLevel) state |= 1;
        if (values[1] < isoLevel) state |= 2;
        if (values[2] < isoLevel) state |= 4;
        if (values[3] < isoLevel) state |= 8;
        if (values[4] < isoLevel) state |= 16;
        if (values[5] < isoLevel) state |= 32;
        if (values[6] < isoLevel) state |= 64;
        if (values[7] < isoLevel) state |= 128;
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