using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class EnemyClickManager : MonoBehaviour
{
	void Update()
	{
		if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
			return;
		if (EventSystem.current.IsPointerOverGameObject()) return;

		Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
		Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);

		if (hit != null)
		{
			BaseEnemy enemy = hit.GetComponent<BaseEnemy>();
			if (enemy != null)
			{
				GameUIManager.Instance.ShowEnemyInfo(enemy, enemy.Avatar);
				return;
			}
		}

		// Clicked outside: hide
		if (GameUIManager.Instance.IsEnemyInfoPanelActive())
		{
			GameUIManager.Instance.HideEnemyInfo();
		}
	}
}
