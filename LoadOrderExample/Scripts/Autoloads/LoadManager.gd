extends Node

signal trigger_load_step(step)

enum LOAD_STEPS{
	PreLoad,
	WaitFrame01,
	SecondLoadStep,
	WaitFrame02,
	FinalLoadStep,
	WaitFrame03,
	StartGame
}

func _ready():
	pass

# Enter tree is done before ready, so just to be on the safe side
func _enter_tree():
	# Connect so we can auto trigger next loading step
# warning-ignore:return_value_discarded
	connect("trigger_load_step", self, "trigger_next_step")

#
# Call to start loading
#
func start_loading():
	call_deferred("trigger_load_step", 0)

#
# Triggers the individual loading step
#
func trigger_load_step(step: int):
	# You can comment this out if you don't want the logging
	print("Trigger Loading Step: " + step_index_to_value(step))
	emit_signal("trigger_load_step", step)

#
# Trigger the next loading step
#
func trigger_next_step(step: int):
	if (step < LOAD_STEPS.values().size()-1):
		call_deferred("trigger_load_step", step+1)


#
# Convenience function to convert a load step value to int
#
func step_value_to_key(value) -> int:
	return LOAD_STEPS.values()[value];

#
# Convenience function to parse load step index to value
#
func step_index_to_value(index : int):
	return LOAD_STEPS.keys()[index]
