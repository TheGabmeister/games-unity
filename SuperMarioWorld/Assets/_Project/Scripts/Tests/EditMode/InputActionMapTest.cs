using NUnit.Framework;
using UnityEditor;
using UnityEngine.InputSystem;

namespace SMW
{
    public sealed class InputActionMapTest
    {
        private const string AssetPath = "Assets/_Project/Input/InputSystem_Actions.inputactions";

        private InputActionAsset _asset;

        [OneTimeSetUp]
        public void Load()
        {
            _asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(AssetPath);
            Assert.IsNotNull(_asset, $"Missing input asset at {AssetPath}");
        }

        private static bool HasBindingWithGroup(InputAction action, string group)
        {
            foreach (var b in action.bindings)
            {
                if (b.isComposite) continue;
                if (!string.IsNullOrEmpty(b.groups) && b.groups.Contains(group))
                    return true;
            }
            return false;
        }

        private void AssertPlayerAction(string name)
        {
            var map = _asset.FindActionMap("Player", throwIfNotFound: true);
            var action = map.FindAction(name, throwIfNotFound: true);
            Assert.IsTrue(HasBindingWithGroup(action, "Keyboard"), $"{name} missing Keyboard binding");
            Assert.IsTrue(HasBindingWithGroup(action, "Gamepad"),  $"{name} missing Gamepad binding");
        }

        [Test] public void Player_Move_HasBothBindings()     => AssertPlayerAction("Move");
        [Test] public void Player_Jump_HasBothBindings()     => AssertPlayerAction("Jump");
        [Test] public void Player_SpinJump_HasBothBindings() => AssertPlayerAction("SpinJump");
        [Test] public void Player_Action_HasBothBindings()   => AssertPlayerAction("Action");
        [Test] public void Player_Pause_HasBothBindings()    => AssertPlayerAction("Pause");

        [Test]
        public void Overworld_Map_Exists()
        {
            var map = _asset.FindActionMap("Overworld", throwIfNotFound: true);
            Assert.IsNotNull(map.FindAction("Move", throwIfNotFound: false));
            Assert.IsNotNull(map.FindAction("Confirm", throwIfNotFound: false));
            Assert.IsNotNull(map.FindAction("Cancel", throwIfNotFound: false));
        }

        [Test]
        public void UI_Map_Exists()
        {
            var map = _asset.FindActionMap("UI", throwIfNotFound: true);
            Assert.IsNotNull(map.FindAction("Navigate", throwIfNotFound: false));
            Assert.IsNotNull(map.FindAction("Submit", throwIfNotFound: false));
            Assert.IsNotNull(map.FindAction("Cancel", throwIfNotFound: false));
        }
    }
}
