using UnityEngine;
public class Square
{
    Voxel[] points;

    public Vector3 bottomLeft;
    public Vector3 topLeft;
    public Vector3 topRight;
    public Vector3 bottomRight;

    public Vector3 centerLeft;
    public Vector3 centerRight;
    public Vector3 centerTop;
    public Vector3 centerBottom;

    public Square(Voxel[] corners)
    {
        this.points = corners;


        bottomLeft = corners[0].position;
        topLeft = corners[1].position;
        topRight = corners[2].position;
        bottomRight = corners[3].position;

        float tb = (GameManager.isoLevel - corners[0].value) / (corners[3].value - corners[0].value);
        centerBottom = Vector3.Lerp(bottomLeft, bottomRight, tb);

        float tt = (GameManager.isoLevel - corners[1].value) / (corners[2].value - corners[1].value);
        centerTop = Vector3.Lerp(topLeft, topRight, tt);

        float tl = (GameManager.isoLevel - corners[0].value) / (corners[1].value - corners[0].value);
        centerLeft = Vector3.Lerp(bottomLeft, topLeft, tl);

        float tr = (GameManager.isoLevel - corners[2].value) / (corners[3].value - corners[2].value);
        centerRight = Vector3.Lerp(topRight, bottomRight, tr);
    }

    public Vector3 Z(Vector3 point, float z = 10)
    {
        return new Vector3(point.x, point.y, z);
    }
}
