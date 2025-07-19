using UnityEngine;

public class CoinEffect : MonoBehaviour
{
	private void OnEnable()
	{
		// Optionally: play animation/sound

		// Disable after 1 second
		Invoke(nameof(DisableSelf), 1f);
	}

	private void DisableSelf()
	{
		gameObject.SetActive(false);
	}

	private void OnDisable()
	{
		CancelInvoke(); // avoid delayed calls stacking
	}
}
