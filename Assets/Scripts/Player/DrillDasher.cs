using UnityEngine;

public class DrillDasher : MonoBehaviour
{
    [SerializeField] BoxCollider collider;
    [SerializeField] Transform[] quadrants;
    [SerializeField] float maxTurn = 35;
    
    float rotationSpd = 45;
    Vector3[] startPositions;
    float z = 0;

    void Start()
    {
        collider.enabled = false;
        startPositions = new Vector3[quadrants.Length];
        for (int i = 0; i < quadrants.Length; i++)
        {
            startPositions[i] = quadrants[i].localPosition;
        }
    }

    void Update()
    {
        z += rotationSpd * Time.deltaTime;
        Vector2 steerInput = InputManager.player.Steer.ReadValue<Vector2>() * maxTurn;
        transform.localEulerAngles = new Vector3(steerInput.y, steerInput.x, z);

        if(InputManager.player.Melee.IsPressed())
        {
            StartDrilling();
        }
        else
        {
            StopDrilling();
        }
    }

    void StartDrilling()
    {
        rotationSpd = 500;
        for (int i = 0; i < quadrants.Length; i++)
        {
            quadrants[i].transform.localPosition = Vector3.Lerp(quadrants[i].localPosition, Vector3.zero, 10 * Time.deltaTime);
        }
        if (quadrants[3].localPosition ==  Vector3.zero && !collider.enabled)
        {
            collider.enabled = true;
        }
    }

    void StopDrilling()
    {
        rotationSpd = 45;
        for (int i = 0; i < quadrants.Length; i++)
        {
            quadrants[i].transform.localPosition = Vector3.Lerp(quadrants[i].localPosition, startPositions[i], 10 * Time.deltaTime);
        }
        if (quadrants[3].localPosition == startPositions[3] && collider.enabled)
        {
            collider.enabled = false;
        }
    }



}
