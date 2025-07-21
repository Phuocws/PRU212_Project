using UnityEngine;

public class AutoDespawn : MonoBehaviour
{
	[SerializeField] private float despawnDelay = 1f;

	private void OnEnable()
	{
		CancelInvoke();
		Invoke(nameof(Despawn), despawnDelay);
	}

	private void Despawn()
	{
		ObjectPool.Instance.DespawnToPool(gameObject);
	}
}

