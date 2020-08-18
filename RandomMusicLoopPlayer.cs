using Godot;
using System;
using System.Collections.Generic;

public class RandomMusicLoopPlayer : AudioStreamPlayer
{
	[Export]
	Godot.Collections.Array SoundsList;

	List<AudioStream> AudioStreamList = new List<AudioStream>();
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		foreach (Godot.Object s in SoundsList)
		{
			if (s is AudioStream)
			{
				AudioStreamList.Add((AudioStream)s);
			}
		}
		PlayRandomSound();
	}

	public void PlayRandomSound()
	{
		int number = GameManager.Instance.Random.RandiRange(0, AudioStreamList.Count -1);
		Stream = AudioStreamList[number];
		Play();
	}
	
	private void _on_Music_finished()
	{
		PlayRandomSound();
	}

}
