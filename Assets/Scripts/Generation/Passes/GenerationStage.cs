namespace Generation.Passes
{
    public enum GenerationStage
    {
        None,
        Terrain,
        Objects
    }

    public static class GenerationStageExtensions
    {
        public static GenerationStage? GetPreviousStage(this GenerationStage stage)
        {
            var previous = (int)stage - 1;
            return previous < 0 ? null : (GenerationStage)previous;
        }
    }
}
