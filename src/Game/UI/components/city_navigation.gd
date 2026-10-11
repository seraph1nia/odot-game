extends VBoxContainer
signal city_requested(city_id: int)
var cities: Array = []
var current_id: int = -1
func _ready() -> void:
	$Row/Previous.pressed.connect(func(): cycle(-1))
	$Row/Next.pressed.connect(func(): cycle(1))
func set_data(items: Array, selected_id: int) -> void:
	cities = items.duplicate(true)
	current_id = selected_id
	for item in cities:
		if int(item.id) == selected_id:
			$Row/Caption.text = str(item.label)
			$Context.text = str(item.get("context", ""))
	$Row/Previous.disabled = cities.size() < 2
	$Row/Next.disabled = cities.size() < 2
func cycle(delta: int) -> void:
	if cities.is_empty(): return
	for i in range(cities.size()):
		if int(cities[i].id) == current_id:
			city_requested.emit(int(cities[posmod(i + delta, cities.size())].id))
			return
