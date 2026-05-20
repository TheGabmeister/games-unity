public sealed class TitleState : IGameState
{
    public void OnEnter()
    {
        // Iterating PlayerInput.all is a no-op when no players are joined (Title, Overworld),
        // and becomes meaningful when level-entry joins P1 (and in co-op, P2).
        PlayerInputBinding.SwitchMapOnAllPlayers(InputMapNames.UI);
    }
    public void OnExit() { }
}
