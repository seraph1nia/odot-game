extends VBoxContainer
# Compact local allocation UI, not game authority, persistence or a new economy.
signal purchased(node_id: String, remaining_points: int)
signal rejected(node_id: String, reason: String)
signal selected(node_id: String)

const CENTERS = [Vector2(480, 215), Vector2(350, 155), Vector2(220, 95), Vector2(90, 35), Vector2(610, 155), Vector2(740, 95), Vector2(870, 35), Vector2(480, 315), Vector2(480, 415), Vector2(480, 515)]
var nodes: Dictionary = {}
var owned: Array = []
var points = 0
var selected_id = ""
var buttons: Dictionary = {}
var links: Dictionary = {}
var scroll: ScrollContainer
var canvas: Control
var detail: Control
var summary: Label
var feedback: Label
var present_root_on_sort = false

func _ready() -> void:
	summary = WatchUI.label(self, "Supply a skill-tree snapshot", "HeadingLabel")
	WatchUI.label(self, "Start in the middle · three paths · every step needs its parent. Select a node to inspect; purchase below.", "ContextLabel")
	scroll = ScrollContainer.new()
	scroll.name = "TreeScroll"
	scroll.follow_focus = true
	scroll.custom_minimum_size.y = 280
	scroll.size_flags_vertical = Control.SIZE_EXPAND_FILL
	add_child(scroll)
	scroll.sort_children.connect(present_root)
	canvas = Panel.new()
	canvas.add_theme_stylebox_override("panel", get_theme_stylebox("panel", "InsetPanel"))
	canvas.name = "Nodes"
	canvas.custom_minimum_size = Vector2(960, 550)
	scroll.add_child(canvas)
	detail = WatchUI.component("quoted_action", self)
	detail.requested.connect(func(id): purchase(id))
	feedback = WatchUI.label(self, "", "ContextLabel")
	build_nodes()

# Exact small topology: one 1-point root and three independent 1–3-step chains.
# Parent ids are explicit and must match the preceding step. Invalid snapshots
# leave the previous state intact. Callers may replace the snapshot at any time.
func set_data(value: Dictionary) -> bool:
	var definitions: Dictionary = {}
	var root: Dictionary = value.get("root", {})
	var root_id = str(root.get("id", ""))
	var branches: Array = value.get("branches", [])
	if root_id.is_empty() or root.get("cost", 1) != 1 or branches.size() != 3: return false
	if not value.get("points", 0) is int or value.get("points", 0) < 0: return false
	definitions[root_id] = root.duplicate(true)
	definitions[root_id].merge({"cost":1, "parent":"", "center":CENTERS[0], "branch":"Foundation"}, true)
	for branch_index in range(3):
		var branch: Dictionary = branches[branch_index]
		var steps: Array = branch.get("nodes", [])
		if steps.is_empty() or steps.size() > 3: return false
		var parent = root_id
		for index in range(steps.size()):
			var step: Dictionary = steps[index]
			var id = str(step.get("id", ""))
			if id.is_empty() or definitions.has(id) or str(step.get("parent", "")) != parent: return false
			if not step.get("cost", 1) is int or step.get("cost", 1) < 1: return false
			definitions[id] = step.duplicate(true)
			definitions[id].merge({"center":CENTERS[1 + branch_index * 3 + index], "branch_index":branch_index, "branch":str(branch.get("title", "Path " + str(branch_index + 1)))}, true)
			parent = id
	var ownership: Array = value.get("owned", [])
	var unique: Array = []
	for id in ownership:
		if not definitions.has(id) or id in unique: return false
		unique.append(id)
	for id in unique:
		var parent = str(definitions[id].parent)
		if not parent.is_empty() and parent not in unique: return false
	nodes = definitions
	owned = unique
	points = value.get("points", 0)
	selected_id = root_id
	if is_node_ready():
		feedback.text = ""
		build_nodes()
	return true

func node_state(id: String) -> String:
	if not nodes.has(id): return "unknown"
	if id in owned: return "owned"
	var parent = str(nodes[id].parent)
	if not parent.is_empty() and parent not in owned: return "locked"
	if points < int(nodes[id].get("cost", 1)): return "insufficient"
	return "available"

