using UnityEngine;
using System.Collections;

[System.Serializable]
public class Boundary
{
    public float xMin, xMax, yMin, yMax;
}

public class CameraController : MonoBehaviour
{
    public Boundary boundary;
    float cameraSpeed = 10f;

    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        if (Camera.current != null)
        {
            transform.Translate(new Vector3(moveHorizontal * cameraSpeed, moveVertical * cameraSpeed));
            transform.position = new Vector3
                (
                Mathf.Clamp(transform.position.x, boundary.xMin, boundary.xMax),
                Mathf.Clamp(transform.position.y, boundary.yMin, boundary.yMax),
                transform.position.z);
        }
    }
}
