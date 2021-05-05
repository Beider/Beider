using Godot;
using System;

[DebugVisibilityGroups("Moving Icon", "Other Group")]
public class MovingIcon : KinematicBody2D
{
	[Export]
	[OnScreenDebug("Moving Icons", "%node_name% Speed", nameof(Colors.Ivory))]
	public float Speed = 200f;

	[Export]
	public Vector2 Direction = Vector2.Left;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	public override void _PhysicsProcess(float delta)
	{
		Position += Direction * Speed * delta;
	}

	private void OnScreenExited()
	{
		QueueFree();
	}

}


