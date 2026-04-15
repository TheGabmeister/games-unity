using UnityEngine;

namespace SMW
{
    // Phase 1 placeholder. Full pickup / carry / throw semantics land with the block
    // and enemy roster (Phase 2 introduces springboards and P-switches; Phase 3 adds
    // stunned shells). For now, exposes the surface that PlayerController calls into
    // so the carry plumbing can be wired without the behavior existing yet.
    public sealed class PlayerCarry : MonoBehaviour
    {
        public bool IsCarrying => false;

        public bool TryPickup()
        {
            return false;
        }

        public void Drop() { }

        public void Throw(float horizontalDir) { _ = horizontalDir; }
    }
}
