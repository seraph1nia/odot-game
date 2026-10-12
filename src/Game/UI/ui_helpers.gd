class_name WatchUI
extends RefCounted
# Small native-layout helpers, not a replacement widget framework.
static func label(parent: Node, text: String, role: String = "") -> Label:
	var l = Label.new()
	l.text = text
	l.theme_type_variation = role
	l.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	l.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	l.mouse_filter = Control.MOUSE_FILTER_IGNORE
	parent.add_child(l)
	return l

static func button(parent: Node, title: String, variant: String = "") -> Button:
	var b = Button.new()
	b.text = title
	b.theme_type_variation = variant
	b.custom_minimum_size.y = 36
	b.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	parent.add_child(b)
	return b

# A modal/parent scroll owns the whole body; standalone lists keep their own scroll.
static func scroll(parent: Node, name: String, minimum_height: int) -> ScrollContainer:
	var s = ScrollContainer.new()
	s.name = name
	s.horizontal_scroll_mode = ScrollContainer.SCROLL_MODE_DISABLED
	s.follow_focus = true
	var ancestor = parent
	var inherited = false
	while ancestor != null:
		if ancestor is ScrollContainer and ancestor.vertical_scroll_mode != ScrollContainer.SCROLL_MODE_DISABLED:
			inherited = true
			break
		ancestor = ancestor.get_parent()
	s.vertical_scroll_mode = ScrollContainer.SCROLL_MODE_DISABLED if inherited else ScrollContainer.SCROLL_MODE_AUTO
	s.custom_minimum_size.y = 0 if inherited else minimum_height
	parent.add_child(s)
	return s

static func stack(parent: Node, spacing: int = 8) -> VBoxContainer:
	var v = VBoxContainer.new()
	v.add_theme_constant_override("separation", spacing)
	v.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	parent.add_child(v)
	return v

static func row(parent: Node, spacing: int = 8) -> HBoxContainer:
	var h = HBoxContainer.new()
	h.add_theme_constant_override("separation", spacing)
	h.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	parent.add_child(h)
	return h

static func clear(parent: Node) -> void:
	for child in parent.get_children():
		parent.remove_child(child)
		child.queue_free()

# Original optional scan aids; names/costs remain readable native text.
static func resource_icon(name: String) -> Texture2D:
	var names = ["gold", "food", "wood", "stone", "metal", "cloth"]
	var index = names.find(name.to_lower())
	if index < 0: return null
	var icon = AtlasTexture.new()
	icon.atlas = load("res://UI/art/woodland/resource_icons.svg")
	icon.region = Rect2(index * 32, 0, 32, 32)
	return icon

static func ornament(parent: Node, width: int = 180) -> TextureRect:
	var art = TextureRect.new()
	art.texture = load("res://UI/art/woodland/woodland_corners.svg")
	art.custom_minimum_size = Vector2(width, width * 112.0 / 240.0)
	art.expand_mode = TextureRect.EXPAND_IGNORE_SIZE
	art.stretch_mode = TextureRect.STRETCH_KEEP_ASPECT_CENTERED
	art.mouse_filter = Control.MOUSE_FILTER_IGNORE
	art.focus_mode = Control.FOCUS_NONE
	parent.add_child(art)
	return art

# Canvas-only ornament uses spare panel space, never layout or hit targets.
static func corner_ornament(panel: PanelContainer, body: Container) -> void:
	var art = Sprite2D.new()
	art.texture = load("res://UI/art/woodland/woodland_corners.svg")
	art.scale = Vector2(.45, .45)
	panel.add_child(art)
	var place = func():
		art.visible = panel.size.y - body.get_combined_minimum_size().y - 32 >= 52
		art.position = Vector2(panel.size.x * .5, panel.size.y - 34)
	panel.resized.connect(func(): place.call_deferred())
	place.call_deferred()

static func component(path: String, parent: Node) -> Control:
	var scene = load("res://UI/components/" + path + ".tscn")
	var c = scene.instantiate()
	parent.add_child(c)
	return c
