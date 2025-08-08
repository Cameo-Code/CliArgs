using System;
using System.Collections.Generic;
using System.Text;

namespace CliArgs
{
    // The utility class that generates the help text
    public static class CliArgHelpGen
    {
        public static string GenerateHelp(CliArgDescr descr)
        {
            if (descr == null) return "";

            // sorting keys by their values
            List<CliArgKey> keys = new List<CliArgKey>();
            keys.AddRange(descr.logicalKeys);

            keys.Sort((a, b) =>
            {
                string ak = GetSortKey(a);
                string bk = GetSortKey(b);
                return string.Compare(ak, bk);
            });

            List<HelpKeyDescr> helpList = new List<HelpKeyDescr>();
            string spfx = descr.HelpGetShortPfx();
            string fpfx = descr.HelpGetFullPfx();
            int maxLen = 0;
            foreach (var k in keys)
            {
                string keyStr = GetSVNLikeKeys(k, spfx, fpfx);
                var hd = new HelpKeyDescr(k, keyStr);
                helpList.Add(hd);
                maxLen = Math.Max(maxLen, keyStr.Length);
            }

            string pfx = "  ";


            StringBuilder bld = new StringBuilder();

            // help for actions
            if ((descr.actions != null) && (descr.actions.Length > 0))
            {
                int maxactlen = MaxLen(descr.actions);
                bld.AppendLine("Actions:");
                const string actpfx = "  ";
                foreach (var a in descr.actions)
                {
                    bld.Append(actpfx);
                    bld.Append($"{a}");
                    bld.Append(' ', maxactlen - a.Length + 1);
                    int used = maxactlen + 1 + actpfx.Length;
                    if (descr.actHelp.TryGetValue(a, out var acthelp))
                    {
                        acthelp = acthelp.TrimEnd();
                        if (used < 30)
                        {
                            bld.Append(acthelp);
                        }
                        else
                        {
                            bld.AppendLine();
                            bld.Append(' ', 8);
                            bld.Append(acthelp);
                        }
                    }
                    bld.AppendLine();
                }
                bld.AppendLine();
            }

            // help for keys
            foreach (var hd in helpList)
            {
                bld.Append(pfx);
                bld.Append(hd.key);
                bld.Append(' ', maxLen - hd.key.Length);
                bld.Append(": ");
                bld.Append(hd.keyDescr.helpDescr);
                bld.AppendLine();
            }
            return bld.ToString();
        }

        public class HelpKeyDescr
        {
            public string key;
            public CliArgKey keyDescr;
            public HelpKeyDescr(CliArgKey d, string k)
            {
                key = k;
                keyDescr = d;
            }
        }


        public static int MaxLen(IEnumerable<string> list)
        {
            if (list == null) return 0;
            int result = 0;
            foreach(var s in list)
            {
                if (string.IsNullOrWhiteSpace(s))
                    continue;
                result = Math.Max(result, s.Length);
            }
            return result;
        }

        public static string GetSortKey(CliArgKey k)
        {
            if (k == null) return string.Empty;
            if ((k.fullKeys != null) && (k.fullKeys.Length > 0))
                return k.fullKeys[0];
            if ((k.shortKeys != null) && (k.shortKeys.Length > 0)) 
                return k.shortKeys[0];
            return string.Empty;
        }

        // returns a string describing keys, in the following manner:
        //  -shortkey [--longkey] 
        // for example
        //   -x [--extensions] ARG
        //   -l [--limit] ARG

        public static string GetSVNLikeKeys(CliArgKey key, string shortPfx, string fullPfx)
        {
            string s = key.HelpGetShortKey();
            if ((string.IsNullOrEmpty(shortPfx)) || string.IsNullOrEmpty(s)) 
                s = "";
            else 
                s = $"{shortPfx}{s}";

            string f = key.HelpGetFullKey();
            if ((string.IsNullOrEmpty(fullPfx)) || string.IsNullOrEmpty(f)) 
                f = "";
            else
                f = $"{fullPfx}{f}";

            if ((s == "")&&(f ==""))
                return "";


            string res;
            if (s == "")
                res = f;
            else if (f == "")
                res = s;
            else
                res = $"{s} [{f}]";

            if (!key.isBoolOnly)
                res = res + " ARG";
            return res;
        }
    }

    public static class HelpHelpers
    {
        public static string HelpGetShortPfx(this CliArgDescr dscr)
        {
            if (dscr == null)
                return "";
            if ((dscr.shortKeyPrefix == null) || (dscr.shortKeyPrefix.Length == 0))
                return "";
            return dscr.shortKeyPrefix[0];
        }
        public static string HelpGetFullPfx(this CliArgDescr dscr)
        {
            if (dscr == null)
                return "";
            if ((dscr.fullKeyPrefix == null) || (dscr.fullKeyPrefix.Length == 0))
                return "";
            return dscr.fullKeyPrefix[0];
        }
        public static string HelpGetShortKey(this CliArgKey key)
        {
            if (key == null) 
                return "";
            if ((key.shortKeys == null) || (key.shortKeys.Length == 0))
                return "";
            return key.shortKeys[0];
        }
        public static string HelpGetFullKey(this CliArgKey key)
        {
            if (key == null)
                return "";
            if ((key.fullKeys == null) || (key.fullKeys.Length == 0))
                return "";
            return key.fullKeys[0];
        }
    }
}
