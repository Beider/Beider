extends Sprite
class_name MovingIcon

# signals
signal about_to_despawn;

#constants
const SIGNAL_ABOUT_TO_DESPAWN : String = "about_to_despawn";
const speed : float = 200.0;

# local variables
var target : Node2D = null;

# Called when the node enters the scene tree for the first time.
func _ready():
	# Get first goal that exists in tree
	target = get_tree().get_nodes_in_group(Constants.GROUP_GOAL)[0];
	PlayerData.connect(PlayerData.SIGNAL_GAME_OVER, self, "on_game_over");

func _exit_tree():
	PlayerData.disconnect(PlayerData.SIGNAL_GAME_OVER, self, "on_game_over");


func on_game_over():
	queue_free();

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
	emit_signal(SIGNAL_ABOUT_TO_DESPAWN);
	queue_free();

#
# Called when we are clicked
#
func _on_Area2D_input_event(viewport, event, shape_idx):
	if (event is InputEventMouseButton):
		if (event.button_index == BUTTON_LEFT):
			PlayerData.increase_score();
			despawn();
