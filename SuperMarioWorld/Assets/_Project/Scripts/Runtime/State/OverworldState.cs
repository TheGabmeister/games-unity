public sealed class OverworldState : IGameState
{
    public void OnEnter()
    {
        PlayerInputBinding.SwitchMapOnAllPlayers(InputMapNames.Overworld);
    }
    public void OnExit() { }
}
