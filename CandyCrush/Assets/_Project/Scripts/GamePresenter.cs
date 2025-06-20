using VContainer;
using VContainer.Unity;

public class GamePresenter : IStartable
{
    readonly HelloWorldService helloWorldService;
    readonly HelloScreen helloScreen;

    public GamePresenter(
        HelloWorldService helloWorldService,
        HelloScreen helloScreen)
    {
        this.helloWorldService = helloWorldService;
        this.helloScreen = helloScreen;
    }

    void IStartable.Start()
    {
        helloScreen.HelloButton.onClick.AddListener(() => helloWorldService.Hello());
    }
}