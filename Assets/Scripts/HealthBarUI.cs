using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    private Camera _cam;

    private void Start()
    {
        _cam = Camera.main;

        if (fillImage == null)
        {
            Debug.LogWarning("[HealthBarUI] fillImage ay hindi naka-assign sa Inspector!");
        }
        else if (fillImage.type != Image.Type.Filled)
        {
            Debug.LogWarning("[HealthBarUI] Ang Image Type ay dapat 'Filled' para gumana ang fillAmount. Kasalukuyan: " + fillImage.type);
        }
    }

    public void SetHealth(int current, int max)
    {
        Debug.Log($"[HealthBarUI] SetHealth called: {current}/{max}, fillImage null? {fillImage == null}");

        if (fillImage == null) return;

        float ratio = (float)current / max;
        fillImage.fillAmount = ratio;

        Debug.Log($"[HealthBarUI] fillAmount set to: {ratio}");
    }

    private void LateUpdate()
    {
        if (_cam == null)
        {
            _cam = Camera.main;
            if (_cam == null) return;
        }

        transform.rotation = Quaternion.LookRotation(transform.position - _cam.transform.position);
    }
}