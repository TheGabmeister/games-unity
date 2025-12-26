namespace ScriptableObjectArchitecture
{
    [System.Serializable]
    public sealed class SceneDataReference : BaseReference<LevelData, SceneDataVariable>
    {
        public SceneDataReference() : base() { }
        public SceneDataReference(LevelData value) : base(value) { }
    }
}