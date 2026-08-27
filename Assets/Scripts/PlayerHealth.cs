using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerHealth : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;

    [Header("Respawn")]
    [SerializeField] private Vector3 respawnPosition = Vector3.zero;

    public NetworkVariable<int> Health = new NetworkVariable<int>(
        100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> IsDead = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private Animator _animator;
    private static readonly int IsDeadHash = Animator.StringToHash("IsDead");

    [Header("UI")]
    [SerializeField] private HealthBarUI healthBarUI;

    private bool _respawnRequested;

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        if (healthBarUI == null)
        {
            healthBarUI = GetComponentInChildren<HealthBarUI>(true);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Health.Value = maxHealth;
            IsDead.Value = false;
        }

        Health.OnValueChanged += OnHealthChanged;
        IsDead.OnValueChanged += OnDeathChanged;

        OnHealthChanged(0, Health.Value);
        OnDeathChanged(false, IsDead.Value);
    }

    public override void OnNetworkDespawn()
    {
        Health.OnValueChanged -= OnHealthChanged;
        IsDead.OnValueChanged -= OnDeathChanged;
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (IsDead.Value)
        {
            if (Input.GetKeyDown(KeyCode.R) && !_respawnRequested)
            {
                _respawnRequested = true;
                RequestRespawnServerRpc();
            }
        }
        else
        {
            _respawnRequested = false;
        }
    }

    public void ServerTakeDamage(int amount, ulong attackerClientId)
    {
        Debug.Log($"[ServerTakeDamage] Tinawag. IsServer: {IsServer}, IsDead: {IsDead.Value}, Current Health: {Health.Value}");

        if (!IsServer || IsDead.Value) return;

        Health.Value = Mathf.Max(0, Health.Value - amount);
        Debug.Log($"[ServerTakeDamage] Bagong Health: {Health.Value}");

        if (Health.Value == 0)
        {
            IsDead.Value = true;

            if (NetworkManager.Singleton.ConnectedClients.TryGetValue(attackerClientId, out var client))
            {
                var shooterScore = client.PlayerObject.GetComponent<PlayerScore>();
                if (shooterScore != null)
                {
                    shooterScore.ServerAddScore(10);
                }
            }
        }
    }

    // =========================
    // RESPAWN
    // =========================

    [ServerRpc]
    private void RequestRespawnServerRpc()
    {
        if (!IsDead.Value) return;
        ServerRespawn();
    }

    private void ServerRespawn()
    {
        Debug.Log($"[ServerRespawn] Bago i-reset - IsDead: {IsDead.Value}, Health: {Health.Value}");

        Health.Value = maxHealth;
        IsDead.Value = false;

        Debug.Log($"[ServerRespawn] Pagkatapos i-reset - IsDead: {IsDead.Value}, Health: {Health.Value}");

        ClientRpcParams targetParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { OwnerClientId }
            }
        };

        TeleportToRespawnClientRpc(respawnPosition, targetParams);
    }

    [ClientRpc]
    private void TeleportToRespawnClientRpc(Vector3 position, ClientRpcParams rpcParams = default)
    {
        if (!IsOwner) return;

        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        transform.position = position;

        if (cc != null) cc.enabled = true;

        if (_animator != null)
        {
            // Direktang i-set dito ang IsDead bool (huwag na lang umasa sa
            // NetworkVariable sync timing), para siguradong Idle na ang
            // animator bago i-sample sa Update(0f) sa ibaba.
            _animator.SetBool(IsDeadHash, false);
            _animator.SetFloat("Speed", 0f);
            _animator.Update(0f);
        }

        if (healthBarUI != null)
        {
            healthBarUI.SetHealth(Health.Value, maxHealth);
        }
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        if (healthBarUI != null)
            healthBarUI.SetHealth(newValue, maxHealth);
    }

    private void OnDeathChanged(bool oldValue, bool isDead)
    {
        _animator.SetBool(IsDeadHash, isDead);
    }
}