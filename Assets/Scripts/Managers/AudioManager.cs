using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance { get; private set; }

	[Header("Buttons")]
	public AudioSource buttonCLick;
	public AudioSource TowerButtonCLick;


	[Header("Archer")]
	public AudioSource shoot;
	public AudioSource hit;
	public AudioSource archerVoice1;
	public AudioSource archerVoice2;
	public AudioSource archerVoice3;

	[Header("Waves")]
	public AudioSource startFirstWave;
	public AudioSource hawk;
	public AudioSource inGame;

	[Header("Enemies Death")]
	public AudioSource slimeDeath;
	public AudioSource beeDeath;
	public AudioSource wolfDeath;
	public AudioSource wolf;
	public AudioSource orcDeath;
	public AudioSource oof;

	[Header("Enemies Moving")]
	public AudioSource flying;
	public AudioSource slimeMoving;
	public AudioSource orcMoving;
	public AudioSource wolfMoving;

	[Header("Game Events")]
	public AudioSource onGame;
	public AudioSource defeat;
	public AudioSource victory;

	[Header("Towers")]
	public AudioSource building;
	public AudioSource sell;
	public AudioSource towerClick;

	private readonly List<AudioSource> _pausedAudioSources = new();

	private void Awake()
	{
		if (Instance == null) Instance = this;
		else { Destroy(gameObject); return; }
	}

	public void PlaySound(AudioSource audio)
	{
		audio.Stop();
		audio.Play();
	}

	public void StopSound(AudioSource audio)
	{
		audio.Stop();
	}

	public void PlayLoop(AudioSource source)
	{
		if (source == null || source.isPlaying) return;
		if (!source.isPlaying)
		{
			source.loop = true;
			source.Play();
		}
	}

	public void StopLoop(AudioSource source)
	{
		if (source.isPlaying)
		{
			source.Stop();
			source.loop = false;
		}
	}

	public void PlayRandomArcherVoice()
	{
		int idx = Random.Range(1, 4); // 1, 2, or 3
		switch (idx)
		{
			case 1:
				if (archerVoice1 != null) archerVoice1.Play();
				break;
			case 2:
				if (archerVoice2 != null) archerVoice2.Play();
				break;
			case 3:
				if (archerVoice3 != null) archerVoice3.Play();
				break;
		}
	}

	public void PlayButtonClickSound()
	{
		if (buttonCLick != null)
		{
			buttonCLick.Stop();
			buttonCLick.Play();
		}
	}

	public void PauseAllGameAudio()
	{
		_pausedAudioSources.Clear();

		// List all that might be playing during gameplay
		AudioSource[] toPause = {
		inGame, onGame, flying, wolfMoving, orcMoving, slimeMoving, wolf, hawk, slimeDeath, beeDeath, wolfDeath, orcDeath, oof, startFirstWave
	};

		foreach (var audio in toPause)
		{
			if (audio != null && audio.isPlaying)
			{
				audio.Pause();
				_pausedAudioSources.Add(audio);
			}
		}
	}

	public void ResumeAllGameAudio()
	{
		foreach (var audio in _pausedAudioSources)
		{
			if (audio != null)
				audio.UnPause();
		}
		_pausedAudioSources.Clear();
	}
}