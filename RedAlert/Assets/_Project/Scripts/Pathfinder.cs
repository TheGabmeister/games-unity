using UnityEngine;
using System.Collections.Generic;

public static class Pathfinder
{
    private static readonly Vector2Int[] Directions =
    {
        new(0, 1), new(1, 1), new(1, 0), new(1, -1),
        new(0, -1), new(-1, -1), new(-1, 0), new(-1, 1)
    };

    private static readonly float[] DirectionCosts =
    {
        1f, 1.414f, 1f, 1.414f,
        1f, 1.414f, 1f, 1.414f
    };

    public static List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal, LocomotionType locomotion)
    {
        if (start == goal) return null;

        var map = MapManager.Instance;
        if (!map.IsInBounds(goal)) return null;
        if (!TerrainMovement.IsPassable(locomotion, map.GetTerrain(goal))) return null;

        var openSet = new List<Vector2Int> { start };
        var closedSet = new HashSet<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, float> { [start] = 0f };
        var fScore = new Dictionary<Vector2Int, float> { [start] = Heuristic(start, goal) };

        while (openSet.Count > 0)
        {
            Vector2Int current = GetLowestF(openSet, fScore);

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            openSet.Remove(current);
            closedSet.Add(current);

            for (int i = 0; i < 8; i++)
            {
                Vector2Int neighbor = current + Directions[i];

                if (closedSet.Contains(neighbor)) continue;
                if (!map.IsInBounds(neighbor)) continue;

                float speedMult = TerrainMovement.GetSpeedMultiplier(locomotion, map.GetTerrain(neighbor));
                if (speedMult <= 0f) continue;

                if (IsDiagonal(i) && !CanCutCorner(current, i, locomotion))
                    continue;

                if (neighbor != goal && map.GetEntityAt(neighbor) != null)
                    continue;

                float moveCost = DirectionCosts[i] / speedMult;
                float tentativeG = gScore[current] + moveCost;

                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, goal);

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        return null;
    }

    static float Heuristic(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return Mathf.Max(dx, dy) + (1.414f - 1f) * Mathf.Min(dx, dy);
    }

    static Vector2Int GetLowestF(List<Vector2Int> openSet, Dictionary<Vector2Int, float> fScore)
    {
        Vector2Int best = openSet[0];
        float bestF = fScore.GetValueOrDefault(best, float.MaxValue);

        for (int i = 1; i < openSet.Count; i++)
        {
            float f = fScore.GetValueOrDefault(openSet[i], float.MaxValue);
            if (f < bestF)
            {
                bestF = f;
                best = openSet[i];
            }
        }

        return best;
    }

    static bool IsDiagonal(int dirIndex) => dirIndex % 2 == 1;

    static bool CanCutCorner(Vector2Int from, int diagIndex, LocomotionType locomotion)
    {
        int cardA = (diagIndex + 7) % 8;
        int cardB = (diagIndex + 1) % 8;

        Vector2Int cellA = from + Directions[cardA];
        Vector2Int cellB = from + Directions[cardB];

        var map = MapManager.Instance;
        return map.IsInBounds(cellA)
            && map.IsInBounds(cellB)
            && TerrainMovement.IsPassable(locomotion, map.GetTerrain(cellA))
            && TerrainMovement.IsPassable(locomotion, map.GetTerrain(cellB));
    }

    static List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        var path = new List<Vector2Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }
        path.Reverse();
        path.RemoveAt(0);
        return path;
    }
}
