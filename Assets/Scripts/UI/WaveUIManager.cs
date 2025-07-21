using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class WaveUIManager : MonoBehaviour
{
    public static WaveUIManager Instance { get; private set; }

    [Header("Wave UI Elements")]
    [SerializeField] private GameObject startWaveButton;
    [SerializeField] private Image fillImage;
    [SerializeField] private GameObject waveDetailPanel;
    [SerializeField] private TextMeshProUGUI waveTitleText;
    [SerializeField] private TextMeshProUGUI waveDetailsText;
    [SerializeField] private TextMeshProUGUI waveInstructionText;
    [SerializeField] private GameObject startBattlePanel;

    [Header("Add Coin Panel")]
    [SerializeField] private GameObject addCoinPanel;
    [SerializeField] private TextMeshProUGUI addCoinText;
    [SerializeField] private float coinPanelDisplayDuration = 2f;

    private float coinPanelTimer;
    private bool isShowingCoinPanel;

    private float countdownTime;
    private float timer;
    private bool isCounting;
    private bool autoTriggerNextWave;
    private bool isWaveDetailShown = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Update()
    {
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
                    EnemySpawner.Instance.OnStartWaveClicked();
                }
            }
        }

        // Hide addCoinPanel after display duration
        if (isShowingCoinPanel)
        {
            coinPanelTimer += Time.deltaTime;
            if (coinPanelTimer >= coinPanelDisplayDuration)
            {
                addCoinPanel.SetActive(false);
                isShowingCoinPanel = false;
            }
        }

        CheckClickOutsideUI();
    }

    public void CheckClickOutsideUI()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (!EventSystem.current.IsPointerOverGameObject() && isWaveDetailShown)
            {
                HideWaveDetail();
            }
        }
    }

    public void OnStartWaveButtonClicked()
    {
        if (!isWaveDetailShown)
        {
            ShowWaveDetail();
        }
        else
        {
            int coinsToAdd = 0;
            if (isCounting && countdownTime > 0)
            {
                float remainingTime = Mathf.Max(0f, countdownTime - timer);
                coinsToAdd = Mathf.FloorToInt(remainingTime); // 1 coin per second left

                if (coinsToAdd > 0 && GameManager.Instance != null)
                {
                    AudioManager.Instance.PlaySound(AudioManager.Instance.sell);
					GameManager.Instance.AddCoins(coinsToAdd);

                    Vector2 coinUiPos = GameUIManager.Instance.WorldToUIPosition(startWaveButton.transform.position + new Vector3(31.11f, -2.035f, 0));
                    ShowAddCoinPanel(coinsToAdd);

                    ObjectPool.Instance.SpawnFromPool("CoinEffect", coinUiPos, Quaternion.identity);
                }
            }

            HideWaveDetail();
            startBattlePanel.SetActive(false);
            EnemySpawner.Instance.OnStartWaveClicked();
        }
    }

    public void ShowWaveDetail()
    {
        isWaveDetailShown = true;
        waveDetailPanel.SetActive(true);

        waveTitleText.text = "INCOMING WAVE";
        waveInstructionText.text = "TAP AGAIN TO CALL IT EARLY";

        var wave = EnemySpawner.Instance.GetCurrentWave();
        waveDetailsText.text = wave != null ? wave.GetWaveSummary() : "???";

        if (TowerUIManager.Instance != null)
            TowerUIManager.Instance.HideAllTowerPanels();

        if (GameUIManager.Instance != null)
            GameUIManager.Instance.HideEnemyInfo();
    }

    public void HideWaveDetail()
    {
        isWaveDetailShown = false;
        waveDetailPanel.SetActive(false);
    }

    public void StartFirstWaveButton()
    {
        isCounting = false;
        fillImage.fillAmount = 1f;
        startWaveButton.SetActive(true);
        startBattlePanel.SetActive(true);
	}

    public void StartCountdown(float duration, bool autoStart)
    {
        countdownTime = duration;
        timer = 0f;
        isCounting = true;
        autoTriggerNextWave = autoStart;

        fillImage.fillAmount = 0f;
        startWaveButton.SetActive(true);
        AudioManager.Instance.PlaySound(AudioManager.Instance.hawk);
	}

    public void ForceStopCountdown()
    {
        isCounting = false;
        timer = 0f;
        fillImage.fillAmount = 0f;
        startWaveButton.SetActive(false);
        HideWaveDetail();
    }

    private void ShowAddCoinPanel(int coinAmount)
    {
        addCoinText.text = $"+ {coinAmount}";
        addCoinPanel.SetActive(true);
        isShowingCoinPanel = true;
        coinPanelTimer = 0f;
    }
}
