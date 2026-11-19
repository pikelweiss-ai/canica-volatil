using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothing = 5f;

    private Vector3 offset;
    private Vector3 velocity = Vector3.zero;

    private float shakeDuration = 0f;
    private float shakeMagnitude = 0.15f;

    Camera view;
    float normalSize;
    PlayerAttack attack;

    void Start()
    {
        offset = new Vector3(0, 0, -10);
        view = GetComponent<Camera>();
        if (view != null) normalSize = view.orthographicSize;
    }

    void LateUpdate()
    {
        if(target==null || Time.deltaTime<=0) return;
        if (attack == null || attack.transform != target) attack = target.GetComponent<PlayerAttack>();
        var weapon = attack != null && target.gameObject.activeInHierarchy ? attack.CurrentWeapon : null;
        if (view != null && view.orthographic)
        {
            float size = normalSize;
            if (weapon != null && !weapon.melee && weapon.range >= 9f)
            {
                float range = weapon.range / Mathf.Cos(weapon.spreadAngle * Mathf.Deg2Rad) + 1f;
                size = Mathf.Max(normalSize, range / Mathf.Min(1f, view.aspect));
            }
            view.orthographicSize = Mathf.MoveTowards(view.orthographicSize, size, 18f * Time.deltaTime);
        }
        Vector3 targetCamPos = target.position + offset;

        if (shakeDuration > 0)
        {
            Vector3 shakeOffset = new Vector3(
                Random.Range(-shakeMagnitude, shakeMagnitude),
                Random.Range(-shakeMagnitude, shakeMagnitude),
                0
            );

            targetCamPos += shakeOffset;
            shakeDuration -= Time.deltaTime;
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetCamPos, ref velocity, 0.2f / Mathf.Max(.01f,smoothing),Mathf.Infinity,Time.deltaTime);
    }

    public void ShakeCamera(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
    }
}
