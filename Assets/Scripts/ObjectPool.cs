using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ObjectPool is a singleton-based pooling system for reusing GameObjects efficiently.
/// </summary>
public class ObjectPool : MonoBehaviour
{
	public static ObjectPool Instance;

	[System.Serializable]
	public class Pool
	{
		public string tag;              // Unique key for the pool
		public GameObject prefab;       // Prefab to instantiate
		public int size = 10;           // Initial size of the pool
	}

	[Header("Pooling Settings")]
	public List<Pool> pools;

	private Dictionary<string, List<GameObject>> poolDictionary;

	private void Awake()
	{
		Instance = this;
		InitializePools();
	}

	/// <summary>
	/// Pre-instantiates all objects defined in the pool list.
	/// </summary>
	private void InitializePools()
	{
		poolDictionary = new Dictionary<string, List<GameObject>>();

		foreach (var pool in pools)
		{
			var objectList = new List<GameObject>();

			for (int i = 0; i < pool.size; i++)
			{
				GameObject obj = Instantiate(pool.prefab);
				obj.SetActive(false);
				objectList.Add(obj);
			}

			poolDictionary[pool.tag] = objectList;
		}
	}

	/// <summary>
	/// Spawns an object from the pool by tag. Automatically expands the pool if necessary.
	/// </summary>
	/// <param name="tag">Pool tag</param>
	/// <param name="position">Position to spawn</param>
	/// <param name="rotation">Rotation to spawn</param>
	/// <returns>The pooled GameObject</returns>
	public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation, Transform parent = null)
	{
		if (!poolDictionary.TryGetValue(tag, out var pool))
		{
			Debug.LogWarning($"[ObjectPool] Pool with tag '{tag}' does not exist.");
			return null;
		}

		// Try to find an inactive object
		GameObject objectToSpawn = pool.Find(obj => !obj.activeInHierarchy);

		if (objectToSpawn == null)
		{
			// Pool exhausted — expand
			var poolConfig = pools.Find(p => p.tag == tag);
			if (poolConfig == null)
			{
				Debug.LogError($"[ObjectPool] No prefab config found for tag '{tag}'");
				return null;
			}

			objectToSpawn = Instantiate(poolConfig.prefab);
			objectToSpawn.SetActive(false);
			pool.Add(objectToSpawn);

			// Optionally: Log or handle expanded pools here
			// Debug.LogWarning($"[{tag}] Pool exhausted — auto-expanded by 1.");
		}

		objectToSpawn.SetActive(true);

		// Parenting first to allow localPosition reset
		if (parent != null)
		{
			objectToSpawn.transform.SetParent(parent, false); // keep local
			objectToSpawn.transform.localPosition = Vector3.zero;
			objectToSpawn.transform.localRotation = Quaternion.identity;
			objectToSpawn.transform.localScale = Vector3.one;
		}
		else
		{
			objectToSpawn.transform.position = position;
			objectToSpawn.transform.rotation = rotation;
		}

		return objectToSpawn;
	}

	public void DespawnToPool(GameObject obj)
	{
		if (obj == null) return;
		obj.SetActive(false);
		obj.transform.SetParent(transform); // Optional: keep hierarchy clean
	}
}
