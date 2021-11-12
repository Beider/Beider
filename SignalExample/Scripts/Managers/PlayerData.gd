extends Node

# signals
signal game_over;
signal game_start;
signal lives_changed(new_value);
signal score_changed(new_value);

# constants
const SIGNAL_GAME_OVER : String = "game_over";
const SIGNAL_GAME_START : String = "game_start";
const SIGNAL_LIVES_CHANGED : String = "lives_changed";
const SIGNAL_SCORE_CHANGED : String = "score_changed";

# local variables
var score : int = 0;
var lives : int = 5;
var game_in_progress : bool = false;


#
# increase our score
#
func increase_score():
	score += 1;
	emit_signal(SIGNAL_SCORE_CHANGED, score);

#
# decrease our lives
#
func decrease_lives():
	lives -= 1;
	emit_signal(SIGNAL_LIVES_CHANGED, lives);
	check_game_over();

func start_game():
	#reset the game
	lives = 5;
	score = 0;
	game_in_progress = true;
	emit_signal(SIGNAL_GAME_START);

#
# check if we lost the game
#
func check_game_over():
	if (lives <= 0):
		game_in_progress = false;
		emit_signal(SIGNAL_GAME_OVER);
