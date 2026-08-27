using Unity.Netcode;

public class PlayerScore : NetworkBehaviour
{
    public NetworkVariable<int> Score = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        if (IsServer) Score.Value = 0;
    }

    public void ServerAddScore(int amount)
    {
        if (!IsServer) return;
        Score.Value += amount;
    }
}