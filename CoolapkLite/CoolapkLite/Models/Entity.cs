using Newtonsoft.Json.Linq;
using System;

namespace CoolapkLite.Models
{
    public class Entity
    {
        public bool EntityFixed { get; set; }
        public int EntityId { get; private set; }
        public string EntityType { get; private set; }

        public Entity(JObject o)
        {
            if (o == null)
            {
                //throw new ArgumentNullException(nameof(o));
                return;
            }

            if (o.TryGetValue("entityId", out JToken entityId))
            {
                EntityId = entityId.ToObject<int>();
            }

            if (o.TryGetValue("entityType", out JToken entityType))
            {
                EntityType = entityType.ToString();
            }

            if (o.TryGetValue("entityFixed", out JToken entityFixed))
            {
                EntityFixed = Convert.ToBoolean(entityFixed.ToObject<int>());
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
