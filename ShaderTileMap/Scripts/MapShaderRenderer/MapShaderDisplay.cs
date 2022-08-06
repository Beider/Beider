using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Takes care of creating MapShaderRenderer to render each segment that needs to be rendered
/// </summary>
public class MapShaderDisplay : Node2D
{
	// For debugging only, if you got more than one display this will not work
	public static MapShaderDisplay Instance;

	public const string SCENE_SHADER_CHUNK = "res://Scenes/Map/MapShaderChunk.tscn";

	// The size of each chunk, they are square by default
	// Should be a power of 2
	public const int WORLD_SEGMENT_SIZE = 128;

	// How many extra cunks to load
	private const int SEGMENTS_OUTSIDE_VIEW_TO_LOAD = 2;

	// How often should we update
	public const float UPDATE_INTERVAL = 0.3f;

	// Used for the queue, set higher than update interval to trigger immediate update
	private float _updateCounter = UPDATE_INTERVAL + 1f;

	// Active map segments
	private Dictionary<Vector2, MapShaderChunk> ActiveMapSegments = new Dictionary<Vector2, MapShaderChunk>();

	private List<MapShaderChunk> InactiveChunks = new List<MapShaderChunk>();

	private MapShaderDataProvider DataProvider;

	private int SegmentCounter = 0;

	private Node2D InactiveParent;

	private Rect2 LastSegmentArea = new Rect2();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;
		DataProvider = new MapShaderDataProvider();
		DataProvider.OnChunkInactive += OnChunkInactive;

		InactiveParent = new Node2D();
		InactiveParent.Name = "InactiveSegments";
		AddChild(InactiveParent);
	}

	public override void _PhysicsProcess(float delta)
	{
		// Add anything that needs to be rendered to queue
		_updateCounter += delta;
		if (_updateCounter > UPDATE_INTERVAL)
		{
			_updateCounter = 0f;
			UpdateShaderMap();
		}
	}

	/// <summary>
	/// Check if we need to update the map
	/// </summary>
	private void UpdateShaderMap()
	{
		// Get segments to process
		Rect2 segmentArea = GetSegmentArea();

		// Go through from top left to bottom right
		for (int x = (int)segmentArea.Position.x; x < (int)segmentArea.Size.x; x++)
		{
			for (int y = (int)segmentArea.Position.y; y < (int)segmentArea.Size.y; y++)
			{
				DrawSegment(new Vector2(x, y));
			}
		}

		// Tell all existing segments that we changed area
		if (LastSegmentArea != segmentArea)
		{
			LastSegmentArea = segmentArea;
			DataProvider.NotifyVisibleSegmentsChanged(segmentArea);
		}
	}

	/// <summary>
	/// Trigger the rendering of some segment
	/// </summary>
	private void DrawSegment(Vector2 segment)
	{
		// Check if it is already drawn, if so ignore
		if (ActiveMapSegments.ContainsKey(segment))
		{
			return;
		}

		// Look for a free map renderer or create one is none found
		MapShaderChunk chunk = null;
		if (InactiveChunks.Count > 0)
		{
			chunk = InactiveChunks[0];
			InactiveChunks.Remove(chunk);
			InactiveParent.RemoveChild(chunk);
			AddChild(chunk);
		}
		else
		{
			PackedScene scene = ResourceLoader.Load(SCENE_SHADER_CHUNK) as PackedScene;
			chunk = scene.Instance() as MapShaderChunk;
			SegmentCounter++;
			chunk.Name = $"Segment{SegmentCounter}";
			CallDeferred("add_child", chunk);
		}

		chunk.SetActive(DataProvider, segment);
		ActiveMapSegments.Add(segment, chunk);
	}

	private void OnChunkInactive(Vector2 segment, MapShaderChunk chunk)
	{
		ActiveMapSegments.Remove(segment);
		InactiveChunks.Add(chunk);
		RemoveChild(chunk);
		InactiveParent.AddChild(chunk);
	}


	/// <summary>
	/// Get area we are supposed to draw
	/// </summary>
	private Rect2 GetSegmentArea()
	{
		// Get camera positon
		Rect2 pos = PlayerCamera.GetCurrentViewAreaTilesAsRect();

		// Convert to top left / bottom right segment
		Vector2 topLeftSegment = new Vector2((Mathf.Floor(pos.Position.x / WORLD_SEGMENT_SIZE)) - SEGMENTS_OUTSIDE_VIEW_TO_LOAD,
								 (Mathf.Floor(pos.Position.y / WORLD_SEGMENT_SIZE)) - SEGMENTS_OUTSIDE_VIEW_TO_LOAD);

		Vector2 bottomRightSegment = new Vector2((Mathf.Floor((pos.Position.x + pos.Size.x) / WORLD_SEGMENT_SIZE) + SEGMENTS_OUTSIDE_VIEW_TO_LOAD),
											 (Mathf.Floor((pos.Position.y + pos.Size.y) / WORLD_SEGMENT_SIZE)) + SEGMENTS_OUTSIDE_VIEW_TO_LOAD);

		return new Rect2(topLeftSegment, bottomRightSegment);
	}

	#region Debugging methods

	/// <summary>
	/// Mainly for debugging
	/// </summary>
	public void SetShaderParameter(string name, object value)
	{
		foreach (var item in ActiveMapSegments.Values)
		{
			item.SetShaderParam(name, value);
		}
		InactiveChunks.ForEach(c => c.SetShaderParam(name, value));
	}



	#endregion
}
