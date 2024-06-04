tool
extends MeshInstance

class_name river_maker

const XTS = preload("res://addons/my_tools/xts.gd")

# PoolVector**Arrays for mesh construction.
var verts = PoolVector3Array()
var uvs = PoolVector2Array()
var normals = PoolVector3Array()
var indices = PoolIntArray()

var converts = PoolVector3Array()

var _mdt = MeshDataTool.new()

export var depth = 1.0
export var auto_rebuild_time = 0.0

var _auto_rebuild_time_rest = 0.0

func _process(delta):
	if Engine.editor_hint and auto_rebuild_time > 0.0:
		_auto_rebuild_time_rest -= delta
		if _auto_rebuild_time_rest < 0.0:
			#print("make_river")
			make_river(self, depth)
			_auto_rebuild_time_rest = auto_rebuild_time



func _append_quad(vx_id: int) -> int:
	indices.append(vx_id + 0)
	indices.append(vx_id + 1)
	indices.append(vx_id + 2)
	
	indices.append(vx_id + 2)
	indices.append(vx_id + 1)
	indices.append(vx_id + 3)
	
	return vx_id + 4
	
func _add2points(xf: Transform, conv: ConvexPolygonShape, depth: float, uv1: Vector2, uv2: Vector2) -> void:
	var p1 = xf.origin - xf.basis[0]
	var p2 = xf.origin + xf.basis[0]
	verts.append(p1)
	verts.append(p2)
	uvs.append(uv1)
	uvs.append(uv2)
	normals.append(Vector3.ZERO)
	normals.append(Vector3.ZERO)
	
	converts.append(p1)
	converts.append(p2)
	converts.append(Vector3(0, -depth, 0) + p1)
	converts.append(Vector3(0, -depth, 0) + p2)


func _prepare_child(mesh_instance: MeshInstance, child_id: int) -> Transform:
	var child = mesh_instance.get_child(child_id) as Spatial
	var xf = child.global_transform
	child.global_transform = XTS.up_align_xf(xf)
	return child.transform
	
func _add_conv_coll(mesh_instance: MeshInstance, conv: ConvexPolygonShape, name: String):
	var col = CollisionShape.new()
	conv.points = converts
	col.shape = conv
	mesh_instance.get_parent().add_child(col)
	col.owner = mesh_instance.owner
	col.name = name
	print(name + " " + String(conv.points.size()))
	converts = PoolVector3Array()
	
func _clear_shapes(node: Node) -> void:
	for c in node.get_parent().get_children():
		if c is CollisionShape:
			c.queue_free()

func make_river(node: Spatial, depth: float) -> void:
	print("make River v4")
	var mesh_instance: MeshInstance
	if node is MeshInstance:
		mesh_instance = node as MeshInstance;
	elif node.get_parent() is MeshInstance:
		mesh_instance = node.get_parent() as MeshInstance;
	else:
		print("no MeshInstance")
		return

	if mesh_instance.get_child_count() > 1:
		_clear_shapes(mesh_instance)
		var conv = ConvexPolygonShape.new()
		var xf = _prepare_child(mesh_instance, 0)
		_add2points(xf, conv, depth, Vector2.ZERO, Vector2(0, 1))
		var vx_id = 0
		var last_id = mesh_instance.get_child_count() - 1
		
		
		
		for child_id in range(1, last_id):
			xf = _prepare_child(mesh_instance, child_id)
			_add2points(xf, conv, depth, Vector2(1, 0), Vector2.ONE)
			vx_id = _append_quad(vx_id)
			_add_conv_coll(mesh_instance, conv, String(child_id))
			conv = ConvexPolygonShape.new()
			
			_add2points(xf, conv, depth, Vector2.ZERO, Vector2(0, 1))

		xf = _prepare_child(mesh_instance, last_id)
		_add2points(xf, conv, depth, Vector2(1, 0), Vector2.ONE)
		vx_id = _append_quad(vx_id)
		_add_conv_coll(mesh_instance, conv, String(last_id))

		var surface_array = []
		surface_array.resize(Mesh.ARRAY_MAX)
		# Assign arrays to mesh array.
		surface_array[Mesh.ARRAY_VERTEX] = verts
		surface_array[Mesh.ARRAY_TEX_UV] = uvs
		surface_array[Mesh.ARRAY_NORMAL] = normals
		surface_array[Mesh.ARRAY_INDEX] = indices
		# Create mesh surface from mesh array.
		mesh_instance.mesh = ArrayMesh.new()
		mesh_instance.mesh.add_surface_from_arrays(Mesh.PRIMITIVE_TRIANGLES, surface_array)
		_calc_normal(mesh_instance.mesh)

