tool
extends EditorPlugin

var plugin_button :MyToolPluginButton

# Create whole plugin
func _enter_tree():
	# Add button to 3D scene UI
	# Shows panel when toggled
	plugin_button = preload("res://addons/my_tools/plugin_button.tscn").instance()
	add_control_to_container(EditorPlugin.CONTAINER_SPATIAL_EDITOR_MENU, plugin_button)
	get_editor_interface().get_selection().connect("selection_changed", self, "selection_changed")
	
	#set_input_event_forwarding_always_enabled()


# Destroy whole plugin
func _exit_tree():
	remove_control_from_container(EditorPlugin.CONTAINER_SPATIAL_EDITOR_MENU, plugin_button)
	if plugin_button:
		plugin_button.free()

func selection_changed() -> void:
	print("selection_changed")
	plugin_button.selection = get_editor_interface().get_selection().get_selected_nodes()


func forward_spatial_gui_input(camera, event):
	if plugin_button.editor_camera != camera:
		plugin_button.editor_camera = camera
	return false
