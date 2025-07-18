using UnityEngine;

public class MissEffect : MonoBehaviour
{
	private void OnEnable()
	{
		// Optionally: play animation/sound

		// Disable after 0.3 second
		Invoke(nameof(DisableSelf), 0.3f);
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

