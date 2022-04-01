extends Label

func _enter_tree():
	# Connect once we enter the tree
# warning-ignore:return_value_discarded
	LoadManager.connect("trigger_load_step", self, "on_load_step")


# Called when the node enters the scene tree for the first time.
func _ready():
	print("- LoadedLabel Ready() called")


func on_load_step(step: int):
	if (step == LoadManager.LOAD_STEPS.SecondLoadStep):
		print("- Setting label text")
		text = "set during second load step"
	elif (step == LoadManager.LOAD_STEPS.FinalLoadStep):
		print("- Setting label position")
		rect_global_position = Vector2(100, 100)
		# Disconnect after loading is done
		print("- Disconnecting from signal to prevent double load (LoadedLabel)")
		LoadManager.disconnect("trigger_load_step", self, "on_load_step")
