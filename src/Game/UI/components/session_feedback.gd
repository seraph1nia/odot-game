extends VBoxContainer
signal requested(action_id: String)
func _ready() -> void:
	$Actions/Reconnect.pressed.connect(func(): requested.emit("reconnect"))
	$Actions/Fresh.pressed.connect(func(): requested.emit("fresh-session"))
func set_data(value: Dictionary) -> void:
	$Status.text = str(value.get("status", ""))
	$Message.text = str(value.get("message", ""))
	$Actions/Reconnect.visible = bool(value.get("reconnect", false))
	$Actions/Fresh.visible = bool(value.get("fresh", false))
	$Actions.visible = $Actions/Reconnect.visible or $Actions/Fresh.visible
