using System;
using System.Collections.Generic;
using System.Text;

namespace CliArgs
{
    public static class RefUtils
    {
        // returns true, if tp is string[]
        public static bool IsStringArray(Type tp)
        {
            if (tp == null) return false;
            if (!tp.IsArray) return false;
            var el = tp.GetElementType();
            return (el != null) && (el == typeof(string));
        }

        // returns if specified type implements ICollection<string>
        public static bool IsCollectionStr(Type tp)
        {
            if (tp == null) return false;
            Type ienum = typeof(ICollection<string>);
            return ienum.IsAssignableFrom(tp);
        }
    }
}
