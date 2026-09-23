using Godot;
using System.Collections.Generic;

// 近战命中查询：以攻击者为顶点先做一次球形查询取候选，再用水平扇形（左右半角）过滤，
// 最后逐个打一条视线射线，被墙挡住的候选不算命中。
public static class MeleeSwing
{
    private const float CHEST_HEIGHT = 1f;
    private const int MAX_CANDIDATES = 32;

    private static readonly SphereShape3D _reachSphere = new();

    public static void CollectTargets(
        CollisionObject3D attacker,
        Vector3 forward,
        float reach,
        float halfAngleDegrees,
        uint targetMask,
        List<Node3D> results)
    {
        results.Clear();

        var forwardXZ = new Vector3(forward.X, 0f, forward.Z);
        if (reach <= 0f || forwardXZ.IsZeroApprox())
        {
            return;
        }
        forwardXZ = forwardXZ.Normalized();

        var origin = attacker.GlobalPosition + Vector3.Up * CHEST_HEIGHT;
        var excluded = new Godot.Collections.Array<Rid> { attacker.GetRid() };
        var space = attacker.GetWorld3D().DirectSpaceState;

        _reachSphere.Radius = reach;

        var candidates = space.IntersectShape(new PhysicsShapeQueryParameters3D
        {
            Shape = _reachSphere,
            Transform = new Transform3D(Basis.Identity, origin),
            CollisionMask = targetMask,
            CollideWithBodies = true,
            CollideWithAreas = false,
            Exclude = excluded,
        }, MAX_CANDIDATES);

        var cosHalfAngle = Mathf.Cos(Mathf.DegToRad(halfAngleDegrees));
        var reachSquared = reach * reach;

        foreach (var candidate in candidates)
        {
            if (candidate["collider"].As<Node3D>() is not Node3D target || results.Contains(target))
            {
                continue;
            }

            var offsetXZ = target.GlobalPosition - origin;
            offsetXZ.Y = 0f;

            if (offsetXZ.LengthSquared() > reachSquared || forwardXZ.Dot(offsetXZ.Normalized()) < cosHalfAngle)
            {
                continue;
            }

            if (IsBlocked(space, origin, target, targetMask, excluded))
            {
                continue;
            }

            results.Add(target);
        }
    }

    private static bool IsBlocked(
        PhysicsDirectSpaceState3D space,
        Vector3 origin,
        Node3D target,
        uint targetMask,
        Godot.Collections.Array<Rid> excluded)
    {
        var query = PhysicsRayQueryParameters3D.Create(
            origin,
            target.GlobalPosition + Vector3.Up * CHEST_HEIGHT,
            targetMask | PhysicsLayers.World);
        query.Exclude = excluded;

        var hit = space.IntersectRay(query);
        return hit.Count > 0 && hit["collider"].As<Node3D>() != target;
    }
}

// 物理层，顺序与 project.godot 的 layer_names 一致
public static class PhysicsLayers
{
    public const uint World = 1u << 0;
    public const uint Player = 1u << 1;
    public const uint Enemy = 1u << 2;
    public const uint PickableItems = 1u << 3;
    public const uint Projectiles = 1u << 4;
    public const uint Door = 1u << 5;
}
