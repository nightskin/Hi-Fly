using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationScript : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Vector3 rotationDirection;
    [SerializeField] float rotationSpeed = 10;

    void Start()
    {
        if(!target) target = transform;
        if (rotationDirection == Vector3.zero) rotationDirection = Util.RandomVector3(-1, 1).normalized;
    }

    
    void Update()
    {
        target.Rotate(rotationDirection * rotationSpeed * Time.deltaTime);
    }
}
