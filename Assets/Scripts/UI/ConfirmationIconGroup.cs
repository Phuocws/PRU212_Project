using UnityEngine;

public class ConfirmationIconGroup
{
	public GameObject CheckedIcon;
	public GameObject DeniedIcon;
	public GameObject DefaultIcon;

	public void SetState(bool isConfirmed, bool isDenied)
	{
		CheckedIcon?.SetActive(isConfirmed);
		DeniedIcon?.SetActive(isDenied);
		DefaultIcon?.SetActive(!isConfirmed && !isDenied);
	}

	public void Reset() => SetState(false, false);
}
