extends Control

var labelScene = preload("res://Scenes/LoadedLabel.tscn")

func _enter_tree():
	# Connect once we enter the tree
# warning-ignore:return_value_discarded
	LoadManager.connect("trigger_load_step", self, "on_load_step")


# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


func on_load_step(step: int):
	if (step == LoadManager.LOAD_STEPS.PreLoad):
		print("- Spawning in label with call_deferred")
		var scene = labelScene.instance()
		call_deferred("add_child", scene)
	elif (step == LoadManager.LOAD_STEPS.FinalLoadStep):
		# Disconnect after loading is done
		print("- Disconnecting from signal to prevent double load (InterfaceRoot)")
		LoadManager.disconnect("trigger_load_step", self, "on_load_step")
