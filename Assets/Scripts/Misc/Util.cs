using UnityEngine;


public static class Util
{
    
    public static float ConstrainAngle(float v)
    {
        v -= 360 * Mathf.Floor((v + 180.0f) * (1.0f / 360.0f));
        return v;
    }

    public static Vector3 RandomVector3()
    {
        float x = Random.Range(float.MinValue, float.MaxValue);
        float y = Random.Range(float.MinValue, float.MaxValue);
        float z = Random.Range(float.MinValue, float.MaxValue);

        return new Vector3(x, y, z);
    }

    public static Vector3 RandomVector3(float min, float max)
    {
        float x = Random.Range(min, max);
        float y = Random.Range(min, max);
        float z = Random.Range(min, max);

        return new Vector3(x, y, z);
    }

    public static Vector3 RandomVector3(Vector3 min, Vector3 max) 
    {
        float x = Random.Range(min.x, max.x);
        float y = Random.Range(min.y, max.y);
        float z = Random.Range(min.z, max.z);

        return new Vector3(x, y, z);
    }

    public static Vector3 RandomPositionInBox(Vector3 boxSize)
    {
        float x = Random.Range(-boxSize.x/2, boxSize.x/2);
        float y = Random.Range(-boxSize.y/2, boxSize.y/2);
        float z = Random.Range(-boxSize.z/2, boxSize.z/2);
        return new Vector3(x, y, z);
    }

    public static Quaternion RandomRotation()
    {
        float x = Random.Range(-180, 180);
        float y = Random.Range(-180, 180);
        float z = Random.Range(-180, 180);
        return Quaternion.Euler(x, y, z);
    }
    
    public static Quaternion RandomRotation(Vector3 min, Vector3 max)
    {
        float x = Random.Range(min.x, max.x);
        float y = Random.Range(min.y, max.y);
        float z = Random.Range(min.z, max.z);
        return Quaternion.Euler(x, y, z);
    }

    public static Color RandomColor()
    {
        float r = Random.value;
        float g = Random.value;
        float b = Random.value;
        return new Color(r, g, b);
    }

    public static Color RandomColorAlpha()
    {
        float r = Random.value;
        float g = Random.value;
        float b = Random.value;
        float a = Random.value;
        return new Color(r, g, b, a);
    }

    public static bool RandomBool() 
    {
        int b = Mathf.RoundToInt(Random.value);
        return b == 1;
    }
    
    public static float ConvertRange(float OldMin, float OldMax, float NewMin, float NewMax, float value)
    {
        float OldRange = (OldMax - OldMin);
        if (OldRange == 0)
        {
            return NewMin;
        }
        else
        {
            float NewRange = (NewMax - NewMin);
            return (((value - OldMin) * NewRange) / OldRange) + NewMin;
        }
    }
}
