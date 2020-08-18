extends AudioStreamPlayer2D


export(Array, AudioStream) var AudioList


# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


func PlayRandomSound():
	stream = AudioList[randi()%AudioList.size()]
	play()
	
