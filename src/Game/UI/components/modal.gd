class_name WatchModal
extends Control
# Visible-modal input scope only. No Window refocus or unconditional global router.
signal canceled
signal confirmed
signal closed
@export var title: String = "Dialog"
@export var ok_button_text: String = "Close"
var invoker: Control
var confirmation = false

func _ready() -> void:
	set_process_input(false)
	visibility_changed.connect(func(): set_process_input(is_visible_in_tree()))
	$Center/Panel/Body/Header/Cancel.pressed.connect(cancel)
	$Center/Panel/Body/Footer/Close.pressed.connect(func():
		if confirmation: confirmed.emit()
		close())
	$Center/Panel/Body/Footer/Stay.pressed.connect(cancel)

func get_content() -> VBoxContainer:
	return $Center/Panel/Body/Scroll/Content
func get_scroll() -> ScrollContainer:
	return $Center/Panel/Body/Scroll
func get_ok_button() -> Button:
	return $Center/Panel/Body/Footer/Close
func get_panel() -> PanelContainer:
	return $Center/Panel
func popup_centered_clamped(minimum: Vector2i, ratio: float = .9) -> void:
	$Center/Panel/Body/Header/Title.text = title
	get_ok_button().text = ok_button_text
	$Center/Panel/Body/Footer/Stay.visible = confirmation
	get_panel().custom_minimum_size = Vector2(minimum).min(get_viewport_rect().size * ratio)
	show()
	set_process_input(true)
	get_ok_button().grab_focus()
func cancel() -> void:
	if not is_visible_in_tree(): return
	canceled.emit()
	close()
func close() -> void:
	if not visible: return
	hide()
	set_process_input(false)
	if invoker != null and is_instance_valid(invoker) and invoker.is_visible_in_tree() and get_window().has_focus():
		invoker.grab_focus()
	closed.emit()

func owns_popup(node: Node) -> bool:
	for child in node.get_children(true):
		if child is Window and child.visible: return true
		if owns_popup(child): return true
	return false
func focusable(node: Node, list: Array[Control]) -> void:
	# Include native internal TabBars/scrollbars, but never child Window scopes.
	for child in node.get_children(true):
		if child is Window: continue
		if child is Control and child.is_visible_in_tree() and child.focus_mode == Control.FOCUS_ALL:
			if not (child is BaseButton and child.disabled): list.append(child)
		focusable(child, list)
func _input(event: InputEvent) -> void:
	if not is_visible_in_tree() or owns_popup(self): return
	if event is InputEventKey and event.pressed:
		if event.is_action_pressed("ui_cancel"):
			get_viewport().set_input_as_handled()
			cancel()
		elif event.keycode == KEY_TAB:
			var controls: Array[Control] = []
			focusable(self, controls)
			if controls.is_empty(): return
			var index = controls.find(get_viewport().gui_get_focus_owner())
			var step = -1 if event.shift_pressed else 1
			controls[posmod(index + step, controls.size())].grab_focus()
			get_viewport().set_input_as_handled()
