extends Control
# Per-instance sap/reveal state. No shared shader/material/tween mutation.
var route = PackedVector2Array()
var progress = 0.0:
	set(value):
		progress = value
		queue_redraw()

func _ready() -> void:
	mouse_filter = Control.MOUSE_FILTER_IGNORE

func _draw() -> void:
	if route.size() < 2 or progress <= 0: return
	var count = min(int(progress * (route.size() - 1)), route.size() - 1)
	var points = PackedVector2Array()
	for i in range(count + 1): points.append(route[i])
	if count < route.size() - 1:
		points.append(route[count].lerp(route[count+1], progress * (route.size()-1) - count))
	if points.size() < 2: return
	draw_polyline(points, Color(1, .53, .16, .09), 19, true)
	draw_polyline(points, Color(1, .64, .23, .27), 10, true)
	draw_polyline(points, Color("de9b4c"), 4, true)
	draw_polyline(points, Color("ffe4a0"), 1.4, true)
	if progress < 1:
		draw_circle(points[-1], 10, Color(1, .74, .3, .2))
		draw_circle(points[-1], 4, Color("fff0b4"))
