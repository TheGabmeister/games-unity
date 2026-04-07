using UnityEngine;

public enum MusicTrack
{
    Title,
    PartyCreation,
    WorldMap,
    Battle,
    BattleVictory,
    BossBattle,
    Town,
    Dungeon,
    Castle,
    Ship,
    Airship,
    GameOver,
    FinalBoss,
    CrystalRoom,
    Ending
}

public enum SoundEffect
{
    Confirm,
    Cancel,
    Cursor,
    Hit,
    CriticalHit,
    Miss,
    MagicCast,
    Heal,
    Buff,
    Debuff,
    EnemyDeath,
    CharacterDeath,
    LevelUp,
    ChestOpen,
    DoorOpen,
    ShopBuy,
    ShopSell,
    Error,
    EncounterStart,
    FleeSuccess,
    FleeFail,
    SaveGame,
    LoadGame
}

public class AudioManager : MonoBehaviour
{
    MusicTrack? currentTrack;

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)] float masterVolume = 1f;
    [SerializeField, Range(0f, 1f)] float bgmVolume = 1f;
    [SerializeField, Range(0f, 1f)] float sfxVolume = 1f;

    public float MasterVolume
    {
        get => masterVolume;
        set => masterVolume = Mathf.Clamp01(value);
    }

    public float BGMVolume
    {
        get => bgmVolume;
        set => bgmVolume = Mathf.Clamp01(value);
    }

    public float SFXVolume
    {
        get => sfxVolume;
        set => sfxVolume = Mathf.Clamp01(value);
    }

    public void PlayBGM(MusicTrack track, float fadeDuration = 1f)
    {
        Debug.Log($"[Audio] PlayBGM: {track} (fade: {fadeDuration}s)");
        currentTrack = track;
    }

    public void StopBGM(float fadeDuration = 1f)
    {
        Debug.Log($"[Audio] StopBGM (fade: {fadeDuration}s) — was playing: {currentTrack}");
        currentTrack = null;
    }

    public void PauseBGM()
    {
        Debug.Log($"[Audio] PauseBGM: {currentTrack}");
    }

    public void ResumeBGM()
    {
        Debug.Log($"[Audio] ResumeBGM: {currentTrack}");
    }

    public void PlaySFX(SoundEffect sfx)
    {
        Debug.Log($"[Audio] PlaySFX: {sfx}");
    }
}
