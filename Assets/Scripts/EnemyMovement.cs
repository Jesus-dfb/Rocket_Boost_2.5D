using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Floating Movement")]
    [Tooltip("Distance from the starting position to the other end. X: horizontal, Y: vertical.")]
    [SerializeField] private Vector3 movement = new Vector3(0f, 3f, 0f);
    [Tooltip("Seconds to travel from one end to the other.")]
    [SerializeField, Min(0.01f)] private float duration = 2f;

    private Rigidbody body;
    private Vector3 startPosition;
    private float elapsedTime;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        body.isKinematic = true;
        startPosition = body.position;
    }

    private void OnEnable()
    {
        elapsedTime = 0f;
    }

    private void FixedUpdate()
    {
        elapsedTime += Time.fixedDeltaTime;
        float progress = (1f - Mathf.Cos(elapsedTime * Mathf.PI / Mathf.Max(0.01f, duration))) * 0.5f;
        body.MovePosition(startPosition + movement * progress);
    }
}
