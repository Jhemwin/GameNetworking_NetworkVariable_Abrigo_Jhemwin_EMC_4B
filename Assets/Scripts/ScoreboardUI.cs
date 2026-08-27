using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class ScoreboardUI : MonoBehaviour
{
    [Header("Display Settings")]
    [SerializeField] private int boxWidth = 350;
    [SerializeField] private int rowHeight = 26;
    [SerializeField] private int fontSize = 16;

    private GUIStyle _headerStyle;
    private GUIStyle _rowStyle;
    private bool _stylesReady;

    private void EnsureStyles()
    {
        if (_stylesReady) return;

        _headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = fontSize + 2,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };

        _rowStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = fontSize,
            normal = { textColor = Color.white },
            wordWrap = false,
            clipping = TextClipping.Overflow // hindi na puputulin kahit lumagpas
        };

        _stylesReady = true;
    }

    private void OnGUI()
    {
        if (NetworkManager.Singleton == null) return;
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer) return;

        EnsureStyles();

        List<(string name, int score, int health)> rows = new List<(string, int, int)>();

        foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
        {
            var playerObj = kvp.Value.PlayerObject;
            if (playerObj == null) continue;

            var scoreComp = playerObj.GetComponent<PlayerScore>();
            var healthComp = playerObj.GetComponent<PlayerHealth>();

            int score = scoreComp != null ? scoreComp.Score.Value : 0;
            int health = healthComp != null ? healthComp.Health.Value : -1; // -1 kung walang nahanap, para malaman natin agad kung may problema

            string label = $"Player {kvp.Key}";
            rows.Add((label, score, health));
        }

        rows.Sort((a, b) => b.score.CompareTo(a.score));

        int totalHeight = 30 + (rows.Count * rowHeight) + 10;

        GUI.Box(new Rect(10, 10, boxWidth, totalHeight), "");
        GUI.Label(new Rect(20, 15, boxWidth - 20, 24), "SCOREBOARD", _headerStyle);

        int y = 40;
        foreach (var row in rows)
        {
            string line = $"{row.name}  Score:{row.score}  HP:{row.health}";
            GUI.Label(new Rect(20, y, boxWidth - 20, rowHeight), line, _rowStyle);
            y += rowHeight;
        }
    }
}