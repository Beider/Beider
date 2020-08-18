using Godot;
using System;

/// <summary>
/// A simple class for easily loading and playing sound effects that may or may not exist.
/// As I have multiple different types of sound effects this class handles them all so I can skip custom code for each type.
/// Just create it with new SoundEffect("Path/To/Sound", this) and then use .Play()
/// </summary>
public class SoundEffect
{
    private static readonly String GD_RANDOM_SOUND_FUNCTION_NAME = "PlayRandomSound";
    protected enum SoundType
    {
        NO_SOUND,
        NORMAL,
        RANDOM_SOUND_CSHARP,
        RANDOM_SOUND_GD
    }
    protected Node _node;
    protected AudioStreamPlayer2D _sound = null;
    protected RandomSoundPlayer _soundRnd = null;
    protected SoundType _type = SoundType.NO_SOUND;

    // Decides how often this sound can be played
    protected uint _delay = 0;

    protected uint _nextAllowedPlayTime = 0;

    public SoundEffect(String path, Node node) : this(path, node, 0)
    {
        // Do nothing
    }

    /// <summary> Create a new sound effect. The node parameter is the class that we look up the path relative from</summary>
    public SoundEffect(String path, Node node, uint delayInMilliseconds)
    {
        _delay = delayInMilliseconds;
        _node = (Node)node.GetNodeOrNull(path);
        if (_node == null)
        {
            // We got no sound
            _type = SoundType.NO_SOUND;
        }
        else if (_node is RandomSoundPlayer)
        {
            _type = SoundType.RANDOM_SOUND_CSHARP;
            _soundRnd = (RandomSoundPlayer)_node;
        }
        else if (_node.HasMethod(GD_RANDOM_SOUND_FUNCTION_NAME))
        {
            _type = SoundType.RANDOM_SOUND_GD;
            _sound = (AudioStreamPlayer2D)_node;
        }
        else if (_node is AudioStreamPlayer2D)
        {
            _sound = (AudioStreamPlayer2D)_node;
            if (_sound.Stream != null)
            {
                _type = SoundType.NORMAL;
            }
        }
    }

    ///<summary>Sets the delay of how often this sound can be played in milliseconds</summary>
    public void SetPlayTimeDelayInMS(uint milliseconds)
    {
        _delay = milliseconds;
    }

    public void Play()
    {
        if (_delay > 0f)
        {
            if (OS.GetTicksMsec() < _nextAllowedPlayTime)
            {
                return;
            }
            _nextAllowedPlayTime = OS.GetTicksMsec() + _delay;
        }
        switch (_type)
        {
            case SoundType.NORMAL:
                _sound.Play();
                break;
            case SoundType.RANDOM_SOUND_CSHARP:
                _soundRnd.PlayRandomSound();
                break;
            case SoundType.RANDOM_SOUND_GD:
                _node.Call(GD_RANDOM_SOUND_FUNCTION_NAME);
                break;
        }
    }

    public void Stop()
    {
        switch (_type)
        {
            case SoundType.NORMAL:
            case SoundType.RANDOM_SOUND_GD:
                _sound.Stop();
                break;
            case SoundType.RANDOM_SOUND_CSHARP:
                _soundRnd.Stop();
                break;
        }
    }
}
