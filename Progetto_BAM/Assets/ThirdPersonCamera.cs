using UnityEngine;

public class ThirdPersonCamera: MonoBehaviour
{
    public Transform player;

    public Vector3 pivotOffset = new Vector3(0f, 1.6f, 0f);

    [Range(1f, 15f)] public float defaultDistance = 4f;
    [Range(0.5f, 5f)] public float minDistance = 1.5f;
    [Range(5f, 20f)] public float maxDistance = 10f;
    public float zoomSpeed = 3f;
    public float zoomDamping = 8f;

    public float sensitivityX = 180f;
    public float sensitivityY = 120f;
    [Range(-80f, 0f)] public float minVerticalAngle = -30f;
    [Range(0f, 80f)] public float maxVerticalAngle = 60f;
    public bool invertY = false;
    [Range(1f, 20f)] public float rotationDamping = 10f;
    [Range(1f, 30f)] public float followSpeed = 12f;

    public LayerMask collisionLayers = ~0;
    public float collisionRadius = 0.3f;

    public float transparencyThreshold = 2f;
    [Range(0f, 1f)] public float minAlpha = 0.15f;

    public Color crosshairColor = new Color(1f, 1f, 1f, 0.9f);
    public int crosshairSize = 10;
    public int crosshairGap = 4;
    public int crosshairThick = 2;
    public bool showDot = true;

    public GameObject _cam;
    private float _yaw;
    private float _pitch;
    private float _targetYaw;
    private float _targetPitch;
    private float _currentDistance;
    private float _targetDistance;
    private Vector3 _currentPivot;
    private Renderer[] _playerRenderers;
    private float[] _originalAlpha;
    private Texture2D _crosshairTex;

    private void Awake()
    {
        _cam = UnityEngine.Camera.main.gameObject;
        _currentDistance = defaultDistance;
        _targetDistance = defaultDistance;

        if (player != null)
        {
            _currentPivot = player.position + pivotOffset;
            _yaw = _targetYaw = player.eulerAngles.y;
        }

        _pitch = _targetPitch = 10f;

        if (player != null)
        {
            _playerRenderers = player.GetComponentsInChildren<Renderer>(true);
            _originalAlpha = new float[_playerRenderers.Length];
            for (int i = 0; i < _playerRenderers.Length; i++)
            {
                var mat = _playerRenderers[i].material;
                _originalAlpha[i] = mat.HasProperty("_Color") ? mat.color.a : 1f;
            }
        }

        _crosshairTex = new Texture2D(1, 1);
        _crosshairTex.SetPixel(0, 0, Color.white);
        _crosshairTex.Apply();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (_crosshairTex != null) Destroy(_crosshairTex);
    }

    private void Update()
    {
        if (player == null || _cam == null) return;
        HandleInput();
    }

    private void LateUpdate()
    {
        if (player == null || _cam == null) return;
        SmoothFollow();
        ApplyRotation();
        ApplyCollision();
        ApplyOcclusion();
    }

    private void OnGUI()
    {
        DrawCrosshair();
    }

    private void HandleInput()
    {
        float inputX = Input.GetAxis("Mouse X");
        float inputY = Input.GetAxis("Mouse Y");

        _targetYaw += inputX * sensitivityX * Time.deltaTime;
        _targetPitch += (invertY ? inputY : -inputY) * sensitivityY * Time.deltaTime;
        _targetPitch = Mathf.Clamp(_targetPitch, minVerticalAngle, maxVerticalAngle);

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        _targetDistance -= scroll * zoomSpeed;
        _targetDistance = Mathf.Clamp(_targetDistance, minDistance, maxDistance);
    }

    private void SmoothFollow()
    {
        Vector3 desiredPivot = player.position + pivotOffset;
        _currentPivot = Vector3.Lerp(_currentPivot, desiredPivot, followSpeed * Time.deltaTime);
        transform.position = _currentPivot;
    }

    private void ApplyRotation()
    {
        _yaw = Mathf.LerpAngle(_yaw, _targetYaw, rotationDamping * Time.deltaTime);
        _pitch = Mathf.LerpAngle(_pitch, _targetPitch, rotationDamping * Time.deltaTime);
        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }

    private void ApplyCollision()
    {
        _currentDistance = Mathf.Lerp(_currentDistance, _targetDistance, zoomDamping * Time.deltaTime);

        Vector3 origin = transform.position;
        Vector3 dir = -transform.forward;
        float safeDistance = _currentDistance;

        if (Physics.SphereCast(origin, collisionRadius, dir, out RaycastHit hit, _currentDistance, collisionLayers, QueryTriggerInteraction.Ignore))
        {
            safeDistance = Mathf.Clamp(hit.distance - collisionRadius * 0.5f, minDistance, _currentDistance);
        }

        _cam.transform.position = origin + dir * safeDistance;
        _cam.transform.LookAt(origin);
    }

    private void ApplyOcclusion()
    {
        if (_playerRenderers == null) return;

        float dist = Vector3.Distance(_cam.transform.position, player.position + pivotOffset);
        float alpha = Mathf.Clamp01((dist - minDistance) / Mathf.Max(transparencyThreshold - minDistance, 0.01f));
        alpha = Mathf.Max(alpha, minAlpha);

        for (int i = 0; i < _playerRenderers.Length; i++)
        {
            var mat = _playerRenderers[i].material;
            if (!mat.HasProperty("_Color")) continue;

            Color c = mat.color;
            c.a = alpha * _originalAlpha[i];
            mat.color = c;

            if (alpha < 1f)
            {
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
            else
            {
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                mat.SetInt("_ZWrite", 1);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.DisableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = -1;
            }
        }
    }

    private void DrawCrosshair()
    {
        int cx = Screen.width / 2;
        int cy = Screen.height / 2;
        GUI.color = crosshairColor;
        GUI.DrawTexture(new Rect(cx - crosshairSize - crosshairGap, cy - crosshairThick / 2, crosshairSize, crosshairThick), _crosshairTex);
        GUI.DrawTexture(new Rect(cx + crosshairGap, cy - crosshairThick / 2, crosshairSize, crosshairThick), _crosshairTex);
        GUI.DrawTexture(new Rect(cx - crosshairThick / 2, cy - crosshairSize - crosshairGap, crosshairThick, crosshairSize), _crosshairTex);
        GUI.DrawTexture(new Rect(cx - crosshairThick / 2, cy + crosshairGap, crosshairThick, crosshairSize), _crosshairTex);
        if (showDot)
            GUI.DrawTexture(new Rect(cx - crosshairThick / 2, cy - crosshairThick / 2, crosshairThick + 1, crosshairThick + 1), _crosshairTex);
        GUI.color = Color.white;
    }

    public Vector3 GetCameraForward()
    {
        Vector3 f = _cam.transform.forward;
        f.y = 0f;
        return f.normalized;
    }

    public Vector3 GetCameraRight()
    {
        Vector3 r = _cam.transform.right;
        r.y = 0f;
        return r.normalized;
    }
}