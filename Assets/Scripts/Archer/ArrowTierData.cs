using UnityEngine;

[CreateAssetMenu(fileName = "ArrowTierData", menuName = "Tower/Arrow Tier Data")]
public class ArrowTierData : ScriptableObject
{
	public float speed;
	public float accuracy;
	public Sprite[] directionSprites;
	public GameObject arrowPrefab;

	public int minDamage;
	public int maxDamage;

	public float critChance;      
	public float critMultiplier;  
}
