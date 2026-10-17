using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public PlayerShip ship;
    public float lerpSpeed = 20;

    void Update()
    {
        if(GameManager.Get().state == GameManager.GameState.GAME_PLAY)
        {
            Vector3 targetPosition = ship.transform.position - (transform.forward * 10) + (transform.up * 3);
            transform.position = Vector3.Lerp(transform.position, targetPosition, lerpSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, ship.transform.rotation, lerpSpeed * Time.deltaTime);
        }
    }
}
