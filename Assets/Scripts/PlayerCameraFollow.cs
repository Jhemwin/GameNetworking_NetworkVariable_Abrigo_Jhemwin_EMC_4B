using Unity.Netcode;
using UnityEngine;

public class PlayerCameraFollow : NetworkBehaviour
{
    [Header("Camera Position")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 5f, -6f);
    [SerializeField] private float smoothSpeed = 10f;

    [Header("Camera Look")]
    [SerializeField] private float lookHeight = 1f;
    [SerializeField] private float lookSmoothSpeed = 12f;

    [Header("Recoil")]
    [SerializeField] private float recoilUp = 2.0f;
    [SerializeField] private float recoilSide = 0.35f;
    [SerializeField] private float recoilReturnSpeed = 8f;
    [SerializeField] private float recoilSnappiness = 15f;

    private Transform _cam;

    private float _currentRecoilX;
    private float _currentRecoilY;

    private float _targetRecoilX;
    private float _targetRecoilY;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        _cam = Camera.main != null
            ? Camera.main.transform
            : null;
    }

    private void LateUpdate()
    {
        if (!IsOwner || _cam == null)
            return;

        // =========================
        // CAMERA POSITION
        // =========================

        Vector3 desiredPos = transform.position + offset;

        _cam.position = Vector3.Lerp(
            _cam.position,
            desiredPos,
            smoothSpeed * Time.deltaTime
        );

        // =========================
        // RECOIL RECOVERY
        // =========================

        _targetRecoilX = Mathf.Lerp(
            _targetRecoilX,
            0f,
            recoilReturnSpeed * Time.deltaTime
        );

        _targetRecoilY = Mathf.Lerp(
            _targetRecoilY,
            0f,
            recoilReturnSpeed * Time.deltaTime
        );

        // Smooth recoil movement
        _currentRecoilX = Mathf.Lerp(
            _currentRecoilX,
            _targetRecoilX,
            recoilSnappiness * Time.deltaTime
        );

        _currentRecoilY = Mathf.Lerp(
            _currentRecoilY,
            _targetRecoilY,
            recoilSnappiness * Time.deltaTime
        );

        // =========================
        // CAMERA LOOK
        // =========================

        Vector3 lookTarget =
            transform.position +
            Vector3.up * lookHeight;

        Vector3 direction =
            lookTarget - _cam.position;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        // Apply recoil
        Quaternion recoilRotation =
            Quaternion.Euler(
                -_currentRecoilX,
                _currentRecoilY,
                0f
            );

        Quaternion finalRotation =
            targetRotation * recoilRotation;

        _cam.rotation = Quaternion.Slerp(
            _cam.rotation,
            finalRotation,
            lookSmoothSpeed * Time.deltaTime
        );
    }

    // =========================
    // ADD RECOIL
    // =========================

    public void AddRecoil()
    {
        _targetRecoilX += recoilUp;

        _targetRecoilY +=
            Random.Range(
                -recoilSide,
                recoilSide
            );
    }
}