using ZD_Article_Grabber.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Collections.ObjectModel;
namespace ZD_Article_Grabber.Services
{
    public class ResourceInstructions : IResourceInstructions
    {
        public IReadOnlyDictionary<Types.ResourceType, Types.Instructions> Instructions { get; }
        public bool IsResourceMatched(Types.ResourceType resourceType) => Instructions.ContainsKey(resourceType);

        public ResourceInstructions(IEnumerable<Claim> resourceClaims)
        {
            if ( resourceClaims?.Any() != true )
            {
                throw new SecurityTokenException("No claims found in the token.");
            }

            var instructions = new Dictionary<Types.ResourceType, Types.Instructions>();

            ParseClaims(resourceClaims, instructions);

            if ( instructions.Count == 0 )
            {
                throw new SecurityTokenException("No valid claims found in the token.");
            }

            Instructions = new ReadOnlyDictionary<Types.ResourceType, Types.Instructions>(instructions);
        }

        private static void ParseClaims(IEnumerable<Claim> claims, Dictionary<Types.ResourceType, Types.Instructions> instructions)
        {
            foreach ( var claim in claims )
            {
                if ( Enum.TryParse<Types.ResourceType>(claim.Type, ignoreCase: true, out Types.ResourceType type) &&
                    Enum.TryParse<Types.Instructions>(claim.Value, ignoreCase: true, out Types.Instructions instruction) )
                {
                    instructions[type] = instruction;
                }
            }
        }
    }
}
