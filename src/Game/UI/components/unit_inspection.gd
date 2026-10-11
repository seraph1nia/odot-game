extends PanelContainer
signal requested(action_id: String, unit_id: int, destination_id: String)
var data: Dictionary = {}
func _ready() -> void:
	$Body/Retire.pressed.connect(func(): requested.emit("retire", int(data.get("id", -1)), ""))
	$Body/Send.pressed.connect(func(): requested.emit(str(data.get("send_id", "store")), int(data.get("id", -1)), destination()))
	$Body/Destination.item_selected.connect(func(_index): update_destination())
func set_data(value: Dictionary) -> void:
	data = value.duplicate(true)
	$Body/Title.text = str(data.get("name", "Unit")) + " · " + str(data.get("level", "I"))
	$Body/Profile.text = str(data.get("profile", ""))
	$Body/Health.value = float(data.get("health_percent", 0))
	$Body/HealthText.text = str(data.get("health_text", ""))
	$Body/Statuses.text = str(data.get("statuses", "No current statuses"))
	$Body/Recovery.text = str(data.get("recovery", ""))
	$Body/Recovery.visible = not $Body/Recovery.text.is_empty()
	$Body/PreviewSlot.visible = bool(data.get("show_preview", false))
	$Body/Retire.disabled = not bool(data.get("can_retire", false))
	$Body/Destination.clear()
	for item in data.get("destinations", []):
		$Body/Destination.add_item(str(item.get("label", "")))
		$Body/Destination.set_item_metadata($Body/Destination.item_count - 1, str(item.get("id", "")))
	$Body/Destination.disabled = $Body/Destination.item_count == 0
	$Body/Send.text = str(data.get("send_label", "Send to Town hall"))
	update_destination()
func destination() -> String:
	if $Body/Destination.selected < 0: return ""
	return str($Body/Destination.get_item_metadata($Body/Destination.selected))
func update_destination() -> void:
	var options = data.get("destinations", [])
	var index = $Body/Destination.selected
	var enabled = bool(data.get("can_send", false)) and index >= 0 and index < options.size()
	if enabled: enabled = bool(options[index].get("available", false))
	$Body/Send.disabled = not enabled
	$Body/Reason.text = str(data.get("reason", ""))
	if index >= 0 and index < options.size() and not bool(options[index].get("available", false)):
		$Body/Reason.text = str(options[index].get("reason", "Destination unavailable"))
