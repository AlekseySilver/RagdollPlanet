tool
extends MenuButton

class_name MyToolPluginButton

const XTS = preload("xts.gd")
const river_maker = preload("river_maker.gd")

var selection : Array
var method_dict : Dictionary
var editor_camera : Camera

func _ready():
	var pu = get_popup()
	var id = 10
	for m in get_script().get_script_method_list():
		if _popup_add(m.name, id):
			id += 1
	#pu.connect("id_pressed", self, "_on_menu_item_pressed")

	
func _popup_add(name, id):
	if name.left(1) == "_":
		return false
	var arr = name.split("__")
	if len(arr) != 2:
		return false
	var submenu = arr[0]
	var menu = arr[1]
	
	var pu = get_popup()
	var pop = pu.get_node_or_null(submenu)
	if pop == null:
		pop = PopupMenu.new()
		pop.set_name(submenu)
		pu.add_child(pop)
		pu.add_submenu_item(submenu, submenu)
		pop.connect("id_pressed", self, "_on_menu_item_pressed")
	pop.add_item(menu, id)
	method_dict[id] = name
	return true


func _on_menu_item_pressed(id):
	var iname = method_dict[id]
	#print(iname)
	call(iname)
	
func _commit_mesh(mdt, mi):
	var mat = mi.get_surface_material(0)
	mi.mesh.surface_remove(0)
	mdt.commit_to_surface(mi.mesh)
	mi.set_surface_material(0, mat)
	

func mesh__recalc_normals():
	print("recalc_normals v1.0")
	for sel in selection:
		print(sel.name)
		var mi = sel as MeshInstance
		if mi != null and mi.mesh != null:
			var mdt = MeshDataTool.new()
			mdt.create_from_surface(mi.mesh, 0)

			var vx_count = mdt.get_vertex_count()
			# zero normal
			for i in range(vx_count):
				mdt.set_vertex_normal(i, Vector3.ZERO)
			
			# Calculate vertex normals, face-by-face.
			for i in range(mdt.get_face_count()):
				# Get the index in the vertex array.
				var a = mdt.get_face_vertex(i, 0)
				var b = mdt.get_face_vertex(i, 1)
				var c = mdt.get_face_vertex(i, 2)
				# Get vertex position using vertex index.
				var ap = mdt.get_vertex(a)
				var bp = mdt.get_vertex(b)
				var cp = mdt.get_vertex(c)
				# Calculate face normal.
				var n = (bp - cp).cross(ap - bp).normalized()
				# Add face normal to current vertex normal.
				# This will not result in perfect normals, but it will be close.
				mdt.set_vertex_normal(a, n + mdt.get_vertex_normal(a))
				mdt.set_vertex_normal(b, n + mdt.get_vertex_normal(b))
				mdt.set_vertex_normal(c, n + mdt.get_vertex_normal(c))

			# Run through vertices one last time to normalize normals
			for i in range(vx_count):
				var v = mdt.get_vertex_normal(i).normalized()
				mdt.set_vertex_normal(i, v)
			
			# commit
			_commit_mesh(mdt, mi)
	
func mesh__noise_to_vertex_color():
	print("noise_to_vertex_color v1.0")
	var sn = OpenSimplexNoise.new()
	sn.period = 0.7
	var mdt = MeshDataTool.new()
	
	for sel in selection:
		print(sel.name)
		var mi = sel as MeshInstance
		if mi != null and mi.mesh != null:
			mdt.create_from_surface(mi.mesh, 0)
			var vx_count = mdt.get_vertex_count()
			for i in range(vx_count):
				var vertex = mdt.get_vertex(i)
				var color = Color.white * abs(sn.get_noise_3dv(vertex))
				mdt.set_vertex_color(i, color)

			# commit
			_commit_mesh(mdt, mi)

func mesh__change_material():
	print("change_mesh_material v1.0")
	for sel in selection:
		print(sel.name)
		var mi = sel as MeshInstance
		if mi != null and mi.mesh != null:
			var n = mi.mesh.get_surface_count()
			for i in range(n):
				mi.mesh.surface_set_material(i, null)

