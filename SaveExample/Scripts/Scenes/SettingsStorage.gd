extends Node2D
class_name SettingsStorage

const STORAGE_NAME = "SettingStorage"

const STORAGE_KEY_AS_ENABLED = "AutosaveEnabled"
const STORAGE_KEY_AS_INTERVAL = "AutosaveInterval"

var AUTOSAVE_ENABLED = true
var AUTOSAVE_INTERVAL = 5

func _ready():
	var data = SaveManager.get_data(STORAGE_NAME)
	if (data == null):
		return
	
	AUTOSAVE_ENABLED = data[STORAGE_KEY_AS_ENABLED]
	AUTOSAVE_INTERVAL = data[STORAGE_KEY_AS_INTERVAL]

#
# SAVE METHODS
#
func get_save_name():
	return STORAGE_NAME

func get_save_data():
	var dict : Dictionary = {}
	dict[STORAGE_KEY_AS_ENABLED] = AUTOSAVE_ENABLED
	dict[STORAGE_KEY_AS_INTERVAL] = AUTOSAVE_INTERVAL
	return dict
