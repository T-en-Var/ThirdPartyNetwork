using thirdpartynetwork.Core.Interfaces.Helpers;

namespace thirdpartynetwork.Application.Helpers
{
    public class DynamicAudienceCache : IDynamicAudienceCache
    {
        private volatile HashSet<string> _validAudiences = new(StringComparer.OrdinalIgnoreCase);

        public void ReloadAudiences(IEnumerable<string> redirectUris)
        {
            var newSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            Console.WriteLine("Reloading Audiences");
            foreach (var uriString in redirectUris)
            {
                if (!Uri.TryCreate(uriString, UriKind.Absolute, out var parsedUri)) continue;

                var baseUrl = parsedUri.GetLeftPart(UriPartial.Authority);
                Console.WriteLine(baseUrl);
                newSet.Add(baseUrl);
            }

            _validAudiences = newSet;
        }

        public bool IsValidAudience(string? audience)
        {
            Console.WriteLine($"IsValidAudience: {audience}");
            foreach (var uriString in _validAudiences)
            {
                Console.WriteLine(uriString);
            }
            
            return audience != null && _validAudiences.Contains(audience);
        }
    }
}