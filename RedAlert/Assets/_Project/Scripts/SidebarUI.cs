using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SidebarUI : MonoBehaviour
{
    [Header("Credits")]
    [SerializeField] private TMP_Text _creditsText;
    private int _displayedCredits;
    private const float CreditTickSpeed = 5000f;

    [Header("Power Bar")]
    [SerializeField] private Image _powerBarFill;
    [SerializeField] private Image _powerBarBG;

    [Header("Buttons")]
    [SerializeField] private Button _sellButton;
    [SerializeField] private Button _repairButton;
    [SerializeField] private Image _sellButtonImage;
    [SerializeField] private Image _repairButtonImage;

    [Header("Build Grid")]
    [SerializeField] private Transform _structureGrid;
    [SerializeField] private Transform _unitGrid;
    [SerializeField] private GameObject _buildSlotPrefab;

    private readonly List<BuildSlot> _structureSlots = new();
    private readonly List<BuildSlot> _unitSlots = new();

    private static readonly Color ActiveButtonColor = new(0.3f, 1f, 0.3f);
    private static readonly Color InactiveButtonColor = Color.white;

    void Start()
    {
        _sellButton.onClick.AddListener(() => SellRepairManager.Instance?.ToggleSellMode());
        _repairButton.onClick.AddListener(() => SellRepairManager.Instance?.ToggleRepairMode());

        if (ConstructionManager.Instance != null)
            ConstructionManager.Instance.OnBuildStateChanged += RefreshBuildGrid;
        if (ProductionManager.Instance != null)
            ProductionManager.Instance.OnProductionStateChanged += RefreshBuildGrid;

        RefreshBuildGrid();
    }

    void OnDestroy()
    {
        if (ConstructionManager.Instance != null)
            ConstructionManager.Instance.OnBuildStateChanged -= RefreshBuildGrid;
        if (ProductionManager.Instance != null)
            ProductionManager.Instance.OnProductionStateChanged -= RefreshBuildGrid;
    }

    void Update()
    {
        UpdateCreditsDisplay();
        UpdatePowerBar();
        UpdateModeButtons();
        UpdateBuildProgress();
    }

    void UpdateCreditsDisplay()
    {
        if (EconomyManager.Instance == null || _creditsText == null) return;

        int localPlayer = PlayerManager.Instance.LocalPlayer.PlayerIndex;
        int actual = EconomyManager.Instance.GetCredits(localPlayer);

        if (_displayedCredits != actual)
        {
            float delta = CreditTickSpeed * Time.deltaTime;
            if (_displayedCredits < actual)
                _displayedCredits = Mathf.Min(actual, _displayedCredits + Mathf.CeilToInt(delta));
            else
                _displayedCredits = Mathf.Max(actual, _displayedCredits - Mathf.CeilToInt(delta));
        }

        _creditsText.text = $"${_displayedCredits:N0}";
    }

    void UpdatePowerBar()
    {
        if (PowerManager.Instance == null || _powerBarFill == null) return;

        int localPlayer = PlayerManager.Instance.LocalPlayer.PlayerIndex;
        int produced = PowerManager.Instance.GetProduced(localPlayer);
        int consumed = PowerManager.Instance.GetConsumed(localPlayer);

        float ratio = produced > 0 ? (float)consumed / produced : 0f;
        _powerBarFill.fillAmount = Mathf.Clamp01(ratio);

        if (PowerManager.Instance.IsLowPower(localPlayer))
            _powerBarFill.color = Color.red;
        else if (ratio > 0.8f)
            _powerBarFill.color = Color.yellow;
        else
            _powerBarFill.color = Color.green;
    }

    void UpdateModeButtons()
    {
        if (SellRepairManager.Instance == null) return;
        if (_sellButtonImage != null)
            _sellButtonImage.color = SellRepairManager.Instance.SellMode ? ActiveButtonColor : InactiveButtonColor;
        if (_repairButtonImage != null)
            _repairButtonImage.color = SellRepairManager.Instance.RepairMode ? ActiveButtonColor : InactiveButtonColor;
    }

    void RefreshBuildGrid()
    {
        int localPlayer = PlayerManager.Instance.LocalPlayer.PlayerIndex;
        var faction = PlayerManager.Instance.LocalPlayer.Faction;
        var factionData = ConstructionManager.Instance?.GetFactionData(faction);
        if (factionData == null) return;

        RefreshColumn(_structureGrid, _structureSlots, factionData.BuildableStructures, localPlayer, true);
        RefreshColumn(_unitGrid, _unitSlots, factionData.BuildableUnits, localPlayer, false);
    }

    void RefreshColumn(Transform parent, List<BuildSlot> slots, UnitData[] items,
        int playerIndex, bool isStructure)
    {
        foreach (var slot in slots)
        {
            if (slot.GO != null)
                Destroy(slot.GO);
        }
        slots.Clear();

        if (items == null || _buildSlotPrefab == null) return;

        foreach (var item in items)
        {
            if (!ConstructionManager.Instance.CanBuild(item, playerIndex)) continue;

            var go = Instantiate(_buildSlotPrefab, parent);
            var slot = new BuildSlot
            {
                GO = go,
                Item = item,
                IsStructure = isStructure
            };

            var icon = go.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null && item.Icon != null)
                icon.sprite = item.Icon;
            else if (icon != null && item.Sprite != null)
                icon.sprite = item.Sprite;

            var nameText = go.transform.Find("Name")?.GetComponent<TMP_Text>();
            if (nameText != null)
                nameText.text = $"${item.Cost}";

            var progressBar = go.transform.Find("Progress")?.GetComponent<Image>();
            if (progressBar != null)
                progressBar.fillAmount = 0f;
            slot.ProgressBar = progressBar;

            var readyLabel = go.transform.Find("ReadyLabel")?.GetComponent<TMP_Text>();
            if (readyLabel != null)
                readyLabel.gameObject.SetActive(false);
            slot.ReadyLabel = readyLabel;

            var button = go.GetComponent<Button>();
            if (button == null) button = go.AddComponent<Button>();

            var capturedItem = item;
            var capturedIsStruct = isStructure;
            button.onClick.AddListener(() => OnSlotClicked(capturedItem, capturedIsStruct, playerIndex));

            slots.Add(slot);
        }
    }

    void OnSlotClicked(UnitData item, bool isStructure, int playerIndex)
    {
        if (SellRepairManager.Instance != null)
            SellRepairManager.Instance.ClearModes();

        if (isStructure)
        {
            var queue = ConstructionManager.Instance.GetStructureQueue(playerIndex);
            if (queue.CurrentItem == item && queue.State == BuildState.Ready)
            {
                PlacementManager.Instance?.EnterPlacement(item, playerIndex);
                return;
            }
            if (queue.CurrentItem == item)
            {
                ConstructionManager.Instance.CancelBuild(playerIndex);
                return;
            }
            if (queue.CurrentItem != null) return;
            ConstructionManager.Instance.StartBuild(item, playerIndex);
        }
        else
        {
            var queue = ProductionManager.Instance.GetQueue(item.Category);
            if (queue.CurrentItem == item)
            {
                ProductionManager.Instance.CancelProduction(item.Category, playerIndex);
                return;
            }
            if (queue.CurrentItem != null) return;
            ProductionManager.Instance.StartProduction(item, playerIndex);
        }
    }

    void UpdateBuildProgress()
    {
        int localPlayer = PlayerManager.Instance.LocalPlayer.PlayerIndex;

        var structQueue = ConstructionManager.Instance?.GetStructureQueue(localPlayer);
        foreach (var slot in _structureSlots)
        {
            bool isActive = structQueue?.CurrentItem == slot.Item;
            if (slot.ProgressBar != null)
                slot.ProgressBar.fillAmount = isActive ? structQueue.Progress : 0f;
            if (slot.ReadyLabel != null)
                slot.ReadyLabel.gameObject.SetActive(isActive && structQueue.State == BuildState.Ready);
        }

        foreach (var slot in _unitSlots)
        {
            var unitQueue = ProductionManager.Instance?.GetQueue(slot.Item.Category);
            bool isActive = unitQueue?.CurrentItem == slot.Item;
            if (slot.ProgressBar != null)
                slot.ProgressBar.fillAmount = isActive ? unitQueue.Progress : 0f;
            if (slot.ReadyLabel != null)
                slot.ReadyLabel.gameObject.SetActive(isActive && unitQueue.State == BuildState.Ready);
        }
    }

    class BuildSlot
    {
        public GameObject GO;
        public UnitData Item;
        public bool IsStructure;
        public Image ProgressBar;
        public TMP_Text ReadyLabel;
    }
}
