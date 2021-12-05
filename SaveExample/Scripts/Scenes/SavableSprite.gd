extends Sprite
class_name SavableSprite

const SIGNAL_NODE_REMOVED = "node_removed"

# warning-ignore:unused_signal
signal node_removed(node)

var g_position = Vector2.ZERO

#
# When we are clicked check for right click and remove
#
func _on_Area2D_input_event(_viewport, event, _shape_idx):
	if event is InputEventMouseButton:
		if (event.pressed and event.button_index == BUTTON_RIGHT):
			remove_icon()

func set_g_postion(position):
	g_position = position
	global_position = position

#
# Emit the removed signal then remove self
#
func remove_icon():
	emit_signal(SIGNAL_NODE_REMOVED, self)
	queue_free()
