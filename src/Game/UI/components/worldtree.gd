extends Control
# Fixed canonical visual composition, intent-only. No local allocation or costs.
signal node_selected(id: String)
signal node_action_requested(id: String)
signal awakening_finished(id: String)
const ART = "res://UI/art/worldtree/"
const Branch = preload("res://UI/components/worldtree_branch.gd")
var topology: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(ART + "topology.json"))
var modules: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(ART + "modules.json"))
var definitions: Dictionary = {}
var buttons: Dictionary = {}
var edges: Dictionary = {}
var visuals: Dictionary = {}
var snapshot: Dictionary = {}
var canvas: Control
var points_label: Label
var footer: Label
var action: Button
var animation: Tween
var awakening_id = ""
var root_life: TextureRect

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_PASS
	for n in topology.nodes: definitions[n.id] = n
	var background = ColorRect.new()
	background.color = Color("101f27")
	background.mouse_filter = Control.MOUSE_FILTER_IGNORE
	background.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	add_child(background)
	var forest = picture(self, "forest", Vector2.ZERO, size)
	forest.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	points_label = Label.new()
	points_label.position = Vector2(22, 12)
	points_label.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(points_label)
	canvas = Control.new()
	canvas.mouse_filter = Control.MOUSE_FILTER_IGNORE
	add_child(canvas)
	# Physical wood and anchors are built once, never rebuilt by snapshots.
	picture(canvas, "roots", Vector2(480, 780), Vector2(640,180))
	root_life = picture(canvas, "roots_alive", Vector2(480,780),Vector2(640,180))
	for edge in topology.edges:
		var m: Dictionary = modules[edge.id]
		var branch = Branch.new()
		branch.name = edge.id
		for p in m.points: branch.route.append(Vector2(p[0], p[1]-110))
		canvas.add_child(branch)
		var bark = picture(branch, "branches/" + edge.id, Vector2(m.origin[0],m.origin[1]-110), Vector2(m.size[0],m.size[1]))
		# Sap is drawn by a separate child above the wooden SVG, not behind it.
		var sap = Branch.new()
		sap.route = branch.route
		branch.add_child(sap)
		var pos = Vector2(m.foliage[0]-50, m.foliage[1]-180)
		var dead = picture(branch, "foliage_dead", pos, Vector2(100,110))
		var fresh = picture(branch, "foliage_alive", pos, Vector2(100,110))
		edges[edge.to] = {"id":edge.id,"bark":bark,"sap":sap,"dead":dead,"fresh":fresh}
	for n in topology.nodes:
		var center = Vector2(n.position.x, n.position.y-110)
		if n.role == "terminal": picture(canvas, "twig", center-Vector2(36,100),Vector2(72,96))
		var b = Button.new()
		b.name = "Node_" + n.id
		b.position = center - Vector2(36,36)
		b.size = Vector2(72,72)
		b.toggle_mode = true
		b.mouse_default_cursor_shape = Control.CURSOR_POINTING_HAND
		# Keep native hover/pressed/focus; transparent styles protect original rings.
		for state in ["normal", "pressed", "hover", "disabled"]:
			var style = StyleBoxFlat.new()
			style.bg_color = Color(0,0,0,0)
			if state == "hover": style.bg_color = Color(1,.8,.4,.08)
			style.set_corner_radius_all(36)
			b.add_theme_stylebox_override(state, style)
		var focus = StyleBoxFlat.new()
		focus.bg_color = Color(0,0,0,0)
		focus.border_color = Color("90cddd")
		focus.set_border_width_all(3)
		focus.set_corner_radius_all(36)
		b.add_theme_stylebox_override("focus", focus)
		canvas.add_child(b)
		picture(b, "socket", Vector2(0,0), Vector2(72,72))
		var halo = picture(b, "halo", Vector2(-13,-13), Vector2(98,98))
		var icon = picture(b, "icons/"+n.icon_key, Vector2(17,17), Vector2(38,38))
		var selection = Label.new()
		selection.text = "·"
		selection.position = Vector2(29,66)
		selection.mouse_filter = Control.MOUSE_FILTER_IGNORE
		b.add_child(selection)
		b.pressed.connect(select_node.bind(n.id))
		buttons[n.id] = b
		visuals[n.id] = {"halo":halo,"icon":icon,"selection":selection}
	# Stable native traversal, including all dormant nodes. No custom key handler.
	var ids: Array = definitions.keys()
	for i in range(ids.size()):
		if i < ids.size()-1: buttons[ids[i]].focus_next = buttons[ids[i+1]].get_path()
		if i > 0: buttons[ids[i]].focus_previous = buttons[ids[i-1]].get_path()
	var row = HBoxContainer.new()
	row.name = "SelectedNodeFooter"
	row.position = Vector2(22, size.y-62)
	add_child(row)
	footer = Label.new()
	footer.custom_minimum_size.x = 650
	footer.mouse_filter = Control.MOUSE_FILTER_IGNORE
	row.add_child(footer)
	action = Button.new()
	action.text = "Request awakening"
	action.disabled = true
	action.focus_mode = Control.FOCUS_NONE
	action.pressed.connect(request_action)
	row.add_child(action)
	# Focus alone previews the native focus ring; Enter inspects. Preserve selection
	# while tabbing so the footer's intent button remains keyboard reachable.
	action.focus_previous = buttons[ids[-1]].get_path()
	resized.connect(layout_tree)
	layout_tree()
	if not snapshot.is_empty():
		refresh()
		if not awakening_id.is_empty(): awaken(awakening_id)
	else:
		canvas.hide()
		points_label.text = "Skill Points —"

