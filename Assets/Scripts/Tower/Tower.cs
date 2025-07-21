using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Tower : MonoBehaviour
{
	[Header("Tower Configuration")]
	[SerializeField] private List<TowerLevelData> levelData;
	[SerializeField] private Transform visualRoot;
	[SerializeField] private GameObject currentRangeIndicator;
	[SerializeField] private GameObject upgradeRangeIndicator;
	public TowerLevelData PreviewLevelData => levelData != null && levelData.Count > 0 ? levelData[0] : null;
	public TowerLevelData CurrentLevelData
	{
		get
		{
			if (currentLevel >= 0 && currentLevel < levelData.Count)
				return levelData[currentLevel];
			return null;
		}
	}

	private GameObject currentVisual;
	private Animator currentAnimator;
	private readonly List<Archer> archerList = new();
	private int currentLevel = -1;

	public bool IsBuilt => currentLevel >= 0;
	public bool CanUpgrade => currentLevel + 1 < levelData.Count;

	#region Unity Lifecycle

	private void Start()
	{
		if (levelData == null || levelData.Count == 0)
			Debug.LogWarning("[Tower] No level data assigned.");

		HideBothRange();
		UpdateRangeVisual();
	}

	private void Update()
	{
		HandleMouseClick();
	}

	#endregion

	#region Mouse Click

	private void HandleMouseClick()
	{
		if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
			return;

		if (UIBlocked()) return;

		Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
		Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);

		if (hit != null && hit.gameObject == gameObject)
		{
			AudioManager.Instance.PlaySound(AudioManager.Instance.towerClick);
			if (IsBuilt)
			{
				TowerUIManager.Instance.ShowSelectedTowerPanel(this);
				TowerUIManager.Instance.ShowTowerInfoPanel(this);
			}
			else
			{
				TowerUIManager.Instance.ShowTowerBuildPanel(false, this);
			}
		}
	}

	private bool UIBlocked()
	{
		return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
	}

	#endregion

	#region Upgrade / Build

	public void Upgrade()
	{
		if (!CanUpgrade)
		{
			Debug.Log("[Tower] Already at max level.");
			return;
		}

		StartCoroutine(ApplyLevelCoroutine(currentLevel + 1));
	}

	private IEnumerator ApplyLevelCoroutine(int levelIndex)
	{
		if (levelIndex < 0 || levelIndex >= levelData.Count)
		{
			Debug.LogError($"[Tower] Invalid level index: {levelIndex}");
			yield break;
		}

		currentLevel = levelIndex;
		TowerLevelData data = levelData[currentLevel];

		// Despawn old
		if (currentVisual != null)
		{
			ObjectPool.Instance.DespawnToPool(currentVisual);
			currentVisual = null;
		}
		DespawnAllArchers();

		// Spawn new
		string visualTag = $"Tower{currentLevel + 1}";
		currentVisual = ObjectPool.Instance.SpawnFromPool(visualTag, visualRoot.position, Quaternion.identity, visualRoot);

		if (currentVisual == null)
		{
			Debug.LogError($"[Tower] Failed to spawn visual: {visualTag}");
			yield break;
		}
		currentVisual.transform.SetParent(visualRoot, false);

		if (currentVisual.TryGetComponent(out TowerEventRelay relay))
			relay.SetTower(this);

		currentAnimator = currentVisual.GetComponent<Animator>();
		if (currentAnimator != null)
		{
			yield return new WaitUntil(() =>
			{
				var state = currentAnimator.GetCurrentAnimatorStateInfo(0);
				return state.IsTag("Idle") && state.normalizedTime >= 1f;
			});
		}

		UpdateRangeVisual();
	}

	public void InitializeArchersForLevel()
	{
		if (!IsBuilt || currentLevel >= levelData.Count)
		{
			Debug.LogError($"[Tower] Invalid level index for archer init: {currentLevel}");
			return;
		}

		TowerLevelData data = levelData[currentLevel];
		List<Transform> slots = FindArcherSlots(currentVisual);

		for (int i = 0; i < data.archerCount && i < slots.Count; i++)
		{
			Transform slot = slots[i];
			string archerTag = $"Archer{data.archerTier.tier}";

			GameObject archerGO = ObjectPool.Instance.SpawnFromPool(archerTag, slot.position, Quaternion.identity, slot);
			if (archerGO == null)
			{
				Debug.LogError($"[Tower] Failed to spawn archer: {archerTag}");
				continue;
			}

			archerGO.transform.SetParent(slot, false);

			if (archerGO.TryGetComponent(out Archer archer))
			{
				archer.Initialize(data.archerTier, data.arrowTier, data.range, transform);
				archerList.Add(archer);
			}
			else
			{
				Debug.LogError($"[Tower] Archer missing script: {archerTag}");
			}
		}
		AudioManager.Instance.PlayRandomArcherVoice();
	}

	private void DespawnAllArchers()
	{
		foreach (var archer in archerList)
		{
			if (archer != null)
			{
				archer.ResetArcher();
				ObjectPool.Instance.DespawnToPool(archer.gameObject);
			}
		}
		archerList.Clear();
	}

	#endregion

	#region Utility

	private List<Transform> FindArcherSlots(GameObject visual)
	{
		List<Transform> slots = new();
		Transform parent = visual.transform.Find("ArcherSlots");

		if (parent == null)
		{
			Debug.LogWarning("[Tower] No ArcherSlots found.");
			return slots;
		}

		foreach (Transform child in parent)
			slots.Add(child);

		return slots;
	}

	public TowerLevelData GetNextLevelData()
	{
		return CanUpgrade ? levelData[currentLevel + 1] : null;
	}

	#endregion

	#region Sell

	public void Sell()
	{
		AudioManager.Instance.PlaySound(AudioManager.Instance.sell);
		if (!IsBuilt)
		{
			Debug.LogWarning("[Tower] Tried to sell an unbuilt tower.");
			return;
		}

		// Refund logic
		int refund = 0;
		for (int i = 0; i <= currentLevel; i++)
			refund += Mathf.FloorToInt(levelData[i].cost * 0.5f);
		GameManager.Instance.AddCoins(refund);

		// Reset state
		if (currentVisual != null)
		{
			ObjectPool.Instance.DespawnToPool(currentVisual);
			currentVisual = null;
		}
		DespawnAllArchers();

		currentLevel = -1;
		HideBothRange();
		ResetRangeScales();
		UpdateRangeVisual(); // for new preview upgrade range
	}

	public int GetSellRefundAmount()
	{
		int refund = 0;
		for (int i = 0; i <= currentLevel; i++)
			refund += Mathf.FloorToInt(levelData[i].cost * 0.5f);
		return refund;
	}

	#endregion

	#region Range Visuals

	private void UpdateRangeVisual()
	{
		// Preview upgrade range
		float diameter = PreviewLevelData.range * 2f;
		upgradeRangeIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);

		// Built tower range
		if (IsBuilt && currentLevel < levelData.Count)
		{
			float currentRange = levelData[currentLevel].range * 2f;
			currentRangeIndicator.transform.localScale = new Vector3(currentRange, currentRange, 1f);
		}

		// Next upgrade range
		if (CanUpgrade)
		{
			float upgradeRange = levelData[currentLevel + 1].range * 2f;
			upgradeRangeIndicator.transform.localScale = new Vector3(upgradeRange, upgradeRange, 1f);
		}
	}

	private void ResetRangeScales()
	{
		currentRangeIndicator.transform.localScale = Vector3.zero;
		upgradeRangeIndicator.transform.localScale = Vector3.zero;
	}

	public void ShowCurrentRange() => currentRangeIndicator.SetActive(true);
	public void HideCurrentRange() => currentRangeIndicator.SetActive(false);

	public void ShowUpgradeRange()
	{
		if (CanUpgrade)
		{
			upgradeRangeIndicator.SetActive(true);
			UpdateRangeVisual();
		}
	}

	public void HideUpgradeRange() => upgradeRangeIndicator.SetActive(false);

	public void ShowRange()
	{
		ShowCurrentRange();
		HideUpgradeRange();
	}

	public void ShowUpgradeRangeOnly()
	{
		HideCurrentRange();
		ResetRangeScales();
		UpdateRangeVisual();
		ShowUpgradeRange();
	}

	public void ShowBothRange()
	{
		ShowCurrentRange();
		ShowUpgradeRange();
	}

	public void HideBothRange()
	{
		HideCurrentRange();
		HideUpgradeRange();
	}

	#endregion
}
