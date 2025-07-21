using UnityEngine;

public class PathEndTrigger : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D other)
	{
		// Optional: check if the object has BaseEnemy (more flexible than tag check)
		if (other.TryGetComponent(out BaseEnemy enemy))
		{
			GameManager.Instance.TakeDamage(enemy.damageAtEndOfPath);
			enemy.gameObject.SetActive(false);
			AudioManager.Instance.PlaySound(AudioManager.Instance.oof);
			if (GameUIManager.Instance != null)
				GameUIManager.Instance.HideEnemyInfoIfSelected(enemy);
			GameManager.Instance.AddCoins(enemy.coinValue);
			EnemyTracker.Instance.UnregisterEnemy();
		}
	}
}