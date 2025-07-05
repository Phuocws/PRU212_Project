using UnityEngine;

[CreateAssetMenu(fileName = "ArcherTierData", menuName = "Tower/Archer Tier Data")]
public class ArcherTierData : ScriptableObject
{
	public float shootSpeed;
	public int arrowsPerShoot;
	public GameObject archerPrefab;
	public int tier;
}