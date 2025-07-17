using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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
    }

    private void ShowTowerDetails(string title, string description, int? damage)
    {
        Vector2 uiPos = GameUIManager.Instance.WorldToUIPosition(selectedTower.transform.position + new Vector3(12, 0f, 0));
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
				selectedTower.PreviewLevelData.arrowTier.damage
			);

			return;
		}

		if (!GameManager.Instance.SpendCoins(cost))
		{
			return;
		}

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
                nextLevel.arrowTier.damage
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
            Debug.LogWarning("[TowerUIManager] Not enough coins to upgrade Tower.");
            return;
        }

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
                null
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
}

