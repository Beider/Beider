using Godot;
using System;

public class MapShaderDataProvider
{
    public delegate void EventVisibleSegmentsChanged(Rect2 segmentArea);
    public event EventVisibleSegmentsChanged OnVisibleSegmentsChanged = delegate { };

    public delegate void EventChunkInactive(Vector2 segment, MapShaderChunk chunk);
    public event EventChunkInactive OnChunkInactive = delegate { };

    public void NotifyVisibleSegmentsChanged(Rect2 segmentArea)
    {
        OnVisibleSegmentsChanged(segmentArea);
    }

    public void SetInactive(Vector2 segment, MapShaderChunk chunk)
    {
        OnChunkInactive(segment, chunk);
    }

    public int GetTile(int x, int y)
    {
        // Very simple noise gen
        float noise = GameManager.Instance.MapNoise.Noise.GetNoise2d(x, y);
        if (noise < 0.01f)
        {
            return 0;
        }
        else if (noise < 0.2f)
        {
            return 1;
        }
        else if (noise < 0.4f)
        {
            return 2;
        }
        else
        {
            return 3;
        }
    }
}
