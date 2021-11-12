extends Sprite
class_name MovingIcon

#constants
const speed : float = 200.0;

# local variables
var target : Node2D = null;
var owning_spawner : Spawner = null;

# Called when the node enters the scene tree for the first time.
func _ready():
	# Get first goal that exists in tree
	target = get_tree().get_nodes_in_group(Constants.GROUP_GOAL)[0];
	
#
# Called by spawner to set which spawner we belong to
#
func set_spawner(spawner : Spawner):
	owning_spawner = spawner;

#
# Move if we got a target
#
func _physics_process(delta):
	if (target != null):
		var direction = (target.global_position - global_position).normalized();
		global_position += delta * direction * speed;

#
# Check if we entered the target area
#
func _on_Area2D_area_entered(area):
	if (area.get_parent().is_in_group(Constants.GROUP_GOAL)):
		PlayerData.decrease_lives();
		despawn();

#
# Called when we are told to despawn ourselves
#
func despawn():
	owning_spawner.notify_despawn(self);
	queue_free();

#
# Called when we are clicked
#
func _on_Area2D_input_event(viewport, event, shape_idx):
	if (event is InputEventMouseButton):
		if (event.button_index == BUTTON_LEFT):
			PlayerData.increase_score();
			despawn();
