using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class WaveUIManager : MonoBehaviour
{
    public static WaveUIManager Instance { get; private set; }


    [SerializeField] private GameObject startWaveButton;
    [SerializeField] private Image fillImage;
    [SerializeField] private GameObject waveDetailPanel;
    [SerializeField] private TextMeshProUGUI waveTitleText;
    [SerializeField] private TextMeshProUGUI waveDetailsText;
    [SerializeField] private TextMeshProUGUI waveInstructionText;
    [SerializeField] private GameObject startBattlePanel;

    private float countdownTime;
    private float timer;
    private bool isCounting;
    private bool autoTriggerNextWave;
    private bool isWaveDetailShown = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject);
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

        CheckClickOutsideUI();
    }

    public void CheckClickOutsideUI()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                if (isWaveDetailShown)
                    HideWaveDetail();

                // Hide tower panels if open
                if (TowerUIManager.Instance != null)
                    TowerUIManager.Instance.HideAllTowerPanels();
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

        // Hide all tower panels if open
        if (TowerUIManager.Instance != null)
            TowerUIManager.Instance.HideAllTowerPanels();
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
