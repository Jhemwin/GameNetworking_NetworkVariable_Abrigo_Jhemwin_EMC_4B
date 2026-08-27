using Unity.Netcode;
using UnityEngine;

public class NetworkLauncher : MonoBehaviour
{
    public void OnHostClicked() => NetworkManager.Singleton.StartHost();
    public void OnClientClicked() => NetworkManager.Singleton.StartClient();
}