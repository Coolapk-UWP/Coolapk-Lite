using CoolapkLite.Controls.DataTemplates;
using CoolapkLite.Helpers;
using CoolapkLite.Models;
using CoolapkLite.ViewModels.DataSource;
using CoolapkLite.ViewModels.Providers;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using Windows.ApplicationModel.Resources;

namespace CoolapkLite.ViewModels.FeedPages
{
    public class IndexViewModel : EntityItemSource, IViewModel
    {
        public string Title { get; } = ResourceLoader.GetForCurrentView("MainPage").GetString("Home");

        public IndexViewModel()
        {
            Provider = new CoolapkListProvider(
                (p, _, __) => UriHelper.GetUri(UriType.GetIndexPage, "/main/indexV8", "?", p),
                GetEntities,
                "entityId");
        }

        bool IViewModel.IsEqual(IViewModel other) => other is IndexViewModel model && Equals(model);

        private IEnumerable<Entity> GetEntities(JObject json)
        {
            if (json.TryGetValue("entityTemplate", out JToken entityTemplate))
            {
                if (entityTemplate?.ToString() == "feedCoolPictureGridCard")
                {
                    foreach (JToken item in json.Value<JArray>("entities"))
                    {
                        Entity entity = EntityTemplateSelector.GetEntity((JObject)item, true);
                        if (entity != null)
                        {
                            yield return entity;
                        }
                    }
                }
                else
                {
                    yield return EntityTemplateSelector.GetEntity(json, true);
                }
            }
            yield break;
        }
    }
}
