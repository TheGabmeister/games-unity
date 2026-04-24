using TMPro;
using UnityEngine;

public class TrainingFacility : MonoBehaviour, IInteractable
{
    [SerializeField] private TrainingData _training;
    [SerializeField] private TextMeshPro _promptText;
    [SerializeField] private int _tirednessThreshold = 80;

    public string InteractPrompt => _training != null ? $"Train {_training.Stat}" : "Train";

    private void Awake()
    {
        HidePrompt();
    }

    public void Interact()
    {
        if (_training == null) return;

        DigimonInstance partner = FindFirstObjectByType<DigimonInstance>();
        if (partner == null || partner.IsSleeping) return;

        if (partner.Tiredness >= _tirednessThreshold)
        {
            Debug.Log($"[Training] {partner.Species.SpeciesName} is too tired to train!");
            return;
        }

        int gain = Random.Range(_training.StatGainMin, _training.StatGainMax + 1);
        partner.TrainStat(_training.Stat, gain);
        partner.ModifyTiredness(_training.TirednessCost);
        partner.ModifyHappiness(-_training.HappinessCost);

        Debug.Log($"[Training] {partner.Species.SpeciesName} trained {_training.Stat}! +{gain}");
    }

    public void ShowPrompt()
    {
        if (_promptText != null)
            _promptText.gameObject.SetActive(true);
    }

    public void HidePrompt()
    {
        if (_promptText != null)
            _promptText.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (_promptText != null && _promptText.gameObject.activeSelf)
            _promptText.transform.rotation = Camera.main.transform.rotation;
    }
}
