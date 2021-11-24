using Newtonsoft.Json.Linq;

namespace CoolapkLite.Core.Models
{
    public class Entity
    {
        public bool EntityFixed { get; set; }
        public string EntityId { get; private set; }
        public string EntityType { get; private set; }

        public Entity(JObject o)
        {
            if (o == null)
            {
                //throw new ArgumentNullException(nameof(o));
                return;
            }

            if (o.TryGetValue("entityId", out JToken v1))
            {
                EntityId = v1.ToString().Replace("\"", string.Empty);
            }

            if (o.TryGetValue("entityType", out JToken v2))
            {
                EntityType = v2.ToString();
            }

            if (o.TryGetValue("entityFixed", out JToken v3))
            {
                try
                {
                    if (v3.ToObject<int>() == 1) { EntityFixed = true; }
                }
                catch { }
            }
        }
    }

    public class NullModel : Entity
    {
        public NullModel(JObject o = null) : base(o)
        {

        }
    }
}
