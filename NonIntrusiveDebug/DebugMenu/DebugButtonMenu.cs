using Godot;
using System;
using System.Collections.Generic;

namespace DebugMenu
{
	public delegate void DebugMenuAction();

	public class DebugButtonMenu : Control
	{
		private static DebugButtonMenu Instance;

		private Control VisibilityGridParent;

		private Control ActionGridParent;

		private List<DebugMenuAction> ActionList = new List<DebugMenuAction>();

		private Dictionary<Node, List<Button>> ButtonRegistry = new Dictionary<Node, List<Button>>();

		private Dictionary<string, List<CanvasItem>> VisibilityGroupMembers = new Dictionary<string, List<CanvasItem>>();

		private Dictionary<string, bool> VisibilityGroupVisible = new Dictionary<string, bool>();

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			
		}

		public override void _EnterTree()
		{
			Instance = this;
			VisibilityGridParent = FindNode("VisibilityGroupGrid") as Control;
			ActionGridParent = FindNode("ActionGrid") as Control;
			
			VisibilityGridParent.ClearNodeChildren();
			ActionGridParent.ClearNodeChildren();
		}

		public static void AddVisibilityGroupMember(string groupName, CanvasItem node)
		{
			if (!Instance.VisibilityGroupVisible.ContainsKey(groupName))
			{
				Instance.VisibilityGroupVisible.Add(groupName, true);
				Instance.VisibilityGroupMembers.Add(groupName, new List<CanvasItem>());
				Button btn = AddButton(groupName, Instance.VisibilityGridParent, () => ToggleGroup(groupName));
				btn.Modulate = Colors.Green;
			}
			Instance.VisibilityGroupMembers[groupName].Add(node);
		}

		public static bool IsGroupVisible(string groupName)
		{
			if (Instance.VisibilityGroupMembers.ContainsKey(groupName))
			{
				return Instance.VisibilityGroupVisible[groupName];
			}
			return true;
		}

		public static void ToggleGroup(string groupName)
		{
			if (Instance.VisibilityGroupMembers.ContainsKey(groupName))
			{
				ToggleGroup(groupName, !Instance.VisibilityGroupVisible[groupName]);
			}
		}

		public static void ToggleGroup(string groupName, bool visible)
		{
			if (Instance.VisibilityGroupMembers.ContainsKey(groupName))
			{
				Instance.VisibilityGroupVisible[groupName] = visible;
				foreach(CanvasItem node in Instance.VisibilityGroupMembers[groupName])
				{
					node.Visible = visible;
				}

				// Toggle button color
				Button btn = FindButtonInGroupByName(groupName, Instance.VisibilityGridParent);
				if (btn != null)
				{
					btn.Modulate =  visible ? Colors.Green : Colors.Red;
				}
			}
		}

		private static Button FindButtonInGroupByName(string name, Node group)
		{
			foreach (Node child in group.GetChildren())
			{
				Button btn = (Button)child;
				if (btn.Text.Equals(name))
				{
					return btn;
				}
			}
			return null;
		}

		public static void RemoveVisibilityGroupMember(string groupName, CanvasItem node)
		{
			if (Instance.VisibilityGroupMembers.ContainsKey(groupName))
			{
				Instance.VisibilityGroupMembers[groupName].Remove(node);
			}
		}

		public static void AddActionButton(string text, Node node, DebugMenuAction action)
		{
			Button btn = AddButton(text, Instance.ActionGridParent, action);
			Instance.RegisterActionButton(node, btn);
		}

		public static void RemoveButtonsForNode(Node node)
		{
			Instance.ClearAllActionButtons(node);
		}

		private static Button AddButton(string text, Control parent, DebugMenuAction action)
		{
			Button btn = new Button();
			btn.Text = text;
			btn.SizeFlagsHorizontal = (int)SizeFlags.Expand + (int)SizeFlags.Fill;
			Instance.ActionList.Add(action);
			Godot.Collections.Array parameters = new Godot.Collections.Array();
			parameters.Add(Instance.ActionList.Count -1);
			btn.Connect("pressed", Instance, nameof(ButtonPressed), parameters);
			parent.AddChild(btn);
			return btn;
		}

		private void RegisterActionButton(Node node, Button button)
		{
			if (!ButtonRegistry.ContainsKey(node))
			{
				ButtonRegistry.Add(node, new List<Button>());
			}
			ButtonRegistry[node].Add(button);
		}

		private void ClearAllActionButtons(Node node)
		{
			if (!ButtonRegistry.ContainsKey(node))
			{
				return;
			}

			ButtonRegistry[node].ForEach( btn => btn.QueueFree());
			ButtonRegistry.Remove(node);
		}

		private void ButtonPressed(int index)
		{
			ActionList[index].Invoke();
		}

	}
}
