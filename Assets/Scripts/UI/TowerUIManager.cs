using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System.Collections;

public class TowerUIManager : MonoBehaviour
{
    public static TowerUIManager Instance { get; private set; }

    [Header("Tower Build Panel")]
    [SerializeField] private GameObject selectTowerPanel;
    [SerializeField] private Button archerTowerButton;
    [SerializeField] private GameObject buildCheckedIcon;
    [SerializeField] private GameObject buildUncheckedIcon;
    [SerializeField] private GameObject towerIcon;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private GameObject buildTowerPanel;

    [Header("Tower Upgrade Panel")]
    [SerializeField] private GameObject selectedTowerPanel;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button sellButton;
    [SerializeField] private GameObject sellCheckedIcon;
    [SerializeField] private GameObject sellIcon;
    [SerializeField] private TextMeshProUGUI upgradePriceText;
    [SerializeField] private GameObject upgradeIcon;
    [SerializeField] private GameObject upgradeCheckedIcon;
    [SerializeField] private GameObject upgradeUncheckedIcon;

    [Header("Details Panel")]
    [SerializeField] private GameObject detailsPanel;
    [SerializeField] private TextMeshProUGUI towerTitle;
    [SerializeField] private TextMeshProUGUI towerDescription;
    [SerializeField] private GameObject towerParameters;
    [SerializeField] private TextMeshProUGUI TowerDamage;

	[Header("Tower Info Panel")]
	[SerializeField] private GameObject towerInformationPanel;
	[SerializeField] private Image avatarBackground;
	[SerializeField] private Image avatarImage;
	[SerializeField] private Image avatarBorder;
	[SerializeField] private TextMeshProUGUI nameText;
	[SerializeField] private TextMeshProUGUI attackValueText;
	[SerializeField] private TextMeshProUGUI accuracyValueText;
	[SerializeField] private TextMeshProUGUI fireRateValueText;

    [Header("Build Progress Bar")]
	[SerializeField] private GameObject buildProgressPanel;
	[SerializeField] private Image progressValueImage;
	private Coroutine upgradeCoroutine;

	private bool isProgressActive = false;
	private bool towerPanelJustOpened = false;
    private float panelOpenGraceTime = 0.1f;
    private float panelOpenTimer = 0f;
    private Tower selectedTower;
    private string selectedBuildType = null;
    private bool upgradeConfirmed = false;
    private bool sellConfirmed = false;
    private bool firstTowerBuilt = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        HideAllTowerPanels();
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

        // Auto-switch unchecked to checked icon if coins become enough for upgrade
        if (selectedTowerPanel.activeSelf && selectedTower != null && upgradeUncheckedIcon.activeSelf && upgradeConfirmed)
        {
            var nextLevel = selectedTower.GetNextLevelData();
            if (nextLevel != null && GameManager.Instance.CurrentCoints >= nextLevel.cost)
            {
                upgradeCheckedIcon.SetActive(true);
                upgradeUncheckedIcon.SetActive(false);
            }
        }

        // Existing build icon logic
        if (selectTowerPanel.activeSelf && selectedTower != null && buildUncheckedIcon.activeSelf)
        {
            int cost = selectedTower.PreviewLevelData.cost;
            if (GameManager.Instance.CurrentCoints >= cost)
            {
                buildCheckedIcon.SetActive(true);
                buildUncheckedIcon.SetActive(false);
            }
        }

