extends Node

class_name XTS

static func get_local_scale(b: Basis) -> Vector3:
	return Vector3(b.x.length(), b.y.length(), b.z.length())

static func  mult_local_scale(b: Basis, scale: Vector3) -> Basis:
	b.x *= scale.x
	b.y *= scale.y
	b.z *= scale.z
	return b

static func is_between(t: float, a: float, b: float) -> bool:
	return t >= a and t <= b

static func up_align(basis: Basis, up_dir: Vector3) -> Basis:
	var scale = get_local_scale(basis)
	var SIN45 = 0.707106781186548
	# Z = [X * Y]
	# X = [Y * Z]
	if (is_between(basis.x.dot(up_dir), -SIN45, SIN45)):
		basis.z = basis.x.cross(up_dir).normalized()
		basis.x = up_dir.cross(basis.z)
	else:
		basis.x = up_dir.cross(basis.z).normalized()
		basis.z = basis.x.cross(up_dir)
	basis.y = up_dir
	#print($"basis {basis.ToString()}")
	basis = mult_local_scale(basis, scale)
	return basis

static func x_align(basis: Basis, up_dir: Vector3) -> Basis:
	var scale = get_local_scale(basis)
	var z = basis.z.normalized()
	if is_between(z.dot(up_dir), -0.9999, 0.9999):
		basis.x = up_dir.cross(z).normalized()
		basis.y = z.cross(basis.x)
		basis.z = z
		basis = mult_local_scale(basis, scale)
	return basis

static func z_align(basis: Basis, up_dir: Vector3) -> Basis:
	var scale = get_local_scale(basis)
	print(basis)
	var x = basis.x.normalized()
	if is_between(x.dot(up_dir), -0.9999, 0.9999):
		basis.z = x.cross(up_dir).normalized()
		basis.y = basis.z.cross(x)
		basis.x = x
		basis = mult_local_scale(basis, scale)
	return basis


static func up_align_xf(xf: Transform) -> Transform:
	xf.basis = up_align(xf.basis, xf.origin.normalized())
	return xf

static func x_align_xf(xf: Transform) -> Transform:
	xf.basis = x_align(xf.basis, xf.origin.normalized())
	return xf

static func z_align_xf(xf: Transform) -> Transform:
	xf.basis = z_align(xf.basis, xf.origin.normalized())
	return xf

static func up_align_node(node: Spatial) -> void:
	node.global_transform = up_align_xf(node.global_transform)

static func x_align_node(node: Spatial) -> void:
	node.global_transform = x_align_xf(node.global_transform)

static func z_align_node(node: Spatial) -> void:
	node.global_transform = z_align_xf(node.global_transform)
