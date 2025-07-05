using UnityEngine;

[CreateAssetMenu(fileName = "ArrowTierData", menuName = "Tower/Arrow Tier Data")]
public class ArrowTierData : ScriptableObject
{
	public int damage;
	public float speed;
	public Sprite[] directionSprites;
	public GameObject arrowPrefab;
	// Optional: status effects like burn, slow, etc.
}