        CheckClickOutsideUI();
    }

    public void CheckClickOutsideUI()
    {
		if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                bool anyPanelOpen = selectTowerPanel.activeSelf || selectedTowerPanel.activeSelf || detailsPanel.activeSelf;
                if (!towerPanelJustOpened && anyPanelOpen)
                {
                    HideAllTowerPanels();

                    // Hide wave panels if open
                    if (WaveUIManager.Instance != null)
                        WaveUIManager.Instance.HideWaveDetail();
                }
            }
        }
    }

    public void ShowTowerBuildPanel(bool isChecked, Tower tower)
    {
        if (isProgressActive) return; // Block selection during progress

        Vector2 uiPos = GameUIManager.Instance.WorldToUIPosition(tower.transform.position);
        RectTransform panelRect = selectTowerPanel.GetComponent<RectTransform>();
        panelRect.anchoredPosition = uiPos;

        HideAllTowerPanels();

        selectedTower = tower;
        selectedBuildType = null;

		selectTowerPanel.SetActive(true);
		priceText.text = $"{selectedTower.PreviewLevelData.cost}";

        buildCheckedIcon.SetActive(isChecked);
        buildUncheckedIcon.SetActive(isChecked);
        towerIcon.SetActive(!isChecked);

        buildTowerPanel.SetActive(false);

        MarkTowerPanelJustOpened();
    }

    public void HideTowerBuildPanel()
    {
        selectTowerPanel.SetActive(false);

        if (!firstTowerBuilt)
        {
            buildTowerPanel.SetActive(true);
        }
    }

    public void ShowSelectedTowerPanel(Tower tower)
    {
        if (isProgressActive) return; // Block selection during progress

        Vector2 uiPos = GameUIManager.Instance.WorldToUIPosition(tower.transform.position);
        RectTransform panelRect = selectedTowerPanel.GetComponent<RectTransform>();
        panelRect.anchoredPosition = uiPos;

        HideAllTowerPanels();
        selectedTowerPanel.SetActive(true);

        selectedTower = tower;
        selectedTower.ShowRange();

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

        upgradeConfirmed = false;
        upgradeCheckedIcon.SetActive(false);
        upgradeIcon.SetActive(false);
        upgradeIcon.SetActive(true);

        sellConfirmed = false;
        sellCheckedIcon.SetActive(false);
        sellIcon.SetActive(true);

        HideTowerDetails();
		towerInformationPanel.SetActive(false);
    }

    private void ShowTowerDetails(string title, string description, int? minDamage, int? maxDamge)
    {
        Vector2 uiPos = GameUIManager.Instance.WorldToUIPosition(selectedTower.transform.position + new Vector3(12, 0f, 0));
        RectTransform detailsRect = detailsPanel.GetComponent<RectTransform>();
        detailsRect.anchoredPosition = uiPos;

        detailsPanel.SetActive(true);

        towerTitle.text = title ?? "Unknown Tower";
        towerDescription.text = description ?? "No description available.";

        if (minDamage.HasValue && maxDamge.HasValue)
        {
            towerParameters.SetActive(true);
            TowerDamage.text = $"{minDamage.Value}-{maxDamge.Value}";
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

    private void OnArcherTowerButtonClicked()
    {
        if (selectedTower == null) return;

        if (selectedTower.IsBuilt)
        {
            if (!selectedTower.CanUpgrade)
            {
                Debug.LogWarning("[TowerUIManager] Tower already at max level.");
                return;
            }

            var nextLevel = selectedTower.GetNextLevelData();

            if (!GameManager.Instance.SpendCoins(nextLevel.cost))
            {
                Debug.LogWarning("[TowerUIManager] Not enough coins to upgrade Tower.");
                return;
            }

			if (upgradeCoroutine != null)
			    StopCoroutine(upgradeCoroutine);
			upgradeCoroutine = StartCoroutine(ShowUpgradeProgress(1f, selectedTower));

			selectedTower.Upgrade();
            HideAllTowerPanels();
            return;
        }

		int cost = selectedTower.PreviewLevelData.cost;

		if (selectedBuildType != "Archer")
		{
			selectedBuildType = "Archer";
			bool enoughCoins = GameManager.Instance.CurrentCoints >= cost;

			buildCheckedIcon.SetActive(enoughCoins);
			buildUncheckedIcon.SetActive(!enoughCoins);
			towerIcon.SetActive(false);

			selectedTower.ShowUpgradeRangeOnly();
			ShowTowerDetails(
				selectedTower.PreviewLevelData.displayName,
				selectedTower.PreviewLevelData.description,
				selectedTower.PreviewLevelData.arrowTier.minDamage,
                selectedTower.PreviewLevelData.arrowTier.maxDamage
			);

			return;
		}

		if (!GameManager.Instance.SpendCoins(cost))
		{
			return;
		}


		// Start upgrade bar
		if (upgradeCoroutine != null)
			StopCoroutine(upgradeCoroutine);
		upgradeCoroutine = StartCoroutine(ShowUpgradeProgress(1f, selectedTower));

		selectedTower.Upgrade();
        firstTowerBuilt = true;

        buildTowerPanel.SetActive(false);
        selectedTower.HideUpgradeRange();
        HideAllTowerPanels();

        selectedBuildType = null;
    }

    private void OnUpgradeButtonClicked()
    {
        if (selectedTower == null || !selectedTower.IsBuilt)
            return;

        if (!selectedTower.CanUpgrade)
        {
            Debug.LogWarning("[TowerUIManager] Tower already at max level.");
            return;
        }

        var nextLevel = selectedTower.GetNextLevelData();
        int cost = nextLevel.cost;
        bool enoughCoins = GameManager.Instance.CurrentCoints >= cost;

        if (!upgradeConfirmed)
        {
            upgradeConfirmed = true;
            upgradeCheckedIcon.SetActive(enoughCoins);
            upgradeUncheckedIcon.SetActive(!enoughCoins);
            upgradeIcon.SetActive(false);

            ShowTowerDetails(
                nextLevel.displayName,
                nextLevel.description,
                nextLevel.arrowTier.minDamage,
                nextLevel.arrowTier.maxDamage
            );

            selectedTower.ShowBothRange();

            if (sellConfirmed)
            {
                sellConfirmed = false;
                sellCheckedIcon.SetActive(false);
                sellIcon.SetActive(true);
            }

            return;
        }

        if (!GameManager.Instance.SpendCoins(cost))
        {
            return;
        }

		// Start upgrade bar
		if (upgradeCoroutine != null)
			StopCoroutine(upgradeCoroutine);
		upgradeCoroutine = StartCoroutine(ShowUpgradeProgress(1f, selectedTower));

        selectedTower.Upgrade();
        HideAllTowerPanels();

        upgradeConfirmed = false;
        upgradeCheckedIcon.SetActive(false);
        upgradeUncheckedIcon.SetActive(false);
        upgradeIcon.SetActive(true);
    }

    private void OnSellButtonClicked()
    {
        if (selectedTower == null || !selectedTower.IsBuilt)
            return;

        if (!sellConfirmed)
        {
            sellConfirmed = true;
            sellCheckedIcon.SetActive(true);
            sellIcon.SetActive(false);

            int refund = selectedTower.GetSellRefundAmount();

            ShowTowerDetails(
                "Sell Tower",
                $"Sell this tower and get for <color=yellow>{refund} coins</color>",
                null, null
            );

            if (upgradeConfirmed)
            {
                upgradeConfirmed = false;
                upgradeCheckedIcon.SetActive(false);
                upgradeIcon.SetActive(true);
            }

            return;
        }

        selectedTower.Sell();
        HideAllTowerPanels();

        sellConfirmed = false;
        sellCheckedIcon.SetActive(false);
        sellIcon.SetActive(true);
    }

	public void ShowTowerInfoPanel(Tower tower)
	{
		if (tower == null || !tower.IsBuilt || isProgressActive) return;

		var currentLevel = tower.CurrentLevelData;

		nameText.text = currentLevel.displayName;
		attackValueText.text = $"{currentLevel.arrowTier.minDamage}-{currentLevel.arrowTier.maxDamage}";

		// Fire Rate Label
		float fireRate = currentLevel.archerTier.fireRate;
		string fireRateLabel;
		if (fireRate > 1f)
			fireRateLabel = "Low";
		else if (fireRate > 0.7f)
			fireRateLabel = "Med";
		else if (fireRate > 0.4f)
			fireRateLabel = "Fast";
		else
			fireRateLabel = "High";
		fireRateValueText.text = fireRateLabel;

		// Accuracy Label
		float accuracy = currentLevel.arrowTier.accuracy;
		string accuracyLabel;
		if (accuracy < 0.6f)
			accuracyLabel = "Low";
		else if (accuracy < 0.8f)
			accuracyLabel = "Med";
		else if (accuracy < 0.95f)
			accuracyLabel = "High";
		else
			accuracyLabel = "Perfect";
		accuracyValueText.text = accuracyLabel;

		avatarImage.sprite = currentLevel.icon;
		towerInformationPanel.SetActive(true);
	}

	private IEnumerator ShowUpgradeProgress(float duration, Tower tower)
	{
        isProgressActive = true; // Block clicks

        // Calculate UI position from tower
        Vector2 getPos = GameUIManager.Instance.WorldToUIPosition(tower.transform.position);
		getPos.y += 130f;
		RectTransform panelRect = buildProgressPanel.GetComponent<RectTransform>();
        panelRect.anchoredPosition = getPos;

        buildProgressPanel.SetActive(true);
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float percent = time / duration;

            progressValueImage.fillAmount = percent;

            yield return null;
        }

        buildProgressPanel.SetActive(false);
        isProgressActive = false; // Allow clicks again
	}
}

