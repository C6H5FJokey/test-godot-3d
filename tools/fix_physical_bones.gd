## 物理骨骼几何修复工具（EditorScript，只在编辑器里跑）
##
## 什么时候用：从 Blender 重新导入模型之后，骨骼静止姿变了，但 PhysicalBone3D 上的
## body_offset / joint_offset 还是按旧静止姿烘焙的老数值。布娃娃一开始模拟，物理体就会
## 落到错误的位置（腿、手臂看着像"下沉"）。
##
## 关键点：真正决定布娃娃落点的不是节点自己的 transform（那只是骨骼姿态的显示镜像），
## 而是 body_offset：
##     物理体世界变换 = skeleton.global_transform * 骨骼全局姿态 * body_offset
## 所以"临时新建一个物理骨骼、把它 Position 抄到老骨骼上再删掉"只改了 transform，
## 运行时不会生效。本脚本重算的是真正起作用的那三个字段。
##
## 做法：按当前骨骼静止姿重算 transform / body_offset / joint_offset / 胶囊尺寸，
## 结果等价于编辑器 "Create Physical Bones" 在同一个骨架上算出来的几何（已逐骨骼核对）。
## 但 joint_type 与 joint_constraints 是你手工调过的参数，这里保留不动 —— 直接重新生成
## 物理骨骼会把这些调参全部丢掉。
##
## 用法：
##   1. 打开目标场景（可选：先选中 Goblin 或它的 Skeleton3D 来限定处理范围）
##   2. 在脚本编辑器里打开本文件，按 File > Run（Ctrl+Shift+X）
##   3. 按 Ctrl+S 保存场景；改坏了按 Ctrl+Z 整体撤销
##
## 可以重复跑：算出来的是同一组数值，第二次跑会直接报"全部已是最新"。
## 约定：骨骼长度取自"第一个子骨骼"的静止姿位置（和编辑器的生成器一致）；没有子骨骼的
## 骨骼推不出长度，会被跳过并给出警告。

# 编辑器的 File > Run 只肯执行 tool 脚本，缺了 @tool 会直接报
# "Cannot run the script because it's not a tool script"。
@tool
extends EditorScript

## 只打印将要发生的修改，不动场景。
@export var dry_run := false

## 同时把 CollisionShape3D 的胶囊改成 半径 = 长度 / 10、高度 = 长度（编辑器生成器的取值）。
## 如果你手工调过胶囊半径，把它关掉。
@export var fix_capsules := true

const CAPSULE_RADIUS_RATIO := 0.1
const MIN_BONE_LENGTH := 0.0001

# CapsuleShape3D 的轴是它自己的 +Y，经 CollisionShape3D 的旋转后落到物理体的 -Z，
# 所以 body_offset 的基要满足 basis * BODY_AXIS == 骨骼方向（骨骼静止姿下的本地朝向）。
const BODY_AXIS := Vector3(0.0, 0.0, -1.0)


## 纯计算：给出 simulator 下每个物理骨骼应有的几何，不碰任何节点。
static func compute_plan(skeleton: Skeleton3D, simulator: PhysicalBoneSimulator3D) -> Array[Dictionary]:
	var plan: Array[Dictionary] = []
	for child in simulator.get_children():
		if not (child is PhysicalBone3D):
			continue
		var bone := child as PhysicalBone3D
		var bone_id := bone.get_bone_id()
		if bone_id < 0:
			push_warning("[fix_physical_bones] %s 的 bone_name 在本骨架里找不到，跳过。" % bone.name)
			continue
		var children := skeleton.get_bone_children(bone_id)
		if children.is_empty():
			push_warning("[fix_physical_bones] 骨骼 %s 没有子骨骼，推不出长度，跳过。" % skeleton.get_bone_name(bone_id))
			continue
		var head := skeleton.get_bone_global_rest(bone_id)
		var tip := skeleton.get_bone_global_rest(children[0]).origin - head.origin
		var length := tip.length()
		if length < MIN_BONE_LENGTH:
			push_warning("[fix_physical_bones] 骨骼 %s 长度为 0，跳过。" % skeleton.get_bone_name(bone_id))
			continue
		var axis := (head.basis.inverse() * tip).normalized()
		var body_offset := Transform3D(Basis(Quaternion(BODY_AXIS, axis)), axis * (length * 0.5))
		plan.append({
			"bone": bone,
			"bone_name": skeleton.get_bone_name(bone_id),
			"length": length,
			"transform": head * body_offset,
			"body_offset": body_offset,
			"joint_offset": Transform3D(Basis(), Vector3(0.0, 0.0, length * 0.5)),
			"radius": length * CAPSULE_RADIUS_RATIO,
			"capsule": _find_capsule(bone),
		})
	return plan


