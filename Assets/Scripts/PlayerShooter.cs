using Unity.Netcode;
using UnityEngine;

public class PlayerShooter : NetworkBehaviour
{
    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 20f;
    [SerializeField] private float fireCooldown = 0.3f;

    private PlayerHealth _health;
    private PlayerCameraFollow _cameraFollow;

    private float _cooldownTimer;

    private void Awake()
    {
        _health = GetComponent<PlayerHealth>();
        _cameraFollow = GetComponent<PlayerCameraFollow>();
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (_health != null && _health.IsDead.Value) return;

        _cooldownTimer -= Time.deltaTime;

        if (Input.GetButtonDown("Fire1") && _cooldownTimer <= 0f)
        {
            _cooldownTimer = fireCooldown;

            if (_cameraFollow != null)
            {
                _cameraFollow.AddRecoil();
            }

            // Kunin ang direction MULA SA FIREPOINT MISMO ng owning client,
            // sa eksaktong sandali ng pagbaril -- ito ang pinaka-accurate
            // na representasyon ng kanyang tunay na posisyon/direksyon.
            Vector3 fireDirection = firePoint.forward;

            RequestFireServerRpc(firePoint.position, fireDirection);
        }
    }

    [ServerRpc]
    private void RequestFireServerRpc(Vector3 position, Vector3 direction, ServerRpcParams rpcParams = default)
    {
        Quaternion rotation = Quaternion.LookRotation(direction);

        GameObject bulletGo = Instantiate(bulletPrefab, position, rotation);

        NetworkObject bulletNo = bulletGo.GetComponent<NetworkObject>();
        Bullet bullet = bulletGo.GetComponent<Bullet>();

        bullet.Initialize(rpcParams.Receive.SenderClientId, bulletSpeed, direction);

        bulletNo.Spawn(true);
    }
}