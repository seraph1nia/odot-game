@tool
extends EditorScenePostImport

# The locked glTF importer substitutes white for an omitted emissiveFactor
# whenever emissiveTexture exists. glTF's legal default is black, not white.
# This post-import hook is attached only to the two affected pinned exports.
# Correct the one color on their original shared material resources, retaining
# the unused texture, all other PBR properties and mesh/surface identities.
func _post_import(scene: Node) -> Object:
	var bytes := FileAccess.get_file_as_bytes(get_source_file())
	assert(bytes.size() >= 28 and bytes.decode_u32(0) == 0x46546c67 and bytes.decode_u32(4) == 2)
	var length := bytes.decode_u32(12)
	assert(length > 0 and length <= bytes.size() - 28)
	var source: Dictionary = JSON.parse_string(bytes.slice(20, 20 + length).get_string_from_utf8())
	var used := {}
	for mesh in source.meshes:
		for primitive in mesh.primitives:
			used[int(primitive.material)] = true
	var targets := {}
	var names := {}
	for index in range(source.materials.size()):
		var material: Dictionary = source.materials[index]
		var name: String = material.name
		assert(not names.has(name), "Ambiguous authored material identity: " + name)
		names[name] = true
		var factor: Array = material.get("emissiveFactor", [0, 0, 0])
		assert(factor.size() == 3)
		if used.has(index) and material.has("emissiveTexture") and factor == [0, 0, 0]:
			targets[name] = index
	var matched := {}
	for node in scene.find_children("*", "MeshInstance3D", true, false):
		for surface in range(node.mesh.get_surface_count()):
			var imported = node.mesh.surface_get_material(surface)
			if imported is StandardMaterial3D and targets.has(imported.resource_name):
				assert(imported.emission_texture != null and imported.emission_operator == BaseMaterial3D.EMISSION_OP_MULTIPLY)
				imported.emission = Color.BLACK
				matched[imported.resource_name] = true
	assert(matched.size() == targets.size(), "Missing source-zero imported material binding")
	return scene
