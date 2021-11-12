extends Control

# Called when the node enters the scene tree for the first time.
func _ready():
	PlayerData.connect(PlayerData.SIGNAL_GAME_OVER, self, "on_game_over");
	PlayerData.connect(PlayerData.SIGNAL_GAME_START, self, "on_game_start");
	PlayerData.connect(PlayerData.SIGNAL_LIVES_CHANGED, self, "on_stats_changed");
	PlayerData.connect(PlayerData.SIGNAL_SCORE_CHANGED, self, "on_stats_changed");

func _exit_tree():
	PlayerData.disconnect(PlayerData.SIGNAL_GAME_OVER, self, "on_game_over");
	PlayerData.diconnect(PlayerData.SIGNAL_GAME_START, self, "on_game_start");
	PlayerData.disconnect(PlayerData.SIGNAL_LIVES_CHANGED, self, "on_stats_changed");
	PlayerData.disconnect(PlayerData.SIGNAL_SCORE_CHANGED, self, "on_stats_changed");

func on_game_over():
	$BtnToggleSpawning.visible = true;
	
func on_game_start():
	on_stats_changed(1);


#
# update stats
#
func on_stats_changed(new_value):
	$Score.text = "Score: " + str(PlayerData.score);
	$Score.text += "\nLives: " + str(PlayerData.lives);

#
# toggle spawning button
#
func _on_BtnToggleSpawning_pressed():
	PlayerData.start_game();
	$BtnToggleSpawning.visible = false;
