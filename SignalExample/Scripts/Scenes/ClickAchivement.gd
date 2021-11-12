extends Node2D


export var required_score : int = 0;

var current_score : int = 0;

# Called when the node enters the scene tree for the first time.
func _ready():
	if (required_score == 0):
		queue_free();
	PlayerData.connect(PlayerData.SIGNAL_SCORE_CHANGED, self, "on_score_changed");
	PlayerData.connect(PlayerData.SIGNAL_GAME_OVER, self, "on_game_over");

func _exit_tree():
	PlayerData.disconnect(PlayerData.SIGNAL_SCORE_CHANGED, self, "on_score_changed");
	PlayerData.disconnect(PlayerData.SIGNAL_GAME_OVER, self, "on_game_over");
	
func on_game_over():
	current_score = 0;

func on_score_changed(value):
	current_score += 1;
	if (current_score >= required_score):
		grant_achivement();
		
func grant_achivement():
	print("Got achivement! Click " + str(required_score) + " icons!");
	queue_free();
