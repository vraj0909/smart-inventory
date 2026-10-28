using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace SmartInventory.Common
{
    public static class SessionKeys
    {
        public const string LastActionMessage = "Session_LastActionMessage";
        public const string LastActionTime = "Session_LastActionTime";
        public const string ActionCount = "Session_ActionCount";
        public const string RestockCart = "Session_RestockCart";
    }

    public static class SessionExtensions
    {
        public static void SetObject<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T? GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }

        public static void RecordUserAction(this ISession session, string actionDescription)
        {
            session.SetString(SessionKeys.LastActionMessage, actionDescription);
            session.SetString(SessionKeys.LastActionTime, System.DateTime.Now.ToString("MMM dd, yyyy - hh:mm:ss tt"));

            int currentCount = session.GetInt32(SessionKeys.ActionCount) ?? 0;
            session.SetInt32(SessionKeys.ActionCount, currentCount + 1);
        }
    }
}
