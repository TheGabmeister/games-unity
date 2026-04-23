using UnityEngine;

public class SplashscreenController : MonoBehaviour
{
    [SerializeField] private float _duration = 1f;

    private float _timer;
    private bool _transitioned;

    private void Update()
    {
        _timer += Time.deltaTime;
        if (!_transitioned && _timer >= _duration)
        {
            _transitioned = true;
            GameManager.Instance.LoadIntroScene();
        }
    }

    private void OnGUI()
    {
        float logoWidth = 300f;
        float logoHeight = 200f;
        float x = (Screen.width - logoWidth) / 2f;
        float y = (Screen.height - logoHeight) / 2f;

        GUI.Box(new Rect(x, y, logoWidth, logoHeight), "");

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 28,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };

        GUIStyle subtitleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.grey }
        };

        GUI.Label(new Rect(x, y + 50f, logoWidth, 50f), "DIGIMON", titleStyle);
        GUI.Label(new Rect(x, y + 90f, logoWidth, 50f), "WORLD", titleStyle);
        GUI.Label(new Rect(x, y + 140f, logoWidth, 30f), "[ placeholder ]", subtitleStyle);
    }
}
