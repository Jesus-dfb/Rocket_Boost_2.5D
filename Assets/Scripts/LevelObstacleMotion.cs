using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class LevelObstacleMotion : MonoBehaviour
{
    public enum Motion { Move, Rotate, Scale }
    public Motion motion;
    public Vector3 travel = new Vector3(0, 3, 0);
    public float period = 4f;
    public float rotationSpeed = 35f;
    public float scaleMultiplier = 1.8f;
    private Rigidbody body;
    private Vector3 origin;
    private Quaternion initialRotation;
    private Vector3 initialScale;
    private float elapsed;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        origin = body.position;
        initialRotation = body.rotation;
        initialScale = transform.localScale;
    }

    private void FixedUpdate()
    {
        elapsed += Time.fixedDeltaTime;
        float wave = (1f - Mathf.Cos(elapsed * Mathf.PI * 2f / Mathf.Max(0.1f, period))) * 0.5f;
        switch (motion)
        {
            case Motion.Move: body.MovePosition(origin + travel * wave); break;
            case Motion.Rotate: body.MoveRotation(initialRotation * Quaternion.Euler(0, 0, elapsed * rotationSpeed)); break;
            case Motion.Scale: transform.localScale = initialScale * Mathf.Lerp(1f, scaleMultiplier, wave); break;
        }
    }
}
