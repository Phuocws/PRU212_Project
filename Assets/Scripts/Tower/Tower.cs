using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tower : MonoBehaviour
{
	[Header("Tower Configuration")]
	[SerializeField] private List<TowerLevelData> levelData;
	[SerializeField] private Transform visualRoot;

	private SpriteRenderer spriteRenderer;
	private GameObject currentVisual;
	private Animator currentAnimator;
	private readonly List<Archer> archerList = new();

	private int currentLevel = -1; // Start at -1 so level 0 is first upgrade

	/// <summary>
	/// Can upgrade if next level is within bounds
	/// </summary>
	public bool CanUpgrade => currentLevel + 1 < levelData.Count;

	private void Start()
	{
		spriteRenderer = GetComponent<SpriteRenderer>();

		if (levelData == null || levelData.Count == 0)
		{
			Debug.LogWarning("[Tower] No level data assigned.");
			return;
		}

		StartCoroutine(AutoUpgradeAfterDelay(10f));
	}

	/// <summary>
	/// Attempts to upgrade the tower to the next level
	/// </summary>
	public void Upgrade()
	{
		if (!CanUpgrade)
		{
			Debug.Log("[Tower] Already at max level.");
			return;
		}

		int nextLevel = currentLevel + 1;
		StartCoroutine(ApplyLevelCoroutine(nextLevel));
	}

	/// <summary>
	/// Applies the given tower level: visuals, animation, and setup
	/// </summary>
	private IEnumerator ApplyLevelCoroutine(int levelIndex)
	{
		if (levelIndex < 0 || levelIndex >= levelData.Count)
		{
			Debug.LogError($"[Tower] Invalid level index {levelIndex}");
			yield break;
		}

		currentLevel = levelIndex;
		TowerLevelData data = levelData[currentLevel];

		// Despawn current visual
		if (currentVisual != null)
		{
			ObjectPool.Instance.DespawnToPool(currentVisual);
			currentVisual = null;
		}

		string visualTag = $"Tower{currentLevel + 1}";
		currentVisual = ObjectPool.Instance.SpawnFromPool(visualTag, visualRoot.position, Quaternion.identity, visualRoot);

		if (currentVisual == null)
		{
			Debug.LogError($"[Tower] Failed to spawn visual: {visualTag}");
			yield break;
		}

		currentVisual.transform.SetParent(visualRoot, false);
		DespawnAllArchers();

		// Setup event relay
		if (currentVisual.TryGetComponent(out TowerEventRelay relay))
			relay.SetTower(this);

		currentAnimator = currentVisual.GetComponent<Animator>();

		// Wait for idle animation state
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
	/// Called via animation event after upgrade visuals are done
	/// </summary>
	public void InitializeArchersForLevel()
	{
		if (currentLevel < 0 || currentLevel >= levelData.Count)
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

	private IEnumerator AutoUpgradeAfterDelay(float delay)
	{
		while (CanUpgrade)
		{
			yield return new WaitForSeconds(delay);
			Upgrade();

			if (spriteRenderer.enabled)
				spriteRenderer.enabled = false;
		}
	}
}
