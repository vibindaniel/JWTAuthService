using System.Collections.Generic;

namespace DuploAuth.Helpers
{
    public static class Message
    {
        public static KeyValuePair<string, string> AddMessageToResponse(string code, string description)
        {
            return new KeyValuePair<string, string>(code, description);
        }
    }
}