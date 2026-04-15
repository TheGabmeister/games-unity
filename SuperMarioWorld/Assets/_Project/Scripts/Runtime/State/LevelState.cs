
namespace SMW
{
    public sealed class LevelState : IGameState
    {
        public LevelData Data { get; }
        public string EntryPoint { get; }

        public LevelState(LevelData data, string entryPoint)
        {
            Data = data;
            EntryPoint = entryPoint;
        }

        public void OnEnter() { }
        public void OnExit() { }
    }
}
