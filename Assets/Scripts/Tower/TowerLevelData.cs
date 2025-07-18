using UnityEngine;

[CreateAssetMenu(menuName = "Tower/Level Data")]
public class TowerLevelData : ScriptableObject
{
	public GameObject towerVisualPrefab;
	public Sprite icon;
	public string displayName;
	public string description;
	public int cost;
	public int archerCount;
	public float range;

	[Header("Tier Configs")]
	public ArcherTierData archerTier;
	public ArrowTierData arrowTier;
}