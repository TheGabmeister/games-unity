using UnityEngine;
using System.Collections.Generic;

public static class Pathfinder
{
    private struct MinHeap
    {
        private List<(Vector2Int cell, float priority)> _data;

        public int Count => _data.Count;

        public MinHeap(int capacity)
        {
            _data = new List<(Vector2Int, float)>(capacity);
        }

        public void Enqueue(Vector2Int cell, float priority)
        {
            _data.Add((cell, priority));
            BubbleUp(_data.Count - 1);
        }

        public Vector2Int Dequeue()
        {
            var result = _data[0].cell;
            int last = _data.Count - 1;
            _data[0] = _data[last];
            _data.RemoveAt(last);
            if (_data.Count > 0) BubbleDown(0);
            return result;
        }

        private void BubbleUp(int i)
        {
            while (i > 0)
            {
                int parent = (i - 1) / 2;
                if (_data[i].priority < _data[parent].priority)
                {
                    (_data[i], _data[parent]) = (_data[parent], _data[i]);
                    i = parent;
                }
                else break;
            }
        }

        private void BubbleDown(int i)
        {
            int count = _data.Count;
            while (true)
            {
                int smallest = i;
                int left = 2 * i + 1;
                int right = 2 * i + 2;
                if (left < count && _data[left].priority < _data[smallest].priority)
                    smallest = left;
                if (right < count && _data[right].priority < _data[smallest].priority)
                    smallest = right;
                if (smallest == i) break;
                (_data[i], _data[smallest]) = (_data[smallest], _data[i]);
                i = smallest;
            }
        }
    }
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

        var openQueue = new MinHeap(64);
        var closedSet = new HashSet<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, float> { [start] = 0f };

        openQueue.Enqueue(start, Heuristic(start, goal));

        while (openQueue.Count > 0)
        {
            Vector2Int current = openQueue.Dequeue();

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            if (!closedSet.Add(current)) continue;

            float currentG = gScore[current];

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
                float tentativeG = currentG + moveCost;

                if (!gScore.TryGetValue(neighbor, out float existingG) || tentativeG < existingG)
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    openQueue.Enqueue(neighbor, tentativeG + Heuristic(neighbor, goal));
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
