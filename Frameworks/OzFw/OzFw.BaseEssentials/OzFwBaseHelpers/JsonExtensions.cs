using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OzFwBaseHelpers
{
    public static class JsonExtensions
    {
        public static bool ValidateJSON(this string s)
        {
            try
            {
                JToken.Parse(s);
                return true;
            }
            catch (JsonReaderException)
            {
                return false;
            }
        }
    }
}