func _calc_normal(mesh: Mesh) -> void:
	_mdt.create_from_surface(mesh, 0)

	# Calculate vertex normals, face-by-face.
	for i in range(_mdt.get_face_count()):
		# Get the index in the vertex array.
		var a = _mdt.get_face_vertex(i, 0)
		var b = _mdt.get_face_vertex(i, 1)
		var c = _mdt.get_face_vertex(i, 2)
		# Get vertex position using vertex index.
		var ap = _mdt.get_vertex(a)
		var bp = _mdt.get_vertex(b)
		var cp = _mdt.get_vertex(c)
		# Calculate face normal.
		var n = (bp - cp).cross(ap - bp).normalized()
		# Add face normal to current vertex normal.
		# This will not result in perfect normals, but it will be close.
		_mdt.set_vertex_normal(a, n + _mdt.get_vertex_normal(a))
		_mdt.set_vertex_normal(b, n + _mdt.get_vertex_normal(b))
		_mdt.set_vertex_normal(c, n + _mdt.get_vertex_normal(c))


	# Run through vertices one last time to normalize normals and
	for i in range(_mdt.get_vertex_count()):
		var v = _mdt.get_vertex_normal(i).normalized()
		_mdt.set_vertex_normal(i, v)
		#_mdt.set_vertex_color(i, Color(v.x, v.y, v.z))

	# compute normals and tangents
	for i4 in range(_mdt.get_vertex_count() / 4):
		var i = i4 * 4
		_set_tangent(i + 0, i + 1)
		_set_tangent(i + 2, i + 3)

	# smooth tangents
	for i4 in range(_mdt.get_vertex_count() / 4 - 1):
		var i = i4 * 4
		_smooth_pair(i + 2, i + 4)
		_smooth_pair(i + 3, i + 5)
		
	# dist on river to UV2.x
	_mdt.set_vertex_uv2(0, Vector2.ZERO)
	_mdt.set_vertex_uv2(1, Vector2.ZERO)
	for i2 in range(1, _mdt.get_vertex_count() / 2):
		var a1 = i2 * 2
		var b1 = a1 + 1
		var a0 = a1 - 2
		var b0 = b1 - 2
		var uv2 = _avg2(_mdt.get_vertex_uv2(a0), _mdt.get_vertex_uv2(b0))
		uv2.x += _dist3(_avg_vertex(a0, b0), _avg_vertex(a1, b1))
		_mdt.set_vertex_uv2(a1, uv2)
		_mdt.set_vertex_uv2(b1, uv2)

	# remove sharp faces
	_mdt


	mesh.surface_remove(0)
	_mdt.commit_to_surface(mesh)

func _dist3(a: Vector3, b: Vector3) -> float:
	return (a - b).length()

func _avg2(a: Vector2, b: Vector2) -> Vector2:
	return (a + b) * 0.5

func _avg3(a: Vector3, b: Vector3) -> Vector3:
	return (a + b) * 0.5
	
func _avg_vertex(a: int, b: int) -> Vector3:
	return _avg3(_mdt.get_vertex(a), _mdt.get_vertex(b))

func _set_tangent(a: int, b: int) -> void:
	var bitan = (_mdt.get_vertex(b) - _mdt.get_vertex(a)).normalized()
	_mdt.set_vertex_tangent(a, Plane(_mdt.get_vertex_normal(a).cross(bitan), 1.0))
	_mdt.set_vertex_tangent(b, Plane(_mdt.get_vertex_normal(b).cross(bitan), 1.0))

func _smooth_pair(a: int, b: int) -> void:
	var normal = (_mdt.get_vertex_normal(a) + _mdt.get_vertex_normal(b)).normalized()
	_mdt.set_vertex_normal(a, normal)
	_mdt.set_vertex_normal(b, normal)

	var tangent = (_mdt.get_vertex_tangent(a).normal + _mdt.get_vertex_tangent(b).normal).normalized()
	## octonormalize
	tangent = normal.cross(tangent).cross(normal)
	var plane = Plane(tangent, 1.0)
	_mdt.set_vertex_tangent(a, plane)
	_mdt.set_vertex_tangent(b, plane)


func subdivide(mesh_instance: MeshInstance):
	print("todo")
	#if mesh_instance.get_child_count() > 1:
	#	var conv = ConvexPolygonShape.new()
		#var xf = _prepare_child(mesh_instance, 0)
	#	_add2points(xf, conv, depth, Vector2.ZERO, Vector2(0, 1))
	#	var vx_id = 0
	#	var last_id = mesh_instance.get_child_count() - 1
		
		
		
		#for child_id in range(1, last_id):
