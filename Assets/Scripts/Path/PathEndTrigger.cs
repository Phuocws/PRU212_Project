using UnityEngine;

public class PathEndTrigger : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D other)
	{
		// Optional: check if the object has BaseEnemy (more flexible than tag check)
		if (other.TryGetComponent(out BaseEnemy enemy))
		{
			enemy.gameObject.SetActive(false); // Return to pool
			// Optionally: apply damage to player base here
			// GameManager.Instance.TakeDamage(enemy.DamageValue); 
		}
	}
}