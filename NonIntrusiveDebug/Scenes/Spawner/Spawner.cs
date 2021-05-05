using Godot;
using System;
using DebugMenu;

[DebugVisibilityGroups("Spawners")]
public class Spawner : Sprite
{
	private static readonly string PathToScene = "res://Scenes/MovingIcon/MovingIcon.tscn";

	private bool Spawning = true;

	private float Counter = 0f;

	private float SpawnDelay = 1f;

	private PackedScene SceneToSpawn;

	private Label NameLabel;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		SceneToSpawn = ResourceLoader.Load(PathToScene) as PackedScene;
		NameLabel = FindNode("NameLabel") as Label;
		NameLabel.Text = Name;
		DebugButtonMenu.AddVisibilityGroupMember("Spawner Names", NameLabel);
	}

	public override void _ExitTree()
	{
		DebugButtonMenu.RemoveVisibilityGroupMember("Spawner Names", NameLabel);
	}

	public override void _Process(float delta)
	{
		if (!Spawning)
		{
			return;
		}
		Counter += delta;
		if (Counter > SpawnDelay)
		{
			Counter -= SpawnDelay;
			Node2D scene = SceneToSpawn.Instance() as Node2D;
			scene.GlobalPosition = GlobalPosition;
			GetTree().Root.AddChild(scene);
		}
	}


	[DebugAction("%node_name% Toggle Spawning", null)]
	private void ToggleSpawning()
	{
		Spawning = !Spawning;
	}

	[DebugAction("%node_name% Increase Speed", -0.1f)]
	[DebugAction("%node_name% Decrease Speed", 0.1f)]
	private void DebugChangeSpeed(float value)
	{
		SpawnDelay -= value;
		if (SpawnDelay < value)
		{
			SpawnDelay = value;
		}
	}

	[OnScreenDebug("%node_name%", "SpawningMethod", nameof(Colors.Blue))]
	private string DebugIsSpawning()
	{
		if (Spawning)
		{
			return "[color=green]True[/color]";
		}
		else
		{
			return "[color=red]False[/color]";
		}
	}
}