func picture(parent: Node, key: String, pos: Vector2, dimensions: Vector2) -> TextureRect:
	var p = TextureRect.new()
	p.texture = load(ART + key + ".svg")
	p.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	p.position = pos
	p.size = dimensions
	p.mouse_filter = Control.MOUSE_FILTER_IGNORE
	parent.add_child(p)
	return p

func layout_tree() -> void:
	if canvas == null: return
	var factor = min((size.x-32)/1600.0, (size.y-112)/970.0)
	factor = max(factor, .1)
	canvas.scale = Vector2.ONE * factor
	canvas.position = Vector2((size.x-1600*factor)/2, 44)
	get_node("SelectedNodeFooter").position = Vector2(22, size.y-60)
	footer.custom_minimum_size.x = max(300, size.x-270)

# Reject malformed complete snapshots atomically; never infer gameplay progression.
func set_data(value: Dictionary, animate_id: String = "") -> bool:
	var known: Dictionary = {}
	for n in topology.nodes: known[n.id] = n
	for key in ["acquired", "available", "icons", "selected", "points"]:
		if not value.has(key): return false
	if not value.points is int or value.points < 0: return false
	if not value.selected is String or not known.has(value.selected): return false
	if not value.icons is Dictionary or value.icons.size() != 24: return false
	for id in value.icons:
		if not id is String or not known.has(id) or not value.icons[id] is String or value.icons[id] not in topology.icon_catalog: return false
	for key in ["acquired", "available"]:
		if not value[key] is Array: return false
		var unique: Array = []
		for id in value[key]:
			if not id is String or not known.has(id) or id in unique: return false
			unique.append(id)
	for id in value.acquired:
		if id in value.available: return false
		var parent = known[id].parent
		if parent != null and parent not in value.acquired: return false
	if not animate_id.is_empty():
		if not known.has(animate_id) or animate_id not in value.acquired or animate_id in snapshot.get("acquired", []): return false
		var parent = known[animate_id].parent
		if parent != null and parent not in snapshot.get("acquired", []): return false
	if animation != null: animation.kill()
	awakening_id = animate_id
	snapshot = value.duplicate(true)
	if is_node_ready():
		canvas.show()
		refresh()
		if not animate_id.is_empty(): awaken(animate_id)
	return true

func node_state(id: String) -> String:
	if not buttons.has(id) and not definitions.has(id):
		var found = false
		for n in topology.nodes:
			if n.id == id: found = true
		if not found: return "unknown"
	if id in snapshot.get("acquired", []): return "acquired"
	if id in snapshot.get("available", []): return "available"
	return "dormant"

func select_node(id: String) -> void:
	if not definitions.has(id) or snapshot.is_empty(): return
	snapshot.selected = id
	refresh_selection()
	node_selected.emit(id)

func request_action() -> void:
	if not snapshot.is_empty() and node_state(snapshot.selected) == "available":
		node_action_requested.emit(snapshot.selected)

func refresh_selection() -> void:
	for id in buttons:
		buttons[id].set_pressed_no_signal(id == snapshot.selected)
		visuals[id].selection.visible = id == snapshot.selected
	var id: String = snapshot.selected
	footer.text = snapshot.icons[id].replace("_", " ").capitalize() + " · " + node_state(id).capitalize() + "\n" + id + " · visual preview only"
	action.disabled = node_state(id) != "available"
	action.focus_mode = Control.FOCUS_NONE if action.disabled else Control.FOCUS_ALL
	var ids: Array = definitions.keys()
	buttons[ids[-1]].focus_next = NodePath() if action.disabled else action.get_path()

func refresh() -> void:
	points_label.text = "Skill Points  " + str(snapshot.points)
	root_life.modulate.a = 1.0 if "root" in snapshot.acquired and awakening_id != "root" else 0.0
	for id in buttons:
		var state = node_state(id)
		var v: Dictionary = visuals[id]
		v.icon.texture = load(ART + "icons/" + snapshot.icons[id] + ".svg")
		v.icon.modulate = Color("ffe6a6") if state == "acquired" else Color("c9bea3") if state == "available" else Color("696565")
		v.halo.modulate.a = 1.0 if state == "acquired" else .22 if state == "available" else 0.0
		buttons[id].tooltip_text = snapshot.icons[id].replace("_", " ").capitalize() + " · " + state.capitalize() + "\n" + id
		if edges.has(id):
			var e: Dictionary = edges[id]
			e.sap.progress = 1.0 if state == "acquired" and id != awakening_id else 0.0
			e.fresh.modulate.a = 1.0 if state == "acquired" and id != awakening_id else 0.0
			e.dead.visible = state != "acquired" or id == awakening_id
		if id == awakening_id: v.halo.modulate.a = 0; v.icon.modulate = Color("696565")
	refresh_selection()

func awaken(id: String) -> void:
	animation = create_tween()
	if edges.has(id):
		animation.tween_property(edges[id].sap, "progress", 1.0, 1.2).set_trans(Tween.TRANS_SINE)
	if id == "root": animation.tween_property(root_life, "modulate:a", 1.0, .35)
	animation.tween_property(visuals[id].halo, "modulate:a", 1.0, .18)
	animation.parallel().tween_property(visuals[id].icon, "modulate", Color("ffe6a6"), .18)
	if edges.has(id):
		animation.tween_property(edges[id].fresh, "modulate:a", 1.0, .35)
		animation.tween_callback(func(): edges[id].dead.visible = false)
	animation.tween_property(visuals[id].halo, "modulate:a", .5, .12)
	animation.tween_property(visuals[id].halo, "modulate:a", 1.0, .18)
	animation.tween_callback(func(): awakening_id = ""; awakening_finished.emit(id))
