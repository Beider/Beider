using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DebugMenu
{
	public class DebugMenu : Node2D
	{
		public static readonly string VISIBILITY_GROUP_NAME = "vg_";
		private static DebugMenu Instance;
		private static int SequenceNumber = 0;

		[Export]
		public Font OnScreenDebugFont;

		[Export]
		public Theme DebugMenuTheme;

		private OnScreenDebugInterface OnScreenDebugControl;
		private DebugButtonMenu DebugButtonMenu;

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			// Do not show unless we are in debug mode
			if (!OS.IsDebugBuild())
			{
				QueueFree();
				return;
			}

			OnScreenDebugControl = FindNode("OnScreenDebug") as OnScreenDebugInterface;
			DebugButtonMenu = FindNode("DebugButtonMenu") as DebugButtonMenu;
			DebugButtonMenu.Visible = false;
			if (GetTheme() != null)
			{
				DebugButtonMenu.Theme = GetTheme();
			}

			// Add basic stuff to OnScreenDebug (FPS)
			OnScreenDebugManager.Initialize();
		}

		public override void _EnterTree()
		{
			// This is done in _EnterTree so that we get the signal of other autoloads being added to the tree
			Instance = this;
			OnScreenDebug.Init();
			GetTree().Connect("node_added", this, nameof(OnNodeAdded));
			GetTree().Connect("node_removed", this, nameof(OnNodeRemoved));
		}

		public static Font GetFont()
		{
			return Instance.OnScreenDebugFont;
		}

		public static Theme GetTheme()
		{
			return Instance.DebugMenuTheme;
		}

		public override void _Input(InputEvent @event)
		{
			if (@event is InputEventKey)
			{
				InputEventKey eventKey = (InputEventKey)@event;
				bool just_pressed = @event.IsPressed() && !@event.IsEcho();
				if (!just_pressed)
				{
					return;
				}
				if (eventKey.Scancode == (int)KeyList.F11)
				{
					OnScreenDebugControl.Visible = !OnScreenDebugControl.Visible;
				}
				else if (eventKey.Scancode == (int)KeyList.F12)
				{
					DebugButtonMenu.Visible = !DebugButtonMenu.Visible;
				}
			}
		}

		private void OnNodeAdded(Node node)
		{
			Type type = node.GetType();
			if (DebugReflectionUtil.IsInGodotNamespace(type))
			{
				return;
			}

			ProcessMembers(node, true);
			ProcessMethods(node, true);
			if (node is CanvasItem)
			{
				ProcessClass((CanvasItem)node, true);
				ProcessGroups((CanvasItem)node, true);
			}
		}

		private void OnNodeRemoved(Node node)
		{
			Type type = node.GetType();
			if (DebugReflectionUtil.IsInGodotNamespace(type))
			{
				return;
			}

			ProcessMembers(node, false);
			ProcessMethods(node, false);
			if (node is CanvasItem)
			{
				ProcessClass((CanvasItem)node, false);
				ProcessGroups((CanvasItem)node, false);
			}
		}

		private void ProcessMembers(Node node, bool add)
		{
			IList<MemberInfo> members = DebugReflectionUtil.GetMemberInfos(node.GetType());
			foreach (MemberInfo info in members)
			{
				ProcessOnScreenDebugAttributeForMember(info, node, add);
			}
		}

		private void ProcessMethods(Node node, bool add)
		{
			IList<MethodInfo> methods = DebugReflectionUtil.GetMethodInfos(node.GetType());
			foreach (MethodInfo info in methods)
			{
				ProcessOnScreenDebugAttributeForMethod(info, node, add);
				ProcessActionAttributeForMethod(info, node, add);
			}
		}

		private void ProcessClass(CanvasItem node, bool add)
		{
			DebugVisibilityGroups attr = DebugReflectionUtil.FindClassAttributeInNode<DebugVisibilityGroups>(node.GetType());
			if (attr == null)
			{
				return;
			}

			foreach (string name in attr.Groups)
			{
				ProcessVisibilityGroupMember(node, name, add);
			}
		}

		/// <summary>
		/// Check if we are in a godot visibility group
		/// </summary>
		private void ProcessGroups(CanvasItem node, bool add)
		{
			foreach (string groupName in node.GetGroups())
			{
				if (groupName.StartsWith(VISIBILITY_GROUP_NAME))
				{
					string name = groupName.Replace(VISIBILITY_GROUP_NAME, "");
					ProcessVisibilityGroupMember(node, name, add);
				}
			}
		}

		private void ProcessVisibilityGroupMember(CanvasItem node, string name, bool add)
		{
			if (add)
			{
				DebugButtonMenu.AddVisibilityGroupMember(name, node);
				if (!DebugButtonMenu.IsGroupVisible(name))
				{
					node.Visible = false;
				}
			}
			else
			{
				DebugButtonMenu.RemoveVisibilityGroupMember(name, node);
			}
		}

		private void ProcessOnScreenDebugAttributeForMember(MemberInfo info, Node node, bool add)
		{
			OnScreenDebug attr = DebugReflectionUtil.GetCustomAttribute<OnScreenDebug>(info) as OnScreenDebug;
			if (attr != null)
			{
				if (add)
				{
					// Add
					OnScreenDebugManager.AddOnScreenDebugInfo(ReplaceName(attr.DebugCategory, node), 
						ReplaceName(attr.Name, node), () => GetValueOfMember(info, node), attr.Color);
				}
				else
				{
					// Remove
					OnScreenDebugManager.RemoveOnScreenDebugInfo(ReplaceName(attr.DebugCategory, node), 
						ReplaceName(attr.Name, node));
				}
			}
		}

		private string GetValueOfMember(MemberInfo info, Node node)
		{
			object value = info.GetValue(node);
			if (value == null)
			{
				return "null";
			}
			return value.ToString();
		}

		private void ProcessOnScreenDebugAttributeForMethod(MethodInfo info, Node node, bool add)
		{
			OnScreenDebug attr = DebugReflectionUtil.GetCustomAttribute<OnScreenDebug>(info) as OnScreenDebug;
			if (attr != null)
			{
				if (add)
				{
					// Add
					OnScreenDebugManager.AddOnScreenDebugInfo(ReplaceName(attr.DebugCategory, node), 
						ReplaceName(attr.Name, node), () => GetValueOfMethod(info, node), attr.Color);
				}
				else
				{
					// Remove
					OnScreenDebugManager.RemoveOnScreenDebugInfo(ReplaceName(attr.DebugCategory, node), 
						ReplaceName(attr.Name, node));
				}
			}
		}

		private string GetValueOfMethod(MethodInfo info, Node node)
		{
			object value = info.Invoke(node, null);
			if (value == null)
			{
				return "null";
			}
			return value.ToString();
		}

		private void ProcessActionAttributeForMethod(MethodInfo info, Node node, bool add)
		{
			if (!add)
			{
				DebugButtonMenu.RemoveButtonsForNode(node);
				return;
			}
			object[] attrs = DebugReflectionUtil.GetCustomAttributes<DebugAction>(info, true);
			if (attrs.Length == 0)
			{
				return;
			}
			foreach (object objAttr in attrs)
			{
				DebugAction attr = (DebugAction) objAttr;
				DebugButtonMenu.AddActionButton(ReplaceName(attr.Name, node), node, () => info.Invoke(node, attr.Parameters));
			}
		}

		private string ReplaceName(string name, Node node)
		{
			string newName = name;
			newName = newName.Replace("%node_name%", node.Name);
			if (newName.Contains("%seq%"))
			{
				newName = newName.Replace("%seq%", GetNextSequenceNumber());
			}
			return newName;
		}

		private string GetNextSequenceNumber()
		{
			SequenceNumber++;
			return SequenceNumber.ToString();
		}

	}
}
