extends SceneTree
# Explicit headless entry point: no main scene, match, scenery, display or captures.
func _initialize() -> void:
	root.add_child(load("res://UiResourceProbe.cs").new())
