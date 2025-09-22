using HarmonyLib;

namespace BetterBeastCrest.Services
{
    public static class ReflectionUtils
    {
        public static void CopyFields<T>(T source, T target, string[] fieldNames)
        {
            foreach (var name in fieldNames)
            {
                var field = AccessTools.Field(typeof(T), name);
                if (field == null)
                {
                    Plugin.Log.LogError($"Field '{name}' not found in type {typeof(T).FullName}");
                    continue;
                }

                var value = field.GetValue(source);
                field.SetValue(target, value);
            }
        }

        public static void CopyProperties<T>(T source, T target, string[] propertyNames)
        {
            foreach (var name in propertyNames)
            {
                var prop = AccessTools.Property(typeof(T), name);
                if (prop == null)
                {
                    Plugin.Log.LogError($"Property '{name}' not found in type {typeof(T).FullName}");
                    continue;
                }

                var value = prop.GetValue(source, null);
                var setter = prop.GetSetMethod(true);
                if (setter != null)
                    setter.Invoke(target, new[] { value });
                else
                    Plugin.Log.LogError($"Property '{name}' has no setter on type {typeof(T).FullName}");
            }
        }
    }
}
