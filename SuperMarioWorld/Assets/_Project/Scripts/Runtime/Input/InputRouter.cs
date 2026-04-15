using UnityEngine;
using UnityEngine.InputSystem;

namespace SMW.InputRouting
{
    public enum InputMapName { None, Player, Overworld, UI }

    public sealed class InputRouter : MonoBehaviour
    {
        [SerializeField] private InputActionAsset actions;

        private InputActionMap _player;
        private InputActionMap _overworld;
        private InputActionMap _ui;

        public InputMapName Active { get; private set; } = InputMapName.None;

        public InputActionAsset Actions => actions;

        private void Awake()
        {
            if (actions == null) return;
            _player = actions.FindActionMap("Player", throwIfNotFound: false);
            _overworld = actions.FindActionMap("Overworld", throwIfNotFound: false);
            _ui = actions.FindActionMap("UI", throwIfNotFound: false);
        }

        public void Switch(InputMapName map)
        {
            _player?.Disable();
            _overworld?.Disable();
            _ui?.Disable();

            switch (map)
            {
                case InputMapName.Player:    _player?.Enable();    break;
                case InputMapName.Overworld: _overworld?.Enable(); break;
                case InputMapName.UI:        _ui?.Enable();        break;
            }
            Active = map;
        }

        public InputActionMap GetMap(InputMapName map) => map switch
        {
            InputMapName.Player => _player,
            InputMapName.Overworld => _overworld,
            InputMapName.UI => _ui,
            _ => null
        };
    }
}
