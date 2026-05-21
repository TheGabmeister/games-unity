using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

public class ScriptNode
{
    public string TypeName;
    public string FilePath;
    public UnityEngine.Rect Rect;
    public List<string> DependsOn = new();
}

public class DependencyGraph
{
    public List<ScriptNode> Nodes = new();
    public Dictionary<string, ScriptNode> NodesByType = new();
}

public static class DependencyScanner
{
    static readonly Regex TypeDeclRegex = new(
        @"(?:class|struct|interface|enum)\s+(\w+)",
        RegexOptions.Compiled);

    static readonly Regex InheritanceRegex = new(
        @"(?:class|struct)\s+\w+(?:<[^>]+>)?\s*:\s*([\w\s,<>]+?)(?:\s*\{|\s*where)",
        RegexOptions.Compiled);

    static readonly Regex FieldRegex = new(
        @"(?:public|private|protected|internal|static|readonly|const|volatile|new)\s+(\w+)(?:<[\w\s,<>]+>)?\s+\w+\s*[;=,)]",
        RegexOptions.Compiled);

    static readonly Regex SimpleFieldRegex = new(
        @"^\s+(\w+)\s+\w+\s*[;=]",
        RegexOptions.Compiled | RegexOptions.Multiline);

    static readonly Regex GenericArgRegex = new(
        @"<\s*(\w+)(?:\s*,\s*(\w+))?(?:\s*,\s*(\w+))?\s*>",
        RegexOptions.Compiled);

    static readonly Regex MethodReturnRegex = new(
        @"(?:public|private|protected|internal|static|virtual|override|abstract|async|new)\s+(?:(?:public|private|protected|internal|static|virtual|override|abstract|async|new)\s+)*(\w+)(?:<[^>]+>)?\s+\w+\s*\(",
        RegexOptions.Compiled);

    static readonly Regex SimpleMethodRegex = new(
        @"^\s+(\w+)\s+\w+\s*\(",
        RegexOptions.Compiled | RegexOptions.Multiline);

    static readonly Regex ParameterRegex = new(
        @"(?:[\(,])\s*(?:(?:ref|out|in|params|this)\s+)?(\w+)(?:<[^>]+>)?\s+\w+",
        RegexOptions.Compiled);

    static readonly Regex NewExprRegex = new(
        @"\bnew\s+(\w+)\s*[\(\{<]",
        RegexOptions.Compiled);

    static readonly Regex TypeofRegex = new(
        @"\btypeof\s*\(\s*(\w+)\s*\)",
        RegexOptions.Compiled);

    static readonly Regex IsAsRegex = new(
        @"\b(?:is|as)\s+(\w+)\b",
        RegexOptions.Compiled);

    static readonly Regex CastRegex = new(
        @"\(\s*(\w+)\s*\)\s*\w",
        RegexOptions.Compiled);

    static readonly Regex GetComponentRegex = new(
        @"GetComponent(?:InChildren|InParent|s)?<\s*(\w+)\s*>",
        RegexOptions.Compiled);

    static readonly Regex SingleLineCommentRegex = new(@"//.*$", RegexOptions.Compiled | RegexOptions.Multiline);
    static readonly Regex MultiLineCommentRegex = new(@"/\*[\s\S]*?\*/", RegexOptions.Compiled);
    static readonly Regex VerbatimStringRegex = new(@"@""(?:[^""]|"""")*""", RegexOptions.Compiled);
    static readonly Regex StringLiteralRegex = new(@"""(?:[^""\\]|\\.)*""", RegexOptions.Compiled);
    static readonly Regex UsingDirectiveRegex = new(@"^\s*using\s+[\w.]+\s*;\s*$", RegexOptions.Compiled | RegexOptions.Multiline);
    static readonly Regex AttributeRegex = new(@"\[[\w\s,()""\.=]+\]", RegexOptions.Compiled);

    static readonly HashSet<string> CSharpKeywords = new()
    {
        "void", "int", "float", "double", "string", "bool", "byte", "char",
        "short", "long", "uint", "ulong", "ushort", "sbyte", "decimal",
        "object", "var", "dynamic", "class", "struct", "interface", "enum",
        "if", "else", "for", "foreach", "while", "do", "switch", "case",
        "return", "break", "continue", "new", "null", "true", "false",
        "this", "base", "public", "private", "protected", "internal",
        "static", "readonly", "const", "virtual", "override", "abstract",
        "sealed", "async", "await", "yield", "try", "catch", "finally",
        "throw", "using", "namespace", "delegate", "event", "ref", "out",
        "in", "params", "is", "as", "typeof", "sizeof", "default",
        "where", "get", "set", "value", "partial", "volatile"
    };

