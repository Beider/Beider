extends Control




# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	$Score.text = "Score: " + str(PlayerData.score);
	$Score.text += "\nLives: " + str(PlayerData.lives);
	$BtnToggleSpawning.visible = !PlayerData.game_in_progress;


func _on_BtnToggleSpawning_pressed():
	PlayerData.game_in_progress = true;
	for node in get_tree().get_nodes_in_group(Constants.GROUP_SPAWNERS):
		node.is_spawning = true;
