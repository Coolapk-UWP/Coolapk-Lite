using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace CoolapkLite.Models
{
    public class Entity
    {
        public bool EntityFixed { get; set; }
        public int EntityID { get; private set; }
        public string EntityIDText { get; private set; }
        public string EntityType { get; private set; }
        public string EntityForward { get; private set; }

        public Entity(JObject token)
        {
            if (token == null) { return; }

            if (token.TryGetValue("entityId", out JToken entityId))
            {
                EntityIDText = entityId.ToString();
                if (int.TryParse(EntityIDText, out int id))
                {
                    EntityID = id;
                }
            }

            if (token.TryGetValue("entityType", out JToken entityType))
            {
                EntityType = entityType.ToString();
            }

            if (token.TryGetValue("entityFixed", out JToken entityFixed))
            {
                EntityFixed = Convert.ToBoolean(entityFixed.ToObject<int>());
            }

            if (token.TryGetValue("entityForward", out JToken entityForward))
            {
                EntityForward = entityForward.ToString();
            }
        }

        public IEnumerable<Entity> AsEnumerable()
        {
            yield return this;
        }

        public override string ToString() => $"{GetType().Name}: {string.Join(" - ", EntityType, EntityIDText)}";
    }

    public class NullEntity : Entity
    {
        public NullEntity(JObject token = null) : base(token) { }
    }
}