    public static DependencyGraph Scan(string rootPath = "Assets/_Project/Scripts")
    {
        var graph = new DependencyGraph();

        if (!Directory.Exists(rootPath))
            return graph;

        var files = Directory.GetFiles(rootPath, "*.cs", SearchOption.AllDirectories)
            .Where(f =>
            {
                var normalized = f.Replace('\\', '/');
                return !normalized.Contains("/Editor/");
            })
            .ToArray();

        var fileContents = new Dictionary<string, string>();
        var projectTypes = new HashSet<string>();

        foreach (var file in files)
        {
            var normalized = file.Replace('\\', '/');
            var content = File.ReadAllText(file);
            fileContents[normalized] = content;

            var match = TypeDeclRegex.Match(content);
            if (!match.Success) continue;

            var typeName = match.Groups[1].Value;
            if (projectTypes.Contains(typeName)) continue;

            projectTypes.Add(typeName);
            var node = new ScriptNode
            {
                TypeName = typeName,
                FilePath = file.Replace('\\', '/')
            };
            graph.Nodes.Add(node);
            graph.NodesByType[typeName] = node;
        }

        foreach (var node in graph.Nodes)
        {
            var content = fileContents[node.FilePath];
            var cleaned = StripNoise(content);
            var deps = FindDependencies(cleaned, node.TypeName, projectTypes);
            node.DependsOn = deps.ToList();
        }

        return graph;
    }

    static string StripNoise(string source)
    {
        source = SingleLineCommentRegex.Replace(source, "");
        source = MultiLineCommentRegex.Replace(source, "");
        source = VerbatimStringRegex.Replace(source, "\"\"");
        source = StringLiteralRegex.Replace(source, "\"\"");
        source = UsingDirectiveRegex.Replace(source, "");
        source = AttributeRegex.Replace(source, "");
        return source;
    }

    static HashSet<string> FindDependencies(string content, string ownType, HashSet<string> projectTypes)
    {
        var deps = new HashSet<string>();

        void TryAdd(string candidate)
        {
            if (string.IsNullOrEmpty(candidate)) return;
            if (candidate == ownType) return;
            if (CSharpKeywords.Contains(candidate)) return;
            if (projectTypes.Contains(candidate))
                deps.Add(candidate);
        }

        foreach (Match m in InheritanceRegex.Matches(content))
        {
            var bases = m.Groups[1].Value.Split(',');
            foreach (var b in bases)
            {
                var trimmed = b.Trim().Split('<')[0].Trim();
                TryAdd(trimmed);
            }
        }

        foreach (Match m in FieldRegex.Matches(content))
            TryAdd(m.Groups[1].Value);

        foreach (Match m in SimpleFieldRegex.Matches(content))
            TryAdd(m.Groups[1].Value);

        foreach (Match m in GenericArgRegex.Matches(content))
        {
            TryAdd(m.Groups[1].Value);
            if (m.Groups[2].Success) TryAdd(m.Groups[2].Value);
            if (m.Groups[3].Success) TryAdd(m.Groups[3].Value);
        }

        foreach (Match m in MethodReturnRegex.Matches(content))
            TryAdd(m.Groups[1].Value);

        foreach (Match m in SimpleMethodRegex.Matches(content))
            TryAdd(m.Groups[1].Value);

        foreach (Match m in ParameterRegex.Matches(content))
            TryAdd(m.Groups[1].Value);

        foreach (Match m in NewExprRegex.Matches(content))
            TryAdd(m.Groups[1].Value);

        foreach (Match m in TypeofRegex.Matches(content))
            TryAdd(m.Groups[1].Value);

        foreach (Match m in IsAsRegex.Matches(content))
            TryAdd(m.Groups[1].Value);

        foreach (Match m in CastRegex.Matches(content))
            TryAdd(m.Groups[1].Value);

        foreach (Match m in GetComponentRegex.Matches(content))
            TryAdd(m.Groups[1].Value);

        return deps;
    }
}
