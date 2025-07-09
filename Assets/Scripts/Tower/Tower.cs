using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Tower : MonoBehaviour
{
	[Header("Tower Configuration")]
	[SerializeField] private List<TowerLevelData> levelData;
	[SerializeField] private Transform visualRoot;
	public TowerLevelData PreviewLevelData => levelData != null && levelData.Count > 0 ? levelData[0] : null;

	private SpriteRenderer spriteRenderer;
	private GameObject currentVisual;
	private Animator currentAnimator;
	private readonly List<Archer> archerList = new();

	private int currentLevel = -1; // -1 = unbuilt, 0+ = built

	public bool IsBuilt => currentLevel >= 0;
	public bool CanUpgrade => currentLevel + 1 < levelData.Count;

	private void Start()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();
		spriteRenderer.enabled = false; 

		if (levelData == null || levelData.Count == 0)
		{
			Debug.LogWarning("[Tower] No level data assigned.");
		}

		//Debug.Log($"[Tower] {PreviewLevelData.archerCount}.");
	}

	private void Update()
	{
		HandleMouseClick(); 
	}

	private void HandleMouseClick()
	{
		if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
			return;

		if (UIBlocked()) return;

		Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
		Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);

		if (hit != null && hit.gameObject == this.gameObject)
		{
			if (IsBuilt)
				UIManager.Instance.ShowSelectedTowerPanel(this); // upgrade/sell
			else
			{
				UIManager.Instance.ShowTowerBuildPanel(false, this); // build
			}
		}
	}

	private bool UIBlocked()
	{
		return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
	}

	/// <summary>
	/// Triggered by UI button (on build or upgrade)
	/// </summary>
	public void Upgrade()
	{
		if (!CanUpgrade)
		{
			Debug.Log("[Tower] Already at max level.");
			return;
		}

		StartCoroutine(ApplyLevelCoroutine(currentLevel + 1));
	}

	/// <summary>
	/// Main logic for applying tower level (visuals + animation wait)
	/// </summary>
	private IEnumerator ApplyLevelCoroutine(int levelIndex)
	{
		if (levelIndex < 0 || levelIndex >= levelData.Count)
		{
			Debug.LogError($"[Tower] Invalid level index: {levelIndex}");
			yield break;
		}

		currentLevel = levelIndex;
		TowerLevelData data = levelData[currentLevel];

		// Despawn previous visual
		if (currentVisual != null)
		{
			ObjectPool.Instance.DespawnToPool(currentVisual);
			currentVisual = null;
		}

		// Spawn new visual
		string visualTag = $"Tower{currentLevel + 1}";
		currentVisual = ObjectPool.Instance.SpawnFromPool(visualTag, visualRoot.position, Quaternion.identity, visualRoot);

		if (currentVisual == null)
		{
			Debug.LogError($"[Tower] Failed to spawn visual: {visualTag}");
			yield break;
		}

		currentVisual.transform.SetParent(visualRoot, false);

		DespawnAllArchers();

		// Setup relay for animation callbacks
		if (currentVisual.TryGetComponent(out TowerEventRelay relay))
			relay.SetTower(this);

		// Wait for idle animation to finish
		currentAnimator = currentVisual.GetComponent<Animator>();
		if (currentAnimator != null)
		{
			yield return new WaitUntil(() =>
			{
				var state = currentAnimator.GetCurrentAnimatorStateInfo(0);
				return state.IsTag("Idle") && state.normalizedTime >= 1f;
			});
		}
	}

	/// <summary>
	/// Called via animation event after idle transition
	/// </summary>
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

			if (archerGO.TryGetComponent<Archer>(out var archer))
			{
				archer.Initialize(data.archerTier, data.arrowTier, data.range);
				archerList.Add(archer);
			}
			else
			{
				Debug.LogError($"[Tower] Archer missing script: {archerTag}");
			}
		}
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

	public void Sell()
	{
		if (!IsBuilt)
		{
			Debug.LogWarning("[Tower] Tried to sell an unbuilt tower.");
			return;
		}

		// Refund partial coin (e.g., 50% of total invested)
		int refund = 0;
		for (int i = 0; i <= currentLevel; i++)
			refund += Mathf.FloorToInt(levelData[i].cost * 0.5f);

		GameManager.Instance.AddCoins(refund);
		Debug.Log($"[Tower] Tower sold. Refunded {refund} coins.");

		// Despawn visual
		if (currentVisual != null)
		{
			ObjectPool.Instance.DespawnToPool(currentVisual);
			currentVisual = null;
		}

		// Despawn archers
		DespawnAllArchers();

		// Reset state
		currentLevel = -1;
		spriteRenderer.enabled = false;
	}

	public int GetSellRefundAmount()
	{
		int refund = 0;
		for (int i = 0; i <= currentLevel; i++)
			refund += Mathf.FloorToInt(levelData[i].cost * 0.5f);
		return refund;
	}

}
