extends Node2D
class_name SettingsStorage

const STORAGE_NAME : String = "SettingsStorage"

var value : String = ""

func _ready():
	var data = SaveManager.get_data(STORAGE_NAME)
	if (data == null):
		value = "not set"
		return
	value = data["ExampleValue"]

func get_value():
	return value
#
# SAVE METHODS
#
func get_save_name():
	return STORAGE_NAME

func get_save_data():
	var dict : Dictionary = {}
	dict["ExampleValue"] = "Some value"
	return dict
