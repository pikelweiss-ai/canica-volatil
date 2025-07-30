using UnityEngine;

public class PoofEffect : MonoBehaviour
{
    public float destroyTime = 2f;

    void Start()
    {
        Destroy(gameObject, destroyTime);
    }
}