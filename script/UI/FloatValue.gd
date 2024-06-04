tool
extends Control

export var caption: String setget caption_set, caption_get
export var range_min: float setget range_min_set, range_min_get
export var range_max: float setget range_max_set, range_max_get
export var float_value: float setget float_value_set, float_value_get


func caption_set(value):
	var label = get_node_or_null("label")
	if label != null:
		label.text = value
	caption = value
	property_list_changed_notify()
func caption_get():
	return caption

func range_min_set(value: float):
	var slider = get_node_or_null("slider")
	if slider != null:
		slider.min_value = value
	range_min = value
	property_list_changed_notify()
func range_min_get():
	return range_min

func range_max_set(value: float):
	var slider = get_node_or_null("slider")
	if slider != null:
		slider.max_value = value
	range_max = value
	property_list_changed_notify()
func range_max_get():
	return range_max

func float_value_set(value: float):
	if float_value == value:
		return
	
	var slider = get_node_or_null("slider")
	if slider != null:
		var sv = value
		if value > range_max:
			sv = range_max
		elif value < range_min:
			sv = range_min
		slider.value = sv
	
	var edit = get_node_or_null("edit")
	if edit != null:
		edit.text = str(value)

	float_value = value
	property_list_changed_notify()

func float_value_get():
	return float_value


# Called when the node enters the scene tree for the first time.
#func _ready():
#	_is_ready = true

# Called every frame. 'delta' is the elapsed time since the previous frame.
#func _process(delta):
#	pass


func _on_line_edit_text_changed(new_text):
	float_value_set(float(new_text))


func _on_h_slider_value_changed(value):
	float_value_set(value)
