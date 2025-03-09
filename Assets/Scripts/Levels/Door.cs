using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] Vector3 leftClosedPosition;
    [SerializeField] Vector3 rightClosedPosition;

    [SerializeField] Vector3 leftOpenPosition;
    [SerializeField] Vector3 rightOpenPosition;

    [SerializeField] float speed = 10;

    [SerializeField] Transform leftSide;
    [SerializeField] Transform rightSide;

    public bool locked = false;
    public bool open = false;

    
    void Update()
    {
        if(!locked)
        {
            if (open)
            {
                leftSide.localPosition = Vector3.Lerp(leftSide.localPosition, leftOpenPosition, speed * Time.deltaTime);
                rightSide.localPosition = Vector3.Lerp(rightSide.localPosition, rightOpenPosition, speed * Time.deltaTime);
            }
            else
            {
                leftSide.localPosition = Vector3.Lerp(leftSide.localPosition, leftClosedPosition, speed * Time.deltaTime);
                rightSide.localPosition = Vector3.Lerp(rightSide.localPosition, rightClosedPosition, speed * Time.deltaTime);
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            open = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            open = false;
        }
    }

}
