using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothing = 5f;

    private Vector3 offset;
    private Vector3 velocity = Vector3.zero;

    private float shakeDuration = 0f;
    private float shakeMagnitude = 0.15f;

    void Start()
    {
        offset = new Vector3(0, 0, -10);
    }

    void FixedUpdate()
    {
        Vector3 targetCamPos = target.position + offset;

        if (shakeDuration > 0)
        {
            Vector3 shakeOffset = new Vector3(
                Random.Range(-shakeMagnitude, shakeMagnitude),
                Random.Range(-shakeMagnitude, shakeMagnitude),
                0
            );

            targetCamPos += shakeOffset;
            shakeDuration -= Time.fixedDeltaTime;
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetCamPos, ref velocity, 0.2f / smoothing);
    }

    public void ShakeCamera(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
    }
}