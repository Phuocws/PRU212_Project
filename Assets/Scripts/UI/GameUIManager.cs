using TMPro;
using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI heartText;
    [SerializeField] private TextMeshProUGUI waveText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject);
    }

    public void SetHearts(int value) => heartText.text = value.ToString();
    public void SetCoins(int amount) => coinText.text = amount.ToString();
    public void SetWaves(int current, int total) => waveText.text = $"Wave: {current}/{total}";

    public Vector2 WorldToUIPosition(Vector3 worldPosition)
    {
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(worldPosition);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mainCanvas.transform as RectTransform,
            screenPoint,
            mainCanvas.worldCamera,
            out Vector2 localPoint);
        return localPoint;
    }
}