func mesh__apply_transform():
	print("apply_transform v1.0")
	for sel in selection:
		print(sel.name)
		var mi = sel as MeshInstance
		if mi != null and mi.mesh != null:
			var mdt = MeshDataTool.new()
			mdt.create_from_surface(mi.mesh, 0)

			var vx_count = mdt.get_vertex_count()
			# calc center
			var center = Vector3.ZERO
			for i in range(vx_count):
				var vertex = mdt.get_vertex(i)
				center += vertex
			center *= 1.0 / vx_count

			# set center to zero 
			for i in range(vx_count):
				var vertex = mdt.get_vertex(i)
				mdt.set_vertex(i, vertex - center)

			# commit
			_commit_mesh(mdt, mi)
			mi.translate(center)

func mesh__create_convex_collision():
	print("create_convex v1.5")
	for sel in selection:
		print(sel.name)
		var mi = sel as MeshInstance
		if mi != null:
			for ch in mi.get_children():
				ch.queue_free()
			mi.create_convex_collision()

func align__up():
	print("up_align v1")
	for s in selection:
		var sp = s as Spatial
		if sp:
			XTS.up_align_node(sp)
			
func align__X():
	print("x_align v1")
	for s in selection:
		var sp = s as Spatial
		if sp:
			XTS.x_align_node(sp)

func align__Z():
	print("z_align v1")
	for s in selection:
		var sp = s as Spatial
		if sp:
			XTS.z_align_node(sp)

func _get_root_spatial():
	for s in selection:
		var sp = s as Spatial
		if sp:
			var root = sp
			while root:
				sp = root
				root = sp.get_parent_spatial()
			print("root name = " + sp.name)
			return sp
	return null
			
func align__root_reset():
	print("align__root_reset v1")
	var sp = _get_root_spatial()
	if sp:
		print("root name = " + sp.name)
		sp.transform = Transform.IDENTITY
		#_look_at_selection()

func align__root_up():
	print("align__root_up v1")
	var sp = _get_root_spatial()
	if sp:
		print("root name = " + sp.name)
		var xf = sp.global_transform
		xf.basis = XTS.up_align(xf.basis, selection[0].global_transform.origin.normalized())
		xf.basis = xf.basis.transposed()
		sp.global_transform = xf
		#_look_at_selection()
		
		


func _look_at_selection():
	var target = selection[0].global_transform.origin
	var pos = editor_camera.global_transform.origin
	pos.y = target.y
	editor_camera.look_at_from_position(pos, target, Vector3.UP)


func test__save_as_scene():
	print("test__save_as_scene v1.2")
	for s in selection:
		var node = s as Node
		if node:
			var path = "res://scene/island/parts/ground_ligthhouse/" + node.name + ".tscn"
			node.filename = path
			_set_node_owner_rec(node, node)
			var ps = PackedScene.new()
			ps.pack(node)
			print(path)
			ResourceSaver.save(path, ps)
				
				
func _set_node_owner_rec(node, owner):
	for c in node.get_children():
		c.owner = owner
		_set_node_owner_rec(c, owner)

func test__dir_contents():
	print("dir_contents v1.2")
	var path = "res://scene/details/var/"
	var dir = Directory.new()
	if dir.open(path) == OK:
		dir.list_dir_begin()
		var file_name = dir.get_next()
		var file = File.new()
		var shape = CylinderShape.new()
		shape.radius = 1
		shape.height = 2
		while file_name != "":
			if not dir.current_is_dir():
				print("Found file: " + file_name)
				if file_name.left(4) == "box_":
					var new_name = file_name.replace("box_", "cylinder_")
					if not file.file_exists(path + new_name):
						var ps = ResourceLoader.load(path + file_name) as PackedScene
						var sb = ps.instance(PackedScene.GEN_EDIT_STATE_INSTANCE)
						var cs = sb.get_child(0) as CollisionShape
						cs.shape = shape
						var mi = sb.get_child(1) as MeshInstance
						mi.mesh = ResourceLoader.load("res://geometry/cylinder32.mesh")
						ps = PackedScene.new()
						ps.pack(sb)
						ResourceSaver.save(path + new_name, ps)
						#return
					
			file_name = dir.get_next()
	else:
		print("An error occurred when trying to access the path.")



func river__make_river():
	print("river__make_river v2")
	for s in selection:
		var m = s as Spatial
		if m:
			print(m.name)
			var rm = river_maker.new()
			rm.make_river(m, 10)
