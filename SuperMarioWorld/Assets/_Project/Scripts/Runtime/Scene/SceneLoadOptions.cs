namespace SMW
{
    public struct SceneLoadOptions
    {
        public float FadeOutDuration;
        public float FadeInDuration;
        public bool UnloadPrevious;
        public object Payload;

        public static SceneLoadOptions Default => new()
        {
            FadeOutDuration = 0.3f,
            FadeInDuration = 0.3f,
            UnloadPrevious = true,
            Payload = null
        };
    }
}
