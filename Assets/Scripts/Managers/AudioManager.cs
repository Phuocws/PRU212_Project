using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioSource shoot;
	public AudioSource SlimeDeath;

	private void Awake()
	{
		if (Instance == null) Instance = this;
		else { Destroy(gameObject); return; }
		DontDestroyOnLoad(gameObject);
	}

	public void PlaySound(AudioSource audio)
    {
        audio.Stop();
        audio.Play();
	}
}
