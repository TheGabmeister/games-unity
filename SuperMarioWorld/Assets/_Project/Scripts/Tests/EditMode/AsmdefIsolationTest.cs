using System.IO;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace SMW.Tests.EditMode
{
    public sealed class AsmdefIsolationTest
    {
        private static string RuntimeAsmdefPath => "Assets/_Project/Scripts/SMW.Runtime.asmdef";
        private static string EditorAsmdefPath  => "Assets/_Project/Scripts/Editor/SMW.Editor.asmdef";
        private static string TestsEditModeAsmdefPath => "Assets/_Project/Scripts/Tests/EditMode/SMW.Tests.EditMode.asmdef";
        private static string TestsPlayModeAsmdefPath => "Assets/_Project/Scripts/Tests/PlayMode/SMW.Tests.PlayMode.asmdef";

        private static JObject Read(string path)
        {
            var full = Path.Combine(Application.dataPath, "..", path);
            return JObject.Parse(File.ReadAllText(full));
        }

        [Test]
        public void Runtime_Has_No_UnityEditor_Reference()
        {
            var json = Read(RuntimeAsmdefPath);
            var refs = json.Value<JArray>("references");
            Assert.IsNotNull(refs, "references array missing");
            foreach (var r in refs)
                Assert.AreNotEqual("UnityEditor", r.ToString(), "Runtime must not reference UnityEditor");
            var includes = json["includePlatforms"];
            if (includes != null)
                foreach (var p in includes)
                    Assert.AreNotEqual("Editor", p.ToString(), "Runtime must not be Editor-only");
        }

        [Test]
        public void Editor_Asmdef_Is_Editor_Only()
        {
            var json = Read(EditorAsmdefPath);
            var includes = json.Value<JArray>("includePlatforms");
            Assert.IsNotNull(includes);
            CollectionAssert.Contains(includes.ToObject<string[]>(), "Editor");
        }

        [Test]
        public void Tests_Have_Nunit_Reference()
        {
            foreach (var path in new[] { TestsEditModeAsmdefPath, TestsPlayModeAsmdefPath })
            {
                var json = Read(path);
                var pre = json.Value<JArray>("precompiledReferences");
                Assert.IsNotNull(pre, $"{path} missing precompiledReferences");
                var list = pre.ToObject<string[]>();
                CollectionAssert.Contains(list, "nunit.framework.dll");
            }
        }
    }
}
