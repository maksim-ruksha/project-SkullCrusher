namespace AI.Classes.States.Configs
{
    public abstract class AiStateConfig
    {
        // in case if only questionable transitions left
        // we'll choose a random one based on weights
        public float stateWeight = 3.25f;
        //TODO: use stateTimeout
        public float stateTimeout = 10.0f * 60.0f;
    }
}