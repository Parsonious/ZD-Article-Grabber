using ZD_Article_Grabber.Helpers;
using ZD_Article_Grabber.Types;

namespace ZD_Article_Grabber.Resources
{
    public abstract class ResourceID
    {
        public virtual string LocalUrl { get; protected init; }
        public virtual string FallbackRemoteUrl { get; protected init; }
        public virtual string ExternalResourceUrl { get; protected init; }
        public virtual string ResourceUrl { get; internal init; }
        public virtual string Name { get; protected init; }
        public virtual ResourceType Type { get; protected init; }
        public virtual string CacheKey { get; internal set; }
        public virtual string ID { get; internal set; }
        
        
        
        private uint? _seed;
        private uint? _salt;
        private protected uint Salt => _salt ??= _fnv1aHashHelper.GenerateID(Name);
        private protected uint Seed => _seed ??= _seedHelper.GetHybridSeed(Salt);
        
        
        private readonly protected CacheHelper _cacheHelper = new();
        private readonly protected SeedHelper _seedHelper = new();
        private readonly protected FNV1aHashHelper _fnv1aHashHelper = new();
        protected void GenerateCacheKey()
        {
            CacheKey = _cacheHelper.GenerateCacheKey(Type.ToString().ToLower(), Name);
        }
        protected string GetID()
        {
            MethodSelectHelper methodSelectHelper = new(Seed, _fnv1aHashHelper, Name);
            string? id = (string?) methodSelectHelper.CallMethod();
            return id ?? throw new InvalidOperationException("ID not generated");
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
