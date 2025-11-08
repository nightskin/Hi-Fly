using UnityEngine;

public class DrillDasher : MonoBehaviour
{
    [SerializeField] BoxCollider collider;
    [SerializeField] Transform[] quadrants;
    [SerializeField] float maxTurn = 35;
    
    float rotationSpd = 45;
    Vector3[] startPositions;
    float zRot = 0;
    float t = 0;

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
        zRot += rotationSpd * Time.deltaTime;
        Vector2 steerInput = InputManager.player.Steer.ReadValue<Vector2>() * maxTurn;
        transform.localEulerAngles = new Vector3(steerInput.y, steerInput.x, zRot);

        if(InputManager.player.Boost.WasPressedThisFrame() || InputManager.player.Boost.WasReleasedThisFrame())
        {
            t = 0;
        }

        if(InputManager.player.Boost.IsPressed())
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
        t += Time.deltaTime;
        t = Mathf.Clamp01(t);
        for (int i = 0; i < quadrants.Length; i++)
        {
            quadrants[i].transform.localPosition = Vector3.Lerp(quadrants[i].localPosition, Vector3.zero, t);
        }
        if (Physics.BoxCast(transform.position, collider.size, transform.forward, out RaycastHit hit, Quaternion.identity, collider.size.z))
        {
            if (hit.transform.tag == "Destructible")
            {
                Asteroid asteroid = hit.transform.GetComponent<Asteroid>();
                if (asteroid)
                {
                    asteroid.RemoveBlock(hit);
                }
            }
        }
    }

    void StopDrilling()
    {
        rotationSpd = 45;
        t += Time.deltaTime;
        t = Mathf.Clamp01(t);
        for (int i = 0; i < quadrants.Length; i++)
        {
            quadrants[i].transform.localPosition = Vector3.Lerp(quadrants[i].localPosition, startPositions[i], t);
        }
    }
    
}
