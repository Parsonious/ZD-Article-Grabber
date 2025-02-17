using ZD_Article_Grabber.Helpers;
using ZD_Article_Grabber.Types;

namespace ZD_Article_Grabber.Resources
{
    public abstract class ResourceID
    {
        public virtual string LocalUrl { get; protected set; }
        public virtual string FallbackRemoteUrl { get; protected set; }
        public virtual string ExternalResourceUrl { get; protected set; }
        public virtual string ResourceUrl { get; internal set; }
        public virtual string Name { get; protected set; }
        public virtual ResourceType Type { get; protected set; }
        public virtual string CacheKey { get; internal set; }
        public virtual string ID { get; internal set; }
        private protected uint _seed { get; private set; }
        private protected uint _salt { get; private set; }
        private protected CacheHelper _cacheHelper = new();
        private protected SeedHelper _seedHelper = new();
        private protected FNV1aHashHelper _fnv1aHashHelper = new();
        protected void GenerateCacheKey()
        {
            CacheKey = _cacheHelper.GenerateCacheKey(Type.ToString().ToLower(), Name);
        }
        protected string GetID()
        {
           _salt = _fnv1aHashHelper.GenerateID(Name);
           _seed = _seedHelper.GetHybridSeed(_salt);
           MethodSelectHelper methodSelectHelper = new(_seed,_fnv1aHashHelper,Name);
           string? id = (string?)methodSelectHelper.CallMethod();
           return id == null ? throw new InvalidOperationException("ID not generated") : id;

        }
        protected uint GetSeed()
        {
            return _seedHelper.GetRandomUInt();
        }
        protected ushort GetSalt()
        {
            return _seedHelper.GetRandomUShort();
        }
    }
}
