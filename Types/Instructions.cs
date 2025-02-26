namespace ZD_Article_Grabber.Types
{
    public enum Instructions: byte
    {
        PlainText = 1,
        BitMap = 2,
        Audio = 4,
        Video= 8,
        Binary = 16,
        Reference = 32
    }
}