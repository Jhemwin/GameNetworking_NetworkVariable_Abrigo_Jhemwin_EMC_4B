using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class Bullet : NetworkBehaviour
{
    [SerializeField] private int damage = 20;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private LayerMask hitMask = ~0; // "Everything" by default

    private ulong _ownerClientId;
    private float _speed;
    private float _timer;
    private bool _hasDespawned;
    private Vector3 _direction;
    private Vector3 _previousPosition;

    public void Initialize(ulong ownerClientId, float speed, Vector3 direction)
    {
        _ownerClientId = ownerClientId;
        _speed = speed;
        _direction = direction.normalized;
        _previousPosition = transform.position;
    }

    private void Update()
    {
        if (!IsServer || _hasDespawned) return;

        _previousPosition = transform.position;
        Vector3 nextPosition = transform.position + _direction * _speed * Time.deltaTime;

        // RAYCAST sa pagitan ng dating posisyon at susunod na posisyon --
        // hindi ito "tumatagos"/"tunneling" kahit gaano kabilis ang bala,
        // dahil chine-check natin ang BUONG DAAN, hindi lang yung bagong point.
        float distanceThisFrame = Vector3.Distance(_previousPosition, nextPosition);

        if (Physics.Raycast(_previousPosition, _direction, out RaycastHit hit, distanceThisFrame, hitMask, QueryTriggerInteraction.Ignore))
        {
            HandleHit(hit.collider);
            if (_hasDespawned) return; // huminto na kung na-despawn na dahil sa hit
        }

        transform.position = nextPosition;

        _timer += Time.deltaTime;
        if (_timer >= lifeTime)
        {
            DespawnBullet();
        }
    }

    private void HandleHit(Collider other)
    {
        NetworkObject hitNo = other.GetComponentInParent<NetworkObject>();
        if (hitNo == null) return;
        if (hitNo.OwnerClientId == _ownerClientId) return; // huwag tamaan ang sariling shooter

        PlayerHealth targetHealth = other.GetComponentInParent<PlayerHealth>();
        if (targetHealth != null)
        {
            targetHealth.ServerTakeDamage(damage, _ownerClientId);
            DespawnBullet();
        }
    }

    // Panatilihin din natin ang OnTriggerEnter bilang BACKUP/fallback,
    // sakaling may mga sitwasyon na hindi na-catch ng raycast (hal. spawn-inside-collider).
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || _hasDespawned) return;
        HandleHit(other);
    }

    private void DespawnBullet()
    {
        if (_hasDespawned) return;
        _hasDespawned = true;

        NetworkObject no = GetComponent<NetworkObject>();
        if (no != null && no.IsSpawned)
        {
            no.Despawn(true);
        }
    }
}