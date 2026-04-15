namespace SMW
{
    public sealed class OverworldState : IGameState
    {
        public void OnEnter()
        {
            GameServices.SwitchMapOnAllPlayers(InputMapNames.Overworld);
        }
        public void OnExit() { }
    }
}
