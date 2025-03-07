namespace ZD_Article_Grabber.Resources.Nodes
{
    /*
     * ResourceContent: An abstract base record that serves as the parent for specific content types.
        ResourceContentString: Inherits from ResourceContent and holds string data.
        ResourceContentBytes: Inherits from ResourceContent and holds byte[] data.
    */
    public abstract record NodeContent;
    public sealed record NodeContentString(string Content) : NodeContent;
    public sealed record NodeContentBytes(byte[] Content) : NodeContent;


}
