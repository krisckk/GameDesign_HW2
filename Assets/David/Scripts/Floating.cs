
using UnityEngine;

public class Floating : MonoBehaviour
{
    private float floatHeight = 0.25f;
    private float floatSpeed = 2f;

    private float rotationSpeed = 60f;

    private Vector3 initialPosition;

    private void Start()
    {
        initialPosition = transform.localPosition;
    }

    private void Update()
    {
        float offset = Mathf.Sin(Time.time * floatSpeed)
                       * floatHeight;

        transform.localPosition =
            initialPosition + Vector3.up * offset;
            
        transform.Rotate(
            Vector3.up,
            rotationSpeed * Time.deltaTime,
            Space.Self
        );
    }
}
