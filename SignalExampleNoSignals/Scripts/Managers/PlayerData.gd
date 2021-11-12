extends Node

# local variables
var score : int = 0;
var lives : int = 5;
var game_in_progress : bool = false;

var achivement_click_5 : bool = false;
var achivement_click_10 : bool = false;


#
# increase our score
#
func increase_score():
	score += 1;
	check_achivements();

#
# decrease our lives
#
func decrease_lives():
	lives -= 1;
	check_game_over();

#
# check if we lost the game
#
func check_game_over():
	if (lives <= 0):
		#reset the game
		lives = 5;
		score = 0;
		game_in_progress = false;
		
		# stop spawners
		for node in get_tree().get_nodes_in_group(Constants.GROUP_SPAWNERS):
			node.is_spawning = false;
		
		# clear up icons
		for icon in get_tree().get_nodes_in_group(Constants.GROUP_MOVING_ICONS):
			icon.despawn();

#
# check for achivements
#
func check_achivements():
	if (score >= 5 && !achivement_click_5):
		achivement_click_5 = true;
		print("Got achivement! Click 5 icons!");
	if (score >= 10 && !achivement_click_10):
		achivement_click_10 = true;
		print("Got achivement! Click 10 icons!");
