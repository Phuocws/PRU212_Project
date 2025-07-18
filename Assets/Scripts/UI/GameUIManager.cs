using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI heartText;
    [SerializeField] private TextMeshProUGUI waveText;

    [Header("Enermies Info Panel")]
    [SerializeField] private GameObject enemyInfoPanel;
	[SerializeField] private TextMeshProUGUI nameText;
	[SerializeField] private Image avatarImage;
	[SerializeField] private TextMeshProUGUI heartValue;
	[SerializeField] private TextMeshProUGUI attackValue;
	[SerializeField] private TextMeshProUGUI armorValue;
	[SerializeField] private TextMeshProUGUI skullValue;

	private BaseEnemy selectedEnemy;

	private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject);
    }

	private void Update()
	{
		if (enemyInfoPanel.activeSelf && selectedEnemy != null)
		{
			heartValue.text = $"{Mathf.CeilToInt(selectedEnemy.currentHealth)}/{selectedEnemy.maxHealth}";
		}
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

	public void ShowEnemyInfo(BaseEnemy enemy, Sprite avatar)
	{
		enemyInfoPanel.SetActive(true);
		selectedEnemy = enemy;

		nameText.text = enemy.typeName;
		heartValue.text = $"{enemy.currentHealth}/{enemy.maxHealth}";

		if (enemy.minAttack > 0 && enemy.maxAttack > 0)
			attackValue.text = $"{enemy.minAttack}-{enemy.maxAttack}";
		else
			attackValue.text = "None";

		if (enemy.armor >= 2)
			armorValue.text = $"High";
		else if (enemy.armor >= 1)
			armorValue.text = "Med";
		else
			armorValue.text = "None";

		skullValue.text = enemy.damageAtEndOfPath.ToString();

		if (avatar != null)
			avatarImage.sprite = avatar;
	}

	public void HideEnemyInfo()
	{
		enemyInfoPanel.SetActive(false);
		selectedEnemy = null;
	}

	public bool IsEnemyInfoPanelActive() => enemyInfoPanel.activeSelf;
}
