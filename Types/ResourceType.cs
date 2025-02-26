namespace ZD_Article_Grabber.Types
{
    [Flags]
    public enum ResourceType : byte
    {
        None = 0,
        CSS = 1,
        HTML = 2,
        IMG = 4,
        JS = 8,
        PS1 = 16,
        SQL = 32,
        OTHER = 128
    }
}
