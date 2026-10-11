extends PanelContainer
signal requested(action_id: String)
var data: Dictionary = {}
func _ready() -> void:
	$Body/Action.pressed.connect(func(): requested.emit(str(data.get("id", ""))))
	refresh()
func set_data(value: Dictionary) -> void:
	data = value.duplicate(true)
	if is_node_ready(): refresh()
func refresh() -> void:
	$Body/Action.text = str(data.get("title", "Action"))
	$Body/Action.disabled = not bool(data.get("enabled", false))
	$Body/Action.theme_type_variation = str(data.get("variant", "CardButton"))
	$Body/Action.icon = WatchUI.resource_icon(str(data.get("icon", "")))
	$Body/Action.add_theme_constant_override("icon_max_width", 24)
	$Body/Quote.text = str(data.get("quote", ""))
	$Body/Quote.visible = not $Body/Quote.text.is_empty()
	$Body/Reason.text = str(data.get("reason", ""))
	$Body/Reason.visible = not $Body/Reason.text.is_empty()
	$Body/Action.tooltip_text = $Body/Quote.text + "\n" + $Body/Reason.text
