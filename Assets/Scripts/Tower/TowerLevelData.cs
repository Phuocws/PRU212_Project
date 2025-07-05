using UnityEngine;

[CreateAssetMenu(menuName = "Tower/Level Data")]
public class TowerLevelData : ScriptableObject
{
	public GameObject towerVisualPrefab;
	public int archerCount;
	public float range;

	[Header("Tier Configs")]
	public ArcherTierData archerTier;
	public ArrowTierData arrowTier;
}