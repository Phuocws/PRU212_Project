using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	public static UIManager Instance { get; private set; }
	[SerializeField] private Canvas mainCanvas;

	[SerializeField] private TextMeshProUGUI coinText;
	[SerializeField] private TextMeshProUGUI heartText;
	[SerializeField] private TextMeshProUGUI waveText;

	[SerializeField] private GameObject startWaveButton;
	[SerializeField] private Image fillImage;

	private float countdownTime;
	private float timer;
	private bool isCounting;
	private bool autoTriggerNextWave;

	[Header("Wave Detail Panel")]
	[SerializeField] private GameObject waveDetailPanel;
	[SerializeField] private TextMeshProUGUI waveTitleText;
	[SerializeField] private TextMeshProUGUI waveDetailsText;
	[SerializeField] private TextMeshProUGUI waveInstructionText;
	private bool isWaveDetailShown = false;

	[Header("Instructions")]
	[SerializeField] private GameObject startBattlePanel;
	[SerializeField] private GameObject buildTowerPanel;

	[Header("Tower Build Panel")]
	[SerializeField] private GameObject selectTowerPanel;
	[SerializeField] private Button archerTowerButton;
	[SerializeField] private GameObject buildCheckedIcon;
	[SerializeField] private GameObject towerIcon;
	[SerializeField] private TextMeshProUGUI priceText;
	private bool isTowerPanelShown = false;

	[Header("Tower Upgrade Panel")]
	[SerializeField] private GameObject selectedTowerPanel;
	[SerializeField] private Button upgradeButton;
	[SerializeField] private Button sellButton;
	[SerializeField] private GameObject sellCheckedIcon;
	[SerializeField] private GameObject sellIcon;
	[SerializeField] private TextMeshProUGUI upgradePriceText;
	[SerializeField] private GameObject upgradeIcon; 
	[SerializeField] private GameObject upgradeCheckedIcon;

	private bool towerPanelJustOpened = false;
	private float panelOpenGraceTime = 0.1f;
	private float panelOpenTimer = 0f;

	private Tower selectedTower;
	private string selectedBuildType = null;
	private bool upgradeConfirmed = false;
	private bool sellConfirmed = false;

	[Header("Details Panel")]
	[SerializeField] private GameObject detailsPanel;
	[SerializeField] private TextMeshProUGUI towerTitle;
	[SerializeField] private TextMeshProUGUI towerDescription;
	[SerializeField] private GameObject towerParameters;
	[SerializeField] private TextMeshProUGUI TowerDamage; 

	private bool firstTowerBuilt = false; // Track if first tower is built

	private void Awake()
	{
		if (Instance == null) Instance = this;
		else { Destroy(gameObject); return; }

		DontDestroyOnLoad(gameObject);
	}

	private void Start()
	{
		archerTowerButton.onClick.AddListener(OnArcherTowerButtonClicked);
		upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
		sellButton.onClick.AddListener(OnSellButtonClicked); 
	}

	private void Update()
	{
		if (towerPanelJustOpened)
		{
			panelOpenTimer -= Time.deltaTime;
			if (panelOpenTimer <= 0f)
				towerPanelJustOpened = false;
		}

		// Run countdown logic only if counting
		if (isCounting)
		{
			timer += Time.deltaTime;
			fillImage.fillAmount = Mathf.Clamp01(timer / countdownTime);

			if (timer >= countdownTime)
			{
				isCounting = false;
				fillImage.fillAmount = 1f;

				if (autoTriggerNextWave)
				{
					HideWaveDetail();
					startWaveButton.SetActive(false);
					EnemySpawner.Instance.OnStartWaveClicked(); // auto call
				}
			}
		}

		// Always check for outside click to close panels
		CheckClickOutsideUI();
	}

	// ================================
	// UI Setters
	// ================================

	public void SetHearts(int value) => heartText.text = value.ToString();
	public void SetCoins(int amount) => coinText.text = amount.ToString();
	public void SetWaves(int current, int total) => waveText.text = $"Wave: {current}/{total}";

	// ================================
	// Position UI
	// ================================

	public Vector2 WorldToUIPosition(Vector3 worldPosition)
	{
		Vector2 screenPoint = Camera.main.WorldToScreenPoint(worldPosition);

		// If Canvas is in Screen Space - Camera or World Space:
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			mainCanvas.transform as RectTransform,
			screenPoint,
			mainCanvas.worldCamera,
			out Vector2 localPoint);
		return localPoint;
	}

	// ================================
	// Handle Click Logic
	// ================================

	public void OnStartWaveButtonClicked()
	{
		if (!isWaveDetailShown)
		{
			ShowWaveDetail();
			// Only hide if something was actually open
			if (isTowerPanelShown || selectedTowerPanel.activeSelf || detailsPanel.activeSelf)
				HideAllTowerPanels();
		}
		else
		{
			HideWaveDetail();
			startBattlePanel.SetActive(false);
			EnemySpawner.Instance.OnStartWaveClicked();
		}
	}

	private void OnArcherTowerButtonClicked()
	{
		if (selectedTower == null) return;

		if (selectedTower.IsBuilt)
		{
			// ======= Upgrade Logic =======
			if (!selectedTower.CanUpgrade)
			{
				Debug.LogWarning("[UIManager] Tower already at max level.");
				return;
			}

			var nextLevel = selectedTower.GetNextLevelData();

			if (!GameManager.Instance.SpendCoins(nextLevel.cost))
			{
				Debug.LogWarning("[UIManager] Not enough coins to upgrade Tower.");
				return;
			}

			selectedTower.Upgrade();
			HideAllTowerPanels();
			return;
		}

		// ======= Build Logic =======
		if (selectedBuildType != "Archer")
		{
			// First click = confirm intent
			selectedBuildType = "Archer";
			buildCheckedIcon.SetActive(true);
			towerIcon.SetActive(false);
			
			selectedTower.ShowUpgradeRangeOnly();
			ShowTowerDetails(
				selectedTower.PreviewLevelData.displayName,
				selectedTower.PreviewLevelData.description,
				selectedTower.PreviewLevelData.arrowTier.damage
			);

			return;
		}

		// Second click = build
		if (!GameManager.Instance.SpendCoins(selectedTower.PreviewLevelData.cost))
		{
			Debug.LogWarning("[UIManager] Not enough coins to build Archer Tower.");
			return;
		}

		selectedTower.Upgrade(); // Build is just first level upgrade
		firstTowerBuilt = true; // Mark first tower built

		buildTowerPanel.SetActive(false);
		selectedTower.HideUpgradeRange(); // Hide upgrade range
		HideAllTowerPanels();

		// Reset confirmation
		selectedBuildType = null;
	}

	private void OnUpgradeButtonClicked()
	{
		if (selectedTower == null || !selectedTower.IsBuilt)
			return;

		if (!selectedTower.CanUpgrade)
		{
			Debug.LogWarning("[UIManager] Tower already at max level.");
			return;
		}

		var nextLevel = selectedTower.GetNextLevelData();

		if (!upgradeConfirmed)
		{
			// First click: Show check icon
			upgradeConfirmed = true;
			upgradeCheckedIcon.SetActive(true);
			upgradeIcon.SetActive(false);

			ShowTowerDetails(
				nextLevel.displayName,
				nextLevel.description,
				nextLevel.arrowTier.damage
			);

			selectedTower.ShowBothRange();

			if (sellConfirmed)
			{
				// Reset sell confirmation if upgrading
				sellConfirmed = false;
				sellCheckedIcon.SetActive(false);
				sellIcon.SetActive(true);
			}

			return;
		}

		// Second click: Apply upgrade
		if (!GameManager.Instance.SpendCoins(nextLevel.cost))
		{
			Debug.LogWarning("[UIManager] Not enough coins to upgrade Tower.");
			return;
		}

		selectedTower.Upgrade();
		HideAllTowerPanels();

		// Reset
		upgradeConfirmed = false;
		upgradeCheckedIcon.SetActive(false);
		upgradeIcon.SetActive(true);
	}

	private void OnSellButtonClicked()
	{
		if (selectedTower == null || !selectedTower.IsBuilt)
			return;

		if (!sellConfirmed)
		{
			// First click: Show check icon
			sellConfirmed = true;
			sellCheckedIcon.SetActive(true);
			sellIcon.SetActive(false);

			// === Sell Refund UI ===
			int refund = selectedTower.GetSellRefundAmount();
			
			ShowTowerDetails(
				"Sell Tower",
				$"Sell this tower and get for <color=yellow>{refund} coins</color>",
				null
			);
			
			if (upgradeConfirmed)
			{
				// Reset upgrade confirmation if selling
				upgradeConfirmed = false;
				upgradeCheckedIcon.SetActive(false);
				upgradeIcon.SetActive(true);
			}

			return;
		}

		// Second click: Confirm sell
		selectedTower.Sell();
		HideAllTowerPanels();

		// Reset
		sellConfirmed = false;
		sellCheckedIcon.SetActive(false);
		sellIcon.SetActive(true);
	}

	private void CheckClickOutsideUI()
	{
		if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
		{
			if (!EventSystem.current.IsPointerOverGameObject())
			{
				if (isWaveDetailShown)
					HideWaveDetail();

				bool shouldHide = !towerPanelJustOpened &&
					(isTowerPanelShown || selectedTowerPanel.activeSelf || detailsPanel.activeSelf);

				if (shouldHide)
				{
					// This will reset all towers, hide ranges, reset confirmations, etc.
					HideAllTowerPanels();
				}
			}
		}
	}

	// ================================
	// Show/Hide Panels Logic
	// ================================

	public void ShowWaveDetail()
	{
		isWaveDetailShown = true;
		waveDetailPanel.SetActive(true);

		waveTitleText.text = "INCOMING WAVE";
		waveInstructionText.text = "TAP AGAIN TO CALL IT EARLY";

		var wave = EnemySpawner.Instance.GetCurrentWave();
		waveDetailsText.text = wave != null ? wave.GetWaveSummary() : "???";
	}

	public void HideWaveDetail()
	{
		isWaveDetailShown = false;
		waveDetailPanel.SetActive(false);
	}

	public void ShowTowerBuildPanel(bool isChecked, Tower tower)
	{
		Vector2 uiPos = WorldToUIPosition(tower.transform.position); 
		RectTransform panelRect = selectTowerPanel.GetComponent<RectTransform>();
		panelRect.anchoredPosition = uiPos; // Use anchoredPosition instead of position

		HideAllTowerPanels();

		selectedTower = tower;
		selectedBuildType = null;

		isTowerPanelShown = true;
		selectTowerPanel.SetActive(true);
		priceText.text = $"{selectedTower.PreviewLevelData.cost}";

		buildCheckedIcon.SetActive(isChecked);
		towerIcon.SetActive(!isChecked);

		buildTowerPanel.SetActive(false);

		MarkTowerPanelJustOpened();
	}

	public void HideTowerBuildPanel()
	{
		isTowerPanelShown = false;
		selectTowerPanel.SetActive(false);

		if (!firstTowerBuilt)
		{
			buildTowerPanel.SetActive(true);
		}
	}

	public void ShowSelectedTowerPanel(Tower tower)
	{
		Vector2 uiPos = WorldToUIPosition(tower.transform.position);
		RectTransform panelRect = selectedTowerPanel.GetComponent<RectTransform>();
		panelRect.anchoredPosition = uiPos; // Use anchoredPosition instead of position

		HideAllTowerPanels();
		selectedTowerPanel.SetActive(true);

		selectedTower = tower;
		selectedTower.ShowRange(); // Show range indicator

		// Upgrade setup
		upgradeConfirmed = false;
		upgradeCheckedIcon.SetActive(false);
		upgradeIcon.SetActive(true);

		bool canUpgrade = tower.CanUpgrade;
		upgradeButton.interactable = canUpgrade;

		if (canUpgrade)
		{
			var nextLevel = tower.GetNextLevelData();
			upgradePriceText.text = $"{nextLevel.cost}";
		}
		else
		{
			upgradePriceText.text = "MAX";
		}

		MarkTowerPanelJustOpened();
	}

	public void HideAllTowerPanels()
	{
		HideTowerBuildPanel();
		selectedTowerPanel.SetActive(false);

		selectedTower?.HideBothRange();
		selectedTower = null;

		selectedBuildType = null;

		// Reset upgrade UI state
		upgradeConfirmed = false;
		upgradeCheckedIcon.SetActive(false);
		upgradeIcon.SetActive(true);

		// Reset sell UI state
		sellConfirmed = false;
		sellCheckedIcon.SetActive(false);
		sellIcon.SetActive(true);

		// Hide details panel as well
		HideTowerDetails();
	}
	
	private void ShowTowerDetails(string title, string description, int? damage)
	{
		Vector2 uiPos = WorldToUIPosition(selectedTower.transform.position + new Vector3(12, 0f, 0)); // Optional offset
		RectTransform detailsRect = detailsPanel.GetComponent<RectTransform>();
		detailsRect.anchoredPosition = uiPos;

		detailsPanel.SetActive(true);

		towerTitle.text = title ?? "Unknown Tower";
		towerDescription.text = description ?? "No description available.";

		if (damage.HasValue)
		{
			towerParameters.SetActive(true);
			TowerDamage.text = $"{damage.Value}";
		}
		else
			towerParameters.SetActive(false);
	}

	private void HideTowerDetails()
	{
		detailsPanel.SetActive(false);
	}

	private void MarkTowerPanelJustOpened()
	{
		towerPanelJustOpened = true;
		panelOpenTimer = panelOpenGraceTime;
	}

	// ================================
	// Countdown Logic
	// ================================

	public void StartFirstWaveButton()
	{
		isCounting = false;
		fillImage.fillAmount = 1f;
		startWaveButton.SetActive(true);
		startBattlePanel.SetActive(true);
		buildTowerPanel.SetActive(true);
	}

	public void StartCountdown(float duration, bool autoStart)
	{
		countdownTime = duration;
		timer = 0f;
		isCounting = true;
		autoTriggerNextWave = autoStart;

		fillImage.fillAmount = 0f;
		startWaveButton.SetActive(true);
	}

	public void ForceStopCountdown()
	{
		isCounting = false;
		timer = 0f;
		fillImage.fillAmount = 0f;
		startWaveButton.SetActive(false);
		HideWaveDetail();
	}

}
