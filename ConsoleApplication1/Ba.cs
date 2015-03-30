using System;
using System.Collections.Generic;

namespace Walkthrough
{
    public static class StreamindExtentions
    {
        public static string GetValue(this IDictionary<string, object> data, string fieldName)
        {
            try
            {
                var v = (Dictionary<string, object>) data["sobject"];
                return v[fieldName].ToString();
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}