using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class DependencyGraphLayout
{
    const float NodeHeight = 60f;
    const float HSpacing = 50f;
    const float VSpacing = 40f;
    const int ForceIterations = 150;
    const float RepulsionStrength = 8000f;
    const float AttractionStrength = 0.05f;
    const float Damping = 0.9f;

    public static void ApplyLayout(DependencyGraph graph, float startX, float startY)
    {
        if (graph.Nodes.Count == 0) return;

        var reverseDeps = BuildReverseDeps(graph);
        var layers = ComputeCompactLayers(graph, reverseDeps);
        var layerGroups = GroupByLayer(graph, layers);
        AssignInitialPositions(layerGroups, startX, startY);
        RunForceDirectedX(graph, reverseDeps, layerGroups);
        CenterHorizontally(graph, startX);
    }

    static Dictionary<string, List<string>> BuildReverseDeps(DependencyGraph graph)
    {
        var reverse = new Dictionary<string, List<string>>();
        foreach (var node in graph.Nodes)
        {
            foreach (var dep in node.DependsOn)
            {
                if (!reverse.TryGetValue(dep, out var list))
                {
                    list = new List<string>();
                    reverse[dep] = list;
                }
                list.Add(node.TypeName);
            }
        }
        return reverse;
    }

    static Dictionary<int, List<ScriptNode>> GroupByLayer(DependencyGraph graph, Dictionary<string, int> layers)
    {
        var groups = new Dictionary<int, List<ScriptNode>>();
        foreach (var node in graph.Nodes)
        {
            int layer = layers.TryGetValue(node.TypeName, out var l) ? l : 0;
            if (!groups.TryGetValue(layer, out var list))
            {
                list = new List<ScriptNode>();
                groups[layer] = list;
            }
            list.Add(node);
        }
        return groups;
    }

    #region Compact Layer Assignment

    static Dictionary<string, int> ComputeCompactLayers(DependencyGraph graph,
        Dictionary<string, List<string>> reverseDeps)
    {
        var layers = new Dictionary<string, int>();
        var visited = new HashSet<string>();
        var inStack = new HashSet<string>();

        foreach (var node in graph.Nodes)
            GetDepth(node.TypeName, graph, layers, visited, inStack);

        if (layers.Count == 0) return layers;

        int maxLayer = layers.Values.Max();
        foreach (var key in layers.Keys.ToList())
            layers[key] = maxLayer - layers[key];

        CompactLayers(graph, layers, reverseDeps);

        return layers;
    }

    static int GetDepth(string typeName, DependencyGraph graph, Dictionary<string, int> layers,
        HashSet<string> visited, HashSet<string> inStack)
    {
        if (visited.Contains(typeName))
            return layers.TryGetValue(typeName, out var l) ? l : 0;

        if (inStack.Contains(typeName))
            return 0;

        if (!graph.NodesByType.TryGetValue(typeName, out var node))
            return 0;

        inStack.Add(typeName);

        int maxDepLayer = -1;
        foreach (var dep in node.DependsOn)
        {
            if (graph.NodesByType.ContainsKey(dep))
            {
                int depLayer = GetDepth(dep, graph, layers, visited, inStack);
                if (depLayer > maxDepLayer)
                    maxDepLayer = depLayer;
            }
        }

        inStack.Remove(typeName);

        int layer = maxDepLayer + 1;
        layers[typeName] = layer;
        visited.Add(typeName);
        return layer;
    }

    static void CompactLayers(DependencyGraph graph, Dictionary<string, int> layers,
        Dictionary<string, List<string>> reverseDeps)
    {
        bool changed = true;
        while (changed)
        {
            changed = false;
            foreach (var node in graph.Nodes)
            {
                int currentLayer = layers[node.TypeName];
                if (currentLayer == 0) continue;

                int minAllowed = 0;
                if (reverseDeps.TryGetValue(node.TypeName, out var parents))
                {
                    foreach (var parentName in parents)
                    {
                        int parentLayer = layers[parentName];
                        if (parentLayer + 1 > minAllowed)
                            minAllowed = parentLayer + 1;
                    }
                }

                int maxAllowed = currentLayer;
                foreach (var dep in node.DependsOn)
                {
                    if (!layers.TryGetValue(dep, out int depLayer)) continue;
                    if (depLayer - 1 < maxAllowed)
                        maxAllowed = depLayer - 1;
                }

                int target = Mathf.Max(minAllowed, 0);
                if (target > maxAllowed) continue;

                if (target < currentLayer)
                {
                    layers[node.TypeName] = target;
                    changed = true;
                }
            }
        }
    }

    #endregion

    #region Initial Position + Force-Directed X

    static void AssignInitialPositions(Dictionary<int, List<ScriptNode>> layerGroups,
        float startX, float startY)
    {
        foreach (var kvp in layerGroups)
        {
            var nodesInLayer = kvp.Value;
            nodesInLayer.Sort((a, b) => string.Compare(a.TypeName, b.TypeName));

            float y = startY + kvp.Key * (NodeHeight + VSpacing);
            float totalWidth = 0f;
            foreach (var n in nodesInLayer)
                totalWidth += ComputeNodeWidth(n.TypeName) + HSpacing;
            totalWidth -= HSpacing;

            float x = startX - totalWidth / 2f;
            foreach (var n in nodesInLayer)
            {
                float nw = ComputeNodeWidth(n.TypeName);
                n.Rect = new Rect(x, y, nw, NodeHeight);
                x += nw + HSpacing;
            }
        }
    }

    static void RunForceDirectedX(DependencyGraph graph,
        Dictionary<string, List<string>> reverseDeps,
        Dictionary<int, List<ScriptNode>> layerGroups)
    {
        var velocities = new Dictionary<string, float>();
        var forces = new Dictionary<string, float>();
        foreach (var node in graph.Nodes)
        {
            velocities[node.TypeName] = 0f;
            forces[node.TypeName] = 0f;
        }

        for (int iter = 0; iter < ForceIterations; iter++)
        {
            foreach (var key in forces.Keys.ToList())
                forces[key] = 0f;

            foreach (var kvp in layerGroups)
            {
                var nodesInLayer = kvp.Value;
                for (int i = 0; i < nodesInLayer.Count; i++)
                {
                    for (int j = i + 1; j < nodesInLayer.Count; j++)
                    {
                        var a = nodesInLayer[i];
                        var b = nodesInLayer[j];
                        float cx = a.Rect.center.x - b.Rect.center.x;
                        float minDist = (a.Rect.width + b.Rect.width) / 2f + HSpacing;
                        float dist = Mathf.Max(Mathf.Abs(cx), 1f);

                        float repulsion = RepulsionStrength / (dist * dist);
                        float sign = cx >= 0 ? 1f : -1f;

                        forces[a.TypeName] += sign * repulsion;
                        forces[b.TypeName] -= sign * repulsion;

                        if (Mathf.Abs(cx) < minDist)
                        {
                            float overlap = minDist - Mathf.Abs(cx);
                            forces[a.TypeName] += sign * overlap * 0.5f;
                            forces[b.TypeName] -= sign * overlap * 0.5f;
                        }
                    }
                }
            }

            foreach (var node in graph.Nodes)
            {
                float targetX = 0f;
                int connections = 0;

                foreach (var dep in node.DependsOn)
                {
                    if (graph.NodesByType.TryGetValue(dep, out var target))
                    {
                        targetX += target.Rect.center.x;
                        connections++;
                    }
                }

                if (reverseDeps.TryGetValue(node.TypeName, out var parents))
                {
                    foreach (var parentName in parents)
                    {
                        if (graph.NodesByType.TryGetValue(parentName, out var parent))
                        {
                            targetX += parent.Rect.center.x;
                            connections++;
                        }
                    }
                }

                if (connections > 0)
                {
                    targetX /= connections;
                    float dx = targetX - node.Rect.center.x;
                    forces[node.TypeName] += dx * AttractionStrength;
                }
            }

            foreach (var node in graph.Nodes)
            {
                float v = velocities[node.TypeName] + forces[node.TypeName];
                v *= Damping;
                velocities[node.TypeName] = v;

                var r = node.Rect;
                r.x += v;
                node.Rect = r;
            }
        }
    }

    #endregion

    #region Centering

    static void CenterHorizontally(DependencyGraph graph, float startX)
    {
        if (graph.Nodes.Count == 0) return;

        float minX = float.MaxValue, maxX = float.MinValue;
        foreach (var node in graph.Nodes)
        {
            if (node.Rect.x < minX) minX = node.Rect.x;
            if (node.Rect.xMax > maxX) maxX = node.Rect.xMax;
        }

        float offset = startX - (minX + maxX) / 2f;
        foreach (var node in graph.Nodes)
        {
            var r = node.Rect;
            r.x += offset;
            node.Rect = r;
        }
    }

    public static float ComputeNodeWidth(string typeName)
    {
        float minWidth = 150f;
        float charWidth = 8f;
        float nameWidth = typeName.Length * charWidth + 40f;
        return Mathf.Max(minWidth, nameWidth);
    }

    #endregion
}
