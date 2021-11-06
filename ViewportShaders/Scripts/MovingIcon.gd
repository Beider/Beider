extends Sprite


# Declare member variables here. Examples:
var bound_x = 1024
var bound_y = 600
var direction = Vector2();
var speed = 200;


# Called when the node enters the scene tree for the first time.
func _ready():
	direction = Vector2(rand_range(-100,100),rand_range(-100,100)).normalized();
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _physics_process(delta):
	global_position += direction * delta * speed;
	if (global_position.x >= bound_x || global_position.x <= 0):
		direction.x = -direction.x
	if (global_position.y >= bound_y || global_position.y <= 0):
		direction.y = -direction.y