static func _find_capsule(bone: PhysicalBone3D) -> CapsuleShape3D:
	for child in bone.get_children():
		if child is CollisionShape3D and child.shape is CapsuleShape3D:
			return child.shape
	return null


func _run() -> void:
	var root := _target_root()
	if root == null:
		push_error("[fix_physical_bones] 没有打开的场景，也没有选中的节点。")
		return

	var skeletons: Array[Node] = []
	_collect(root, "Skeleton3D", skeletons)
	if skeletons.is_empty():
		push_error("[fix_physical_bones] %s 里没有 Skeleton3D。" % root.name)
		return

	var changes: Array[Dictionary] = []
	for node in skeletons:
		var skeleton := node as Skeleton3D
		var simulators: Array[Node] = []
		for child in skeleton.get_children():
			if child is PhysicalBoneSimulator3D:
				simulators.append(child)
		if simulators.size() > 1:
			push_warning("[fix_physical_bones] %s 下有 %d 个物理骨骼模拟器，建议只留游戏真正使用的那个（这里会全部修）。" % [skeleton.name, simulators.size()])
		for item in simulators:
			var simulator := item as PhysicalBoneSimulator3D
			var plan := compute_plan(skeleton, simulator)
			print("[fix_physical_bones] %s > %s：%d 个物理骨骼" % [skeleton.get_path(), simulator.name, plan.size()])
			for fix in plan:
				_collect_changes(fix, changes)
				print("    %-14s 长度 %.6f  节点位移 %.6f  %s" % [
					fix["bone_name"],
					fix["length"],
					(fix["bone"] as PhysicalBone3D).transform.origin.distance_to((fix["transform"] as Transform3D).origin),
					_capsule_note(fix),
				])

	if changes.is_empty():
		print("[fix_physical_bones] 全部已是最新，无需修改。")
		return
	if dry_run:
		print("[fix_physical_bones] dry_run：本应修改 %d 个属性，未写入。" % changes.size())
		return

	var undo := EditorInterface.get_editor_undo_redo()
	undo.create_action("修复物理骨骼几何")
	for change in changes:
		undo.add_do_property(change["object"], change["property"], change["value"])
		undo.add_undo_property(change["object"], change["property"], change["old"])
	undo.commit_action()

	EditorInterface.mark_scene_as_unsaved()
	print("[fix_physical_bones] 已修改 %d 个属性，Ctrl+S 保存场景，Ctrl+Z 可撤销。" % changes.size())


func _target_root() -> Node:
	for node in EditorInterface.get_selection().get_selected_nodes():
		if _has_skeleton(node):
			return node
	return EditorInterface.get_edited_scene_root()


func _has_skeleton(node: Node) -> bool:
	var skeletons: Array[Node] = []
	_collect(node, "Skeleton3D", skeletons)
	return not skeletons.is_empty()


func _collect(node: Node, type_name: String, out: Array[Node]) -> void:
	if node.is_class(type_name):
		out.append(node)
	for child in node.get_children():
		_collect(child, type_name, out)


func _capsule_note(fix: Dictionary) -> String:
	var capsule := fix["capsule"] as CapsuleShape3D
	if capsule == null:
		return "无胶囊，跳过"
	if _same(capsule.height, fix["length"]) and _same(capsule.radius, fix["radius"]):
		return "胶囊已正确"
	return "胶囊 %.6f/%.6f -> %.6f/%.6f" % [capsule.height, capsule.radius, fix["length"], fix["radius"]]


func _collect_changes(fix: Dictionary, out: Array[Dictionary]) -> void:
	var bone := fix["bone"] as PhysicalBone3D
	_add_change(out, bone, "transform", fix["transform"])
	_add_change(out, bone, "body_offset", fix["body_offset"])
	_add_change(out, bone, "joint_offset", fix["joint_offset"])
	if not fix_capsules:
		return
	var capsule := fix["capsule"] as CapsuleShape3D
	if capsule == null:
		return
	_add_change(out, capsule, "height", fix["length"])
	_add_change(out, capsule, "radius", fix["radius"])


func _add_change(out: Array[Dictionary], object: Object, property: StringName, value: Variant) -> void:
	var old: Variant = object.get(property)
	if _same(old, value):
		return
	out.append({"object": object, "property": property, "value": value, "old": old})


static func _same(a: Variant, b: Variant) -> bool:
	if a is Transform3D:
		return (a as Transform3D).is_equal_approx(b as Transform3D)
	if a is float:
		return is_equal_approx(a as float, b as float)
	return a == b
