using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour
{
    public float rotateSpeed = 3f;

    // Random rotator
    void Update()
    {
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
    }
}
