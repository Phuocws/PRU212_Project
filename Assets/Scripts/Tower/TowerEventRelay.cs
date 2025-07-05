using System.Collections;
using UnityEngine;

public class TowerEventRelay : MonoBehaviour
{
	private Tower tower;

	public void TriggerArcherInit()
	{
		if (tower == null)
		{
			Debug.LogError("[TowerEventRelay] No Tower reference found.");
			return;
		}
		tower.InitializeArchersForLevel();
	}

	public void SetTower(Tower towerRef)
	{
		tower = towerRef;
	}
}

