using InkCards.Models.Preferences;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Windows.Storage;

namespace InkCards.Services.Storage.Extensions
{
    public static class DesignInkCanvasPreferencesExtensions
    {
       public static ApplicationDataCompositeValue ToCompositeValue(this InkToolbarPreferences preferences)
        {
            var result = new ApplicationDataCompositeValue();

            foreach (var property in GetProperties(preferences))
                result.Add(new KeyValuePair<string, object>(property.Name, property.GetValue(preferences)));

            return result;
        }

        public static InkToolbarPreferences FromCompositeValue(this ApplicationDataCompositeValue preferences)
        {
            var result = new InkToolbarPreferences();

            foreach (var property in GetProperties(result))
            {
                if(preferences.ContainsKey(property.Name))
                    property.SetValue(result, preferences[property.Name]);
            }

            return result;
        }

        private static IEnumerable<PropertyInfo> GetProperties(InkToolbarPreferences preferences)
            => preferences.GetType().GetTypeInfo()
                    .DeclaredProperties
                    .Where(x => x.CanRead && x.CanWrite);
    }
}
