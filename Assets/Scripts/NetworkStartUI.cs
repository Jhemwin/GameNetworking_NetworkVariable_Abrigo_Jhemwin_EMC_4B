using UnityEngine;
using Unity.Netcode;

public class NetworkStartUI : MonoBehaviour
{
    private void OnGUI()
    {
        if (NetworkManager.Singleton == null)
            return;

        if (!NetworkManager.Singleton.IsClient &&
            !NetworkManager.Singleton.IsServer)
        {
            float w = 200f;
            float h = 40f;
            float spacing = 10f;

            // Total height ng 3 buttons
            float totalHeight = (h * 3) + (spacing * 2);

            // Center ng screen
            float x = (Screen.width - w) / 2f;
            float y = (Screen.height - totalHeight) / 2f;

            if (GUI.Button(
                new Rect(x, y, w, h),
                "Host"))
            {
                NetworkManager.Singleton.StartHost();
            }

            if (GUI.Button(
                new Rect(x, y + h + spacing, w, h),
                "Client"))
            {
                NetworkManager.Singleton.StartClient();
            }

            if (GUI.Button(
                new Rect(x, y + (h + spacing) * 2f, w, h),
                "Server"))
            {
                NetworkManager.Singleton.StartServer();
            }
        }
    }
}