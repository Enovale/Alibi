using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Alibi.Helpers
{
    public class JsonResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var prop = base.CreateProperty(member, memberSerialization);

            if (member.DeclaringType == typeof(Configuration))
            {
                prop.Writable = true;
            }

            return prop;
        }
    }
}