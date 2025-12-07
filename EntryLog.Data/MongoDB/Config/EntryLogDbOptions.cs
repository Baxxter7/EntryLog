namespace EntryLog.Data.MongoDB.Config
{
    internal sealed class EntryLogDbOptions
    {
        public string ConnectionUri { get; init; } = string.Empty;
        public string DatabaseName { get; init; } = string.Empty;
    }
}
