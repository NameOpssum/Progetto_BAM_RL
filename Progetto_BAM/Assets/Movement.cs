using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform player;
    public Vector3 pivotOffset = new Vector3(0f, 1.6f, 0f);

    public float distance = 4f;
    public float sensitivityX = 180f;
    public float sensitivityY = 120f;
    public float minPitch = -30f;
    public float maxPitch = 60f;
    public float smooth = 10f;

    private float _yaw;
    private float _pitch;

    void Awake()
    {
        _yaw = player.eulerAngles.y;
        _pitch = 10f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (player == null) return;

        HandleRotation();
        UpdateCameraPosition();
    }

    void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        _yaw += mouseX * sensitivityX * Time.deltaTime;
        _pitch -= mouseY * sensitivityY * Time.deltaTime;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
    }

    void UpdateCameraPosition()
    {
        Vector3 pivot = player.position + pivotOffset;

        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);

        // CAMERA ORBITALE
        Vector3 desiredPosition = pivot + rotation * new Vector3(0f, 0f, -distance);

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smooth * Time.deltaTime);
        transform.LookAt(pivot);
    }

    public Vector3 GetCameraForward()
    {
        Vector3 f = transform.forward;
        f.y = 0f;
        return f.normalized;
    }

    public Vector3 GetCameraRight()
    {
        Vector3 r = transform.right;
        r.y = 0f;
        return r.normalized;
    }
}using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform player;
    public Vector3 pivotOffset = new Vector3(0f, 1.6f, 0f);

    public float distance = 4f;
    public float sensitivityX = 180f;
    public float sensitivityY = 120f;
    public float minPitch = -30f;
    public float maxPitch = 60f;
    public float smooth = 10f;

    private float _yaw;
    private float _pitch;

    void Awake()
    {
        _yaw = player.eulerAngles.y;
        _pitch = 10f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (player == null) return;

        HandleRotation();
        UpdateCameraPosition();
    }

    void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        _yaw += mouseX * sensitivityX * Time.deltaTime;
        _pitch -= mouseY * sensitivityY * Time.deltaTime;
        _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);
    }

    void UpdateCameraPosition()
    {
        Vector3 pivot = player.position + pivotOffset;

        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);

        // CAMERA ORBITALE
        Vector3 desiredPosition = pivot + rotation * new Vector3(0f, 0f, -distance);

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smooth * Time.deltaTime);
        transform.LookAt(pivot);
    }

    public Vector3 GetCameraForward()
    {
        Vector3 f = transform.forward;
        f.y = 0f;
        return f.normalized;
    }

    public Vector3 GetCameraRight()
    {
        Vector3 r = transform.right;
        r.y = 0f;
        return r.normalized;
    }
}