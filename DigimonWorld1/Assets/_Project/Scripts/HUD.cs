using TMPro;
using UnityEngine;

public class HUD : Singleton<HUD>
{
    [SerializeField] private TMP_Text _timeText;

    private void Update()
    {
        _timeText.text = TimeSystem.Instance.TimeString;
    }
}
