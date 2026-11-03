using UnityEngine;

public class LevelCamera : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float minX = 14f;
    [SerializeField] private float maxX = 53f;
    [SerializeField] private float followSpeed = 4f;
    [Header("2.5D Perspective")]
    [SerializeField] private Vector3 cameraOffset = new Vector3(-6f, 6f, -27f);
    [SerializeField] private float focusHeight = 7f;
    [SerializeField, Range(20f, 75f)] private float fieldOfView = 43f;

    public void SetTarget(Transform player) => target = player;

    private Vector3 FocusPoint => new Vector3(Mathf.Clamp(target.position.x + 5f, minX, maxX), focusHeight, 0f);

    private void Start()
    {
        SnapToTarget();
    }

    public void SnapToTarget()
    {
        Camera view = GetComponent<Camera>();
        if (view != null)
        {
            view.orthographic = false;
            view.fieldOfView = fieldOfView;
        }
        if (target == null) return;
        transform.position = FocusPoint + cameraOffset;
        transform.rotation = Quaternion.LookRotation(-cameraOffset, Vector3.up);
    }

    private void LateUpdate()
    {
        if (target == null) return;
        Vector3 desired = FocusPoint + cameraOffset;
        transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-followSpeed * Time.deltaTime));
        transform.rotation = Quaternion.LookRotation(-cameraOffset, Vector3.up);
    }
}
