using UnityEngine;

public class SpinParent : MonoBehaviour
{
    [SerializeField] private float spinSpeed;
    [SerializeField] private Vector3 direction;
    void Update()
    {
        transform.parent.Rotate(direction, spinSpeed * Time.deltaTime);
    }
}
