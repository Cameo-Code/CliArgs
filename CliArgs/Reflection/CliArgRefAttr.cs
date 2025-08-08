using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace CliArgs
{

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    // full key 
    public class CliFullAttribute : Attribute
    {
        public string Key;
        public CliFullAttribute(string nm = "")
        {
            Key = nm;
        }
    }

    [AttributeUsage(AttributeTargets.Property| AttributeTargets.Field, AllowMultiple = true)]
    public class CliShortAttribute : Attribute
    {
        public string Key;
        public CliShortAttribute(string sh = "")
        {
            Key = sh;
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    // just the indication that the value needs to be applied here
    // if index is specified, then the value should be of a certain val
    public class CliValueAttribute : Attribute
    {
        public int Index = -1;
        public CliValueAttribute(int aindex = -1)
        {
            Index = aindex;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
    // a line of help description
    public class CliHelpAttribute : Attribute
    {
        public string Line;
        public CliHelpAttribute(string line = "")
        {
            Line = line;
        }
    }


    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    // shows the "action" property to be filled
    // "action" is expected to be specified prior to all the keys
    public class CliActionAttribute : Attribute 
    {
        public string ActValue;
        public CliActionAttribute(string act = "")
        {
            ActValue = act;
        }
    }
}
