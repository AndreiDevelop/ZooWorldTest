namespace ZooWorld.Domain.Session
{
    public enum SessionState
    {
        Created,
        Loading,
        Ready,
        Stopping,
        Stopped,
        Failed
    }
}
