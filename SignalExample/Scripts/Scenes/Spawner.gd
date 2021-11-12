extends Node2D
class_name Spawner

# exports
export var max_icons : int = 10;

# constants
const spawn_delay : float = 0.5;
const spawn_distance_max_y : float = 300.0;

# local variables
var moving_icon_scene : PackedScene;
var spawn_cooldown : float = 0.0;
var current_icons : int = 0;
var is_spawning : bool = false;


# Called when the node enters the scene tree for the first time.
func _ready():
	moving_icon_scene = ResourceLoader.load(Constants.SCENE_MOVING_ICON);
	PlayerData.connect(PlayerData.SIGNAL_GAME_OVER, self, "on_game_over");
	PlayerData.connect(PlayerData.SIGNAL_GAME_START, self, "on_game_start");

func _exit_tree():
	PlayerData.disconnect(PlayerData.SIGNAL_GAME_OVER, self, "on_game_over");
	PlayerData.disconnect(PlayerData.SIGNAL_GAME_START, self, "on_game_start");


func on_game_over():
	is_spawning = false;

func on_game_start():
	current_icons = 0;
	is_spawning = true;

#
# spawns icons at regular intervals
#
func _physics_process(delta):
	if (current_icons >= max_icons || !is_spawning):
		return;
		
	spawn_cooldown -= delta;
	if (spawn_cooldown <= 0.0):
		spawn_cooldown += spawn_delay;
		spawn_icon();

#
# spawns a single icon and increase counter
#
func spawn_icon():
	var position : Vector2 = global_position;
	position +=  + Vector2(0.0, rand_range(-spawn_distance_max_y, spawn_distance_max_y));
	var icon = moving_icon_scene.instance();
	get_parent().add_child(icon);
	icon.global_position = position;
	icon.connect(MovingIcon.SIGNAL_ABOUT_TO_DESPAWN, self, "notify_despawn");
	current_icons += 1;

#
# notification sent by the moving icon when it is despawned
#
func notify_despawn():
	current_icons -= 1;
