using UnityEngine;

[CreateAssetMenu(fileName = "ArcherTierData", menuName = "Tower/Archer Tier Data")]
public class ArcherTierData : ScriptableObject
{
	public float fireRate;
	public int arrowsPerShoot;
	public GameObject archerPrefab;
	public int tier;
}