func reason(id: String) -> String:
	match node_state(id):
		"owned": return "Already owned · no additional spending."
		"locked": return "Requires " + str(nodes[nodes[id].parent].get("title", nodes[id].parent)) + " first."
		"insufficient": return "Insufficient skill points · need " + str(int(nodes[id].get("cost", 1)) - points) + " more."
		"available": return "Available · parent requirement met."
	return "Unknown node."

func purchase(id: String) -> bool:
	if node_state(id) != "available":
		var why = reason(id)
		if is_node_ready(): feedback.text = why
		rejected.emit(id, why)
		return false
	points -= int(nodes[id].get("cost", 1))
	owned.append(id)
	if is_node_ready():
		feedback.text = "Purchased " + str(nodes[id].get("title", id)) + "."
		refresh()
	purchased.emit(id, points)
	return true

func select_node(id: String) -> void:
	if not nodes.has(id): return
	selected_id = id
	refresh()
	selected.emit(id)

func build_nodes() -> void:
	WatchUI.clear(canvas)
	buttons.clear()
	links.clear()
	# Lightweight native Line2D decoration behind native Buttons; no custom input.
	for id in nodes:
		var parent = str(nodes[id].parent)
		if parent.is_empty(): continue
		var link = Line2D.new()
		link.add_point(nodes[parent].center)
		link.add_point(nodes[id].center)
		canvas.add_child(link)
		links[id] = link
	var branch_indices: Array = []
	for id in nodes:
		if nodes[id].parent.is_empty(): continue
		var branch_index = int(nodes[id].branch_index)
		if branch_index in branch_indices: continue
		branch_indices.append(branch_index)
		var heading = Label.new()
		heading.text = str(nodes[id].branch)
		heading.theme_type_variation = "ContextLabel"
		heading.mouse_filter = Control.MOUSE_FILTER_IGNORE
		heading.position = [Vector2(130, 215), Vector2(640, 215), Vector2(575, 345)][branch_index]
		canvas.add_child(heading)
	for id in nodes:
		var button = Button.new()
		button.name = "Node_" + id
		button.position = nodes[id].center - Vector2(62, 31)
		button.size = Vector2(124, 62)
		button.add_theme_font_size_override("font_size", 14)
		button.clip_text = true
		button.toggle_mode = true
		button.pressed.connect(select_node.bind(id))
		button.focus_entered.connect(select_node.bind(id))
		canvas.add_child(button)
		buttons[id] = button
	refresh()
	# Let the VBox and ScrollContainer perform their native layout first. Never
	# capture a Button in queued work: a snapshot can replace it in the same frame.
	present_root_on_sort = true
	queue_sort()

func present_root() -> void:
	if not present_root_on_sort or not is_inside_tree() or is_queued_for_deletion(): return
	var target: Control = buttons.get(selected_id)
	if not is_instance_valid(target) or not scroll.is_ancestor_of(target): return
	if scroll.size.x <= 0 or scroll.get_h_scroll_bar().page <= 0: return
	present_root_on_sort = false
	scroll.ensure_control_visible(target)

func refresh() -> void:
	if not is_node_ready(): return
	summary.text = str(points) + " skill points · " + str(owned.size()) + "/" + str(nodes.size()) + " owned"
	for id in nodes:
		var state = node_state(id)
		var b: Button = buttons[id]
		# Locked/owned nodes remain inspectable by pointer and keyboard.
		var state_label = "Unfunded" if state == "insufficient" else state.capitalize()
		b.text = str(nodes[id].get("title", id)) + "\n" + str(nodes[id].get("cost", 1)) + " pt · " + state_label
		b.button_pressed = id == selected_id
		b.theme_type_variation = "PrimaryButton" if state in ["available", "owned"] else ""
		b.tooltip_text = str(nodes[id].get("description", "")) + "\n" + reason(id)
		if links.has(id):
			links[id].width = 4 if state == "owned" else 2
			links[id].default_color = Color("23889b") if state == "owned" else Color("b88d44") if state in ["available", "insufficient"] else Color("a99b7e")
	if nodes.has(selected_id):
		var n: Dictionary = nodes[selected_id]
		detail.set_data({"id":selected_id, "title":"Purchase " + str(n.get("title", selected_id)), "enabled":node_state(selected_id) == "available", "variant":"PrimaryButton", "quote":str(n.branch) + " · " + str(n.get("cost", 1)) + " skill point(s)\n" + str(n.get("description", "")), "reason":reason(selected_id)})
	else: detail.set_data({"title":"Select a skill", "enabled":false})
