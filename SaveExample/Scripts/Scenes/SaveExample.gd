extends Node2D

export var IconScene : PackedScene = null
export var SaveName : String = "SaveExample"

var icons : Array = []

var settings

# Called when the node enters the scene tree for the first time.
func _ready():
	SaveManager.add_savable_object(self)
	settings = SaveManager.get_data_storage(SettingsStorage.STORAGE_NAME)
	print("Settings value: " + settings.get_value())
	load_data()

#
# Load the data if it exists
#
func load_data():
	var data = SaveManager.get_data(SaveName)
	if (data == null):
		return
	
	for key in data:
		if (key == SaveManager.SAVE_DATA_NAME):
			continue
		
		add_icon(str2var(data[key]))

#
# Check for left click and add icon
#
func _input(event):
	if event is InputEventMouseButton:
		if (event.pressed and event.button_index == BUTTON_LEFT):
			add_icon(get_global_mouse_position())

#
# Add icon at position
#
func add_icon(position):
	var instance : SavableSprite = IconScene.instance()
	# warning-ignore:return_value_discarded
	instance.connect(SavableSprite.SIGNAL_NODE_REMOVED, self, "node_removed")
	add_child(instance)
	icons.push_back(instance)
	instance.global_position = position

#
# Called form icon when it is about to be removed
#
func node_removed(node):
	var index = icons.find(node)
	if (index >= 0):
		icons.remove(index)


#
# SAVE METHODS
#
func get_save_name():
	return SaveName

func get_save_data():
	var dict : Dictionary = {}
	var i = 0
	for icon in icons:
		dict["icon" + str(i)] = var2str(icon.global_position)
		i += 1
	return dict
