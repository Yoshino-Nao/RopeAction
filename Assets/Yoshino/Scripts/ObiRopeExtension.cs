using Obi;
using UnityEngine;

public static class ObiRopeExtension
{
    /// <summary>
    /// targetWorldPositionに一番近いObiRope中のパーティクルのindexを取得します。
    /// </summary>
    /// <param name="rope"></param>
    /// <param name="targetWorldPosition"></param>
    /// <param name="outParticleIndex"></param>
    /// <returns></returns>
    public static bool TryGetNearestParticleIndex(this ObiRope rope, Vector3 targetWorldPosition, out int outParticleIndex)
    {
        var distance = 10000f;
        var targetIndex = -1;
        foreach (var particleIndex in rope.solver.simplices)
        {
            var particlePos = GetParticleWorldPosition(rope, particleIndex);
            var currentDistance = Vector3.Distance(particlePos, targetWorldPosition);
            if (currentDistance < distance)
            {
                distance = currentDistance;
                targetIndex = particleIndex;
            }
        }

        if (targetIndex == -1)
        {
            outParticleIndex = -1;
            return false;
        }

        outParticleIndex = targetIndex;
        return true;
    }

    public static Vector3 GetParticleWorldPosition(this ObiRope rope, int particleIndex)
    {
        var solver = rope.solver;
        Matrix4x4 solver2World = solver.transform.localToWorldMatrix;
        return solver2World.MultiplyPoint3x4(solver.positions[particleIndex]);
    }

    /// <summary>
    /// projectionWorldTarget座標に一番近いRope上の座標を取得します。
    /// </summary>
    /// <param name="rope"></param>
    /// <param name="projectionWorldTarget"></param>
    /// <param name="mostCloseParticleIndex"></param>
    /// <param name="solver"></param>
    /// <param name="outPos"></param>
    /// <param name="outDirection"></param>
    /// <returns></returns>
    public static bool TryGetRopeProjectionPosition(this ObiRope rope, Vector3 projectionWorldTarget, int mostCloseParticleIndex, ObiSolver solver, out Vector3 outPos, out Vector3 outDirection)
    {
        Matrix4x4 solver2World = solver.transform.localToWorldMatrix;

        if (rope.TryFindElement(mostCloseParticleIndex, out var outElement))
        {
            var currentIndex = outElement.particle2;
            var nextIndex = outElement.particle1;

            var currentParticlePos = solver2World.MultiplyPoint3x4(solver.positions[currentIndex]);
            var nextParticlePos = solver2World.MultiplyPoint3x4(solver.positions[nextIndex]);
            outPos = ObiUtils.ProjectPointLine(projectionWorldTarget, currentParticlePos, nextParticlePos, out var mu, false);
            outDirection = (nextParticlePos - currentParticlePos).normalized;
        }
        else
        {
            outPos = solver2World.MultiplyPoint3x4(solver.positions[mostCloseParticleIndex]);
            outDirection = Vector3.up;
        }
        return true;
    }


    private static bool TryFindElement(this ObiRope rope, int index, out ObiStructuralElement element)
    {
        foreach (var one in rope.elements)
        {
            if (one.particle1 == index)
            {
                element = one;
                return true;
            }
        }
        element = null;
        return false;
    }
}

