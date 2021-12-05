extends Node

#
# Constants
#
const METHOD_GET_SAVE_DATA = "get_save_data"
const METHOD_GET_SAVE_NAME = "get_save_name"

const SAVE_DATA_NAME = "save_data"

# Windows: %APPDATA%/Godot/app_userdata/<game name>/
const SAVE_FILE_NAME = "user://SaveGame.json"

const DATA_STORAGE_PATH = "res://Scenes/DataStorages/"

const SIGNAL_ON_GAME_SAVED = "on_game_saved"

#
# Signals
#

signal on_game_saved

#
# Variables
#
# key = get_save_name, data = get_save_data
var loaded_data : Dictionary = {}
var data_storages : Dictionary = {}
var savable_objects : Array = []
var autosave_timer : Timer = null

var settings


# Called when the node enters the scene tree for the first time.
func _ready():
	get_tree().set_auto_accept_quit(false)
	load_from_file()
	init_data_storages()
	settings = get_data_storage(SettingsStorage.STORAGE_NAME)
	init_autosave()
	get_tree().connect("node_removed", self, "on_node_removed")

#
# When a node is removed check if it is a savable object
# if it is remove it from the list of tracked nodes and update the data
#
func on_node_removed(node):
	if (savable_objects.has(node)):
		savable_objects.remove(savable_objects.find(node))
		update_loaded_data(node)
		print("Auto updated node on removal - " + node.name)

#
# Save on game exit
#
func _notification(what):
	if (what == MainLoop.NOTIFICATION_WM_QUIT_REQUEST ||
		what == MainLoop.NOTIFICATION_WM_GO_BACK_REQUEST):
			save()
			get_tree().quit()

#
# Create autosave timer
#
func init_autosave():
	if (settings.AUTOSAVE_ENABLED):
		autosave_timer = Timer.new()
		autosave_timer.wait_time = settings.AUTOSAVE_INTERVAL
		autosave_timer.connect("timeout", self, "save")
		add_child(autosave_timer)
		autosave_timer.start()

#
# Get data storage
#
func get_data_storage(key : String):
	if (data_storages.has(key)):
		return data_storages[key]
	return null


#
# Create data storages from all scenes in the DATA_STORAGE_PATH folder
#
func init_data_storages():
	var dir = Directory.new()
	if (dir.open(DATA_STORAGE_PATH) == OK):
		dir.list_dir_begin(true)
		var file_name = dir.get_next()
		while (file_name != ""):
			var scene = ResourceLoader.load(DATA_STORAGE_PATH + file_name)
			var node = scene.instance()
			if (add_savable_object(node)):
				add_child(node)
				data_storages[node.get_save_name()] = node
				print("Added data storage: " + node.name)
			else:
				node.queue_free()
				print("Node is not a data storage: " + node.name)
			file_name = dir.get_next()
		dir.list_dir_end()

#
# Add savable object
#
func add_savable_object(object : Node):
	if (object.has_method(METHOD_GET_SAVE_DATA) &&
		object.has_method(METHOD_GET_SAVE_NAME)):
			savable_objects.push_back(object)
			return true
	return false

#
# Get data
#
func get_data(key : String):
	if (loaded_data.has(key)):
		return loaded_data[key]
	return null

#
# Load method
#

func load_from_file():
	var file = File.new()
	if (!file.file_exists(SAVE_FILE_NAME)):
		return
	
	loaded_data.clear()
	file.open(SAVE_FILE_NAME, File.READ)
	while (!file.eof_reached()):
		var line = file.get_line()
		if (line == null || line.length() < 2):
			continue
		var jr = JSON.parse(line)
		if (jr.error == OK):
			var data = jr.result
			loaded_data[data[SAVE_DATA_NAME]] = data
	file.close()
	print ("data loaded")

#
# Save method
#
func save():
	print("save")
	for node in savable_objects:
		update_loaded_data(node)
	save_to_file()

func update_loaded_data(node):
	loaded_data[node.get_save_name()] = node.get_save_data()
	loaded_data[node.get_save_name()][SAVE_DATA_NAME] = node.get_save_name()

#
# Save data to file as jason
#
func save_to_file():
	var file = File.new()
	file.open(SAVE_FILE_NAME, File.WRITE)
	for key in loaded_data:
		file.store_line(JSON.print(loaded_data[key]))
	file.close()
	emit_signal(SIGNAL_ON_GAME_SAVED)
