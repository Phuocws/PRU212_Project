using UnityEngine;

public class PathEndTrigger : MonoBehaviour
{
	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Orc") || other.CompareTag("Wolf"))
		{
			Debug.Log("Enemy reached the end!");

			// Later: Deal damage to base here
			Destroy(other.gameObject);
		}
	}
}
