using Godot;
using System;

public class MapShaderChunk : Node2D
{
	public const String SHADER_PARAM_TEXTURE_ATLAS = "textureAtlas";
	public const String SHADER_PARAM_BLEND_TEXTURE = "blendTexture";
	public const String SHADER_PARAM_MAP_DATA = "mapData";
	public const String SHADER_PARAM_MAP_TILES_COUNT_X = "mapTilesCountX";
	public const String SHADER_PARAM_MAP_TILES_COUNT_Y = "mapTilesCountY";
	public const String SHADER_PARAM_TILE_SIZE_PIXELS = "tileSizeInPixels";
	public const String SHADER_PARAM_HALF_TILE_SIZE_PIXELS = "halfTileSizeInPixels";

	private Vector2 Segment = Vector2.Inf;
	private MapShaderDataProvider DataProvider;
	private Sprite MapRenderer = null;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MapRenderer = FindNode("MapRenderer") as Sprite;

		// Setup shader
		ShaderMaterial mat = (ShaderMaterial)MapRenderer.Material;
		mat.SetShaderParam(SHADER_PARAM_TEXTURE_ATLAS, GameManager.Instance.MegaTexture);
		mat.SetShaderParam(SHADER_PARAM_BLEND_TEXTURE, GameManager.Instance.TileBlendTexture);
		mat.SetShaderParam(SHADER_PARAM_MAP_TILES_COUNT_X, MapShaderDisplay.WORLD_SEGMENT_SIZE);
		mat.SetShaderParam(SHADER_PARAM_MAP_TILES_COUNT_Y, MapShaderDisplay.WORLD_SEGMENT_SIZE);
		mat.SetShaderParam(SHADER_PARAM_TILE_SIZE_PIXELS, GameManager.TILE_SIZE);
		mat.SetShaderParam(SHADER_PARAM_HALF_TILE_SIZE_PIXELS, GameManager.TILE_SIZE / 2f);

		// Important is that the base picture for this image is the same as our tile size
		// If not you have to adjust the scale calculation accordingly
		MapRenderer.Scale = new Vector2(MapShaderDisplay.WORLD_SEGMENT_SIZE, MapShaderDisplay.WORLD_SEGMENT_SIZE);
	}

	public void SetShaderParam(string name, object value)
	{
		ShaderMaterial mat = (ShaderMaterial)MapRenderer.Material;
		mat.SetShaderParam(name, value);
	}

	public void SetActive(MapShaderDataProvider provider, Vector2 segment)
	{
		this.Segment = segment;
		this.DataProvider = provider;
		DataProvider.OnVisibleSegmentsChanged += OnVisibleSegmentsChanged;
		CallDeferred(nameof(PerformInitialRender));
	}

	private void OnVisibleSegmentsChanged(Rect2 segmentArea)
	{
		if (Segment == Vector2.Inf || MapRenderer == null)
		{
			return;
		}

		// We do our own check since the Size of segmentArea is actually the bottom right corner not the size
		if (segmentArea.Position.x > Segment.x || segmentArea.Size.x < Segment.x ||
			segmentArea.Position.y > Segment.y || segmentArea.Size.y < Segment.y)
		{
			//GD.Print($"SegmentArea: {segmentArea} does not contain segment: {Segment}");
			// Time to go inactive
			DataProvider.OnVisibleSegmentsChanged -= OnVisibleSegmentsChanged;
			DataProvider.SetInactive(Segment, this);
			DataProvider = null;
			MapRenderer.Visible = false;
		}
	}

	/// <summary>
	/// Does the initial rendering of this segment
	/// </summary>
	private void PerformInitialRender()
	{
		Rect2 area = GetRectFromSegment(Segment);

		// Position ourselves, area has -1 / +1 on it's size
		GlobalPosition = new Vector2((area.Position.x + 1) * GameManager.TILE_SIZE,
								 (area.Position.y + 1) * GameManager.TILE_SIZE);

		//Vector2 topLeft = new Vector2(area.Position.x * GameWorldManager.TILE_SIZE, area.Position.y * GameWorldManager.TILE_SIZE);
		// TODO: Calculate start offset

		GenerateMapTexture(area);
	}

	private void GenerateMapTexture(Rect2 area)
	{
		// Setup dimensions
		Vector2 start = area.Position;
		Vector2 size = area.Size - area.Position;

		byte[] dataArray = new byte[(int)(size.x * size.y)];

		// Create image
		Image img = new Image();

		// Draw image
		int index = 0;
		for (int y = 0; y < size.y; y++)
		{
			for (int x = 0; x < size.x; x++)
			{
				var cell = DataProvider.GetTile((int)start.x + x, (int)start.y + y);
				dataArray[index] = (byte)cell;
				index++;
			}
		}
		img.CreateFromData((int)size.x, (int)size.y, false, Image.Format.R8, dataArray);
		ImageTexture texture = new ImageTexture();
		texture.CreateFromImage(img);

		// Set to shader
		((ShaderMaterial)MapRenderer.Material).SetShaderParam(SHADER_PARAM_MAP_DATA, texture);
		MapRenderer.Visible = true;
	}

	private Rect2 GetRectFromSegment(Vector2 segment)
	{
		// Render 1 extra cell in each direction so shading gets ok
		Vector2 topLeft = new Vector2((segment.x * MapShaderDisplay.WORLD_SEGMENT_SIZE) - 1, (segment.y * MapShaderDisplay.WORLD_SEGMENT_SIZE) - 1);
		Vector2 bottomRight = new Vector2((segment.x * MapShaderDisplay.WORLD_SEGMENT_SIZE) + MapShaderDisplay.WORLD_SEGMENT_SIZE + 1,
										  (segment.y * MapShaderDisplay.WORLD_SEGMENT_SIZE) + MapShaderDisplay.WORLD_SEGMENT_SIZE + 1);

		return new Rect2(topLeft, bottomRight);
	}
}
