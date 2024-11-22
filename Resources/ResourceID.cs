using ZD_Article_Grabber.Helpers;

namespace ZD_Article_Grabber.Resources
{
    public abstract class ResourceID
    {
        public virtual string LocalUrl { get; protected set; }
        public virtual string RemoteUrl { get; protected set; }
        public virtual string ResourceUrl { get; internal set; }
        public virtual string Name { get; protected set; }
        public virtual ResourceType Type { get; protected set; }
        public virtual string CacheKey { get; internal set; }

        protected void GenerateCacheKey()
        {
            CacheKey = CacheHelper.GenerateCacheKey(Type.ToString().ToLower(), Name);
        }
    }
}
