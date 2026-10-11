extends PanelContainer
# Rows are already-formatted forecast/receipt projections, never calculated here.
var data: Dictionary = {}
func _ready() -> void: refresh()
func set_data(value: Dictionary) -> void:
	data = value.duplicate(true)
	if is_node_ready(): refresh()
func refresh() -> void:
	WatchUI.clear($Body)
	WatchUI.label($Body, str(data.get("heading", "Upkeep · Next battle")), "HeadingLabel")
	var grid = GridContainer.new()
	grid.columns = 2
	grid.add_theme_constant_override("h_separation", 12)
	grid.add_theme_constant_override("v_separation", 3)
	$Body.add_child(grid)
	for item in data.get("rows", []):
		WatchUI.label(grid, str(item.get("label", "")))
		var amount = WatchUI.label(grid, str(item.get("value", "")))
		amount.size_flags_horizontal = Control.SIZE_SHRINK_END
		amount.horizontal_alignment = HORIZONTAL_ALIGNMENT_RIGHT
		amount.autowrap_mode = TextServer.AUTOWRAP_OFF
	var detail = str(data.get("detail", ""))
	if not detail.is_empty(): WatchUI.label($Body, detail, "ContextLabel")
