using System;
using System.Collections.Generic;
using System.Text;

namespace CliArgs
{
    // The key description
    public class CliArgKey
    {
        // just a logical name. not used anywhere?
        public string logicalName;
        
        // the list of full-name. (short keys are always separated by whitespace)
        public string[] fullKeys;
        
        // the list of short keys. (short keys are always separated by whitespace)
        // and are using "short prefixes)
        public string[] shortKeys;

        // if set to true, then no value is expected when parsing the key
        // no attempt to parse the value is made.
        public bool isBoolOnly;
        
        // popuplated as needed 
        public object tag;

        // help description (used only during help generation)
        public string helpDescr;
    }

    // The basic definition of the command-line OR command-line action
    // Not to be used directly, but rather via sub-classing to CliArgDescr or CliArgAction
    public class CliArgSetOfKeys
    {
        public bool isCaseSensitive;

        public CliArgKey[] logicalKeys;
        private Dictionary<string, CliArgKey> fullLk;
        private Dictionary<string, CliArgKey> shortLk;
        private List<int> shortKeyLengths;

        private void BuildSearch()
        {
            IEqualityComparer<string> comparer;
            if (isCaseSensitive)
                comparer = StringComparer.InvariantCulture;
            else
                comparer = StringComparer.InvariantCultureIgnoreCase;
            fullLk = new Dictionary<string, CliArgKey>(comparer);
            shortLk = new Dictionary<string, CliArgKey>(comparer);
            Dictionary<int, bool> shLen = new Dictionary<int, bool>();
            if (logicalKeys != null)
            {
                foreach (var k in logicalKeys)
                {
                    if (k.fullKeys != null)
                        foreach (var fk in k.fullKeys)
                            fullLk[fk] = k;
                    if (k.shortKeys != null)
                        foreach (var sk in k.shortKeys)
                        {
                            shortLk[sk] = k;
                            shLen[sk.Length] = true;
                        }
                }
            }
            shortKeyLengths = new List<int>();
            shortKeyLengths.AddRange(shLen.Keys);
            shortKeyLengths.Sort();
        }

        public CliArgKey SearchFullKey(string fk)
        {
            if (fullLk == null) BuildSearch();
            CliArgKey result;
            if (!fullLk.TryGetValue(fk, out result))
                result = null;
            return result;
        }

        public CliArgKey SearchShortKey(string sk)
        {
            if (shortLk == null) BuildSearch();
            CliArgKey result;
            if (!shortLk.TryGetValue(sk, out result))
                result = null;
            return result;
        }


        public CliArgKey SearchKey(string fk)
        {
            var result = SearchFullKey(fk);
            if (result == null)
                result = SearchShortKey(fk);
            return result;
        }

        public int[] GetShortKeyLength()
        {
            return shortKeyLengths.ToArray();
        }

    }

    // todo!
    public class CliArgAction : CliArgSetOfKeys
    {

    }

    public class CliArgDescr : CliArgSetOfKeys
    {
        public string[] fullKeyPrefix; // i.e. --
        // normally it's a single string
        public string[] fullKeySeparator; // i.e. --key=value

        // short keys are expected to be separated by whiespace
        public string[] shortKeyPrefix;  // i.e - -k v

        public bool allowCombineShortKey; // i.e. -iab, means -i -a -b.. quite often used in unix tools

        public static CliArgDescr Default2000 = new CliArgDescr {
            fullKeyPrefix = new string[] { "--" },
            fullKeySeparator = new string[] { "=" },
            shortKeyPrefix = new string[] { "-" },
            isCaseSensitive = true,
            allowCombineShortKey = false,
        };

        public CliArgDescr()
        { }
        public CliArgDescr(CliArgDescr init):this()
        {
            if (init == null) return;
            this.fullKeyPrefix = init.fullKeyPrefix;
            this.fullKeySeparator = init.fullKeySeparator;
            this.shortKeyPrefix = init.shortKeyPrefix;
            this.isCaseSensitive = init.isCaseSensitive;
            this.allowCombineShortKey = init.allowCombineShortKey;
        }



        public bool HasFullKeySeparator()
        {
            return ((fullKeySeparator) != null && (fullKeySeparator.Length > 0));
        }
    }
}
