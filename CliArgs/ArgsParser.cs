using System;
using System.Collections.Generic;
using System.Text;

namespace CliArgs
{
    public enum ArgResultType
    {
        Value,  // value is the value of the value
        Key,  // value is the value of the key, if the key is passed with the value
        Action,  // "value: is the value of action
        Error // "value" contains the error mesasge
    }

    public class ArgResult
    {
        public string value = string.Empty;
        public ArgResultType argType = ArgResultType.Value;
        public bool hasValue; // it's only used if "isKey" is used
                              // it would be set to false, if no value was specified (i.e. end of line)
                              // or the next value was the key
                              // or the key doesn't expect to have a value

        public string rawKeyName = string.Empty; // the actual key name as found in the command-line
        public bool isRawKeyShort;
        public CliArgKey logicalKey; // key scription. null, if the key found is unknown or unspecified
        public CliArgDescr logicalDescr; // the description. only used with "action"

        public bool isAction => argType == ArgResultType.Action;
        public bool isKey => argType == ArgResultType.Key;

        public static ArgResult AllocValue(string s)
        {
            ArgResult result = new ArgResult();
            result.value = s;
            result.hasValue = true;
            return result;
        }


        // short-key that has no value (and to be used only as a logical key)
        public static ArgResult AllocKey(string rawKey,  bool isShortKey, CliArgKey logKey, string value = null)
        {
            ArgResult result = new ArgResult();
            result.argType = ArgResultType.Key;
            result.hasValue = (value != null);
            if (value != null)
                result.value = value;
            result.rawKeyName = rawKey;
            result.isRawKeyShort = isShortKey;
            result.logicalKey = logKey;
            return result;
        }
        public static ArgResult AllocShortKeyNoVal(string rawKey, CliArgKey logKey)
        {
            return AllocShortKey(rawKey, logKey, null);
        }

        // short-key that has no value (and to be used only as a logical key)
        public static ArgResult AllocShortKey(string rawKey, CliArgKey logKey, string value)
        {
            return AllocKey(rawKey, true, logKey, value);
        }

        // short-key that has no value (and to be used only as a logical key)
        public static ArgResult AllocFullKey(string rawKey, CliArgKey logKey, string value)
        {
            return AllocKey(rawKey, false, logKey, value);
        }
        public static ArgResult AllocFullKeyNoVal(string rawKey, CliArgKey logKey)
        {
            return AllocFullKey(rawKey, logKey, null);
        }
    
        public static ArgResult AllocAction(string rawAction, CliArgDescr keysDescr)
        {
            return new ArgResult {
                argType = ArgResultType.Action,
                rawKeyName = rawAction,
                value = rawAction,
                logicalDescr = keysDescr
            };
        }

        public static ArgResult AllocError(string errorName)
        {
            return new ArgResult
            {
                argType = ArgResultType.Error,
                value = errorName
            };
        }
    }

    // 
    // %action% %key1% %key2% %key3% %value1% %value2% %value3%

    public class ArgsParser
    {
        public enum ParserState
        { 
            // expecting to find the action (prior to any key)
            Action,
            // expecting to find the key 
            Keys,
            // expecting to find values
            Values,
        }


        private string[] inp;
        private int inpIdx;
        private CliArgDescr descr;

        private List<string> boundShortKeys;

        public ParserState state = ParserState.Action;

        public void SetDescr(CliArgDescr descr)
        {
            this.descr = descr;
        }

        public void SetArgs(string[] args)
        {
            inpIdx = 0;
            inp = args;
        }

        private ArgResult ParseFullKey(string keypfx)
        {

            string ar = inp[inpIdx];
            inpIdx++;
            string rawkey;
            string fullval = "";
            bool hasValue = ExtractFullKey(descr.fullKeySeparator,
                ar, keypfx, out rawkey, ref fullval);

            var logKey = descr.SearchFullKey(rawkey);
            bool tryNextVal = !descr.HasFullKeySeparator();
            if ((tryNextVal) && (inpIdx < inp.Length))
            {
                hasValue = !IsAnyKey(descr, inp[inpIdx]);
                if (hasValue)
                {
                    fullval = inp[inpIdx];
                    inpIdx++;
                }
            }

            if (hasValue)
                return ArgResult.AllocFullKey(rawkey, logKey, fullval);
            else
                return ArgResult.AllocFullKeyNoVal(rawkey, logKey);
        }

        private bool TryParseCombined(string rawKey)
        {
            int[] len = descr.GetShortKeyLength();
            if ((len == null)||(len.Length == 0)) return false;
            
            List<string> keys = new List<string>();
            int ofs = 0;
            
            bool anyFound = false;
            while (ofs < rawKey.Length)
            {
                bool found = false;
                foreach(var l in len)
                {
                    if (ofs + l > rawKey.Length) continue;
                    string comb = rawKey.Substring(ofs, l);
                    var cl = descr.SearchShortKey(comb);
                    if (cl != null)
                    {
                        found = true;
                        keys.Add(comb);
                        ofs += comb.Length;
                        break;
                    }
                }
                if (!found)
                {
                    keys.Add(rawKey.Substring(ofs, 1));
                    ofs++;
                }
                else
                    anyFound = true;
            }
            if (anyFound)
                boundShortKeys = keys;

            return anyFound;
        }

        private ArgResult ParseShortKey(string keypfx)
        {
            string ar = inp[inpIdx];
            inpIdx++;
            string rawkey = ar.Substring(keypfx.Length);
            var logKey = descr.SearchShortKey(rawkey);

            if ((logKey == null)&&(descr.allowCombineShortKey))
            {
                if (TryParseCombined(rawkey))
                    return PopPendingShortKey();
            }

            bool tryNextVal = true;
            bool hasValue = false;
            string val = "";
            if ((tryNextVal) && (inpIdx < inp.Length))
            {
                hasValue = !IsAnyKey(descr, inp[inpIdx]);
                if (hasValue)
                {
                    val = inp[inpIdx];
                    inpIdx++;
                }
            }
            if (hasValue)
                return ArgResult.AllocShortKey(rawkey, logKey, val);
            else
                return ArgResult.AllocShortKeyNoVal(rawkey, logKey);

        }

        public ArgResult PopPendingShortKey()
        {
            string k = boundShortKeys[0];
            boundShortKeys.RemoveAt(0);
            var keyDescr = descr.SearchShortKey(k);
            return ArgResult.AllocShortKeyNoVal(k, keyDescr);
        }

        private ArgResult TryParseAction(string curStr)
        {
            if (IsAnyKey(descr, curStr))
            {
                // we found the key, even though we were expecting an action
                // that means we either should assume a default key

                if (descr.actionUse == ActionUse.SingleStrict)
                {
                    // todo: fail the parsing!
                    return ArgResult.AllocError("Action is expected, but key found");
                }

                // we found the key! and not an action
                // thus we would pretend as we have found an action, even though we didn't
                if (!string.IsNullOrEmpty(descr.defaultAction))
                    return ArgResult.AllocAction(descr.defaultAction, descr);
                return null;
            }


            inpIdx++;
            return ArgResult.AllocAction(curStr, descr);
        }

        public ArgResult FindNext()
        {
            if ((boundShortKeys != null)&&(boundShortKeys.Count > 0))
            {
                return PopPendingShortKey();
            }

            if (inp == null) return null;
            if (inpIdx >= inp.Length) return null;

            string ar = inp[inpIdx];
            if (descr == null) 
            {
                inpIdx++;
                return ArgResult.AllocValue(ar);
            }

            if (state == ParserState.Action)
            {
                // we would switch to keys anyway.
                state = ParserState.Keys;
                if (descr.actionUse != ActionUse.None)
                {
                    var res = TryParseAction(ar);
                    if (res != null) 
                        return res;
                }
            }


            string keypfx;
            if ((state == ParserState.Keys) && (IsFullKey(descr, ar, out keypfx)))
            {
                return ParseFullKey(keypfx);
            }
            else if ((state == ParserState.Keys) && (IsShortKey(descr, ar, out keypfx)))
            {
                return ParseShortKey(keypfx);
            }
            else
            {
                if (descr.stopKeyParsingOnValue)
                    state = ParserState.Values;
                inpIdx++;
                return ArgResult.AllocValue(ar);
            }
        }

        public static bool IsAnyKey(CliArgDescr descr, string data)
        {
            return IsShortKey(descr, data, out var _)
                || IsFullKey(descr, data, out var _);
        }

        // no null reference check is done. it's up to you to perform it before the call
        public static bool IsShortKey(CliArgDescr descr, string data, out string keyPfx)
        {
            keyPfx = "";
            if ((descr.shortKeyPrefix == null) || (descr.shortKeyPrefix.Length == 0))
                return false;

            foreach (var pfx in descr.shortKeyPrefix)
                if (data.StartsWith(pfx))
                {
                    keyPfx = pfx;
                    return true;
                }
            return false;
        }

        public static bool IsFullKey(CliArgDescr descr, string data, out string keyPfx)
        {
            keyPfx = "";
            if ((descr.fullKeyPrefix == null) || (descr.fullKeyPrefix.Length == 0))
                return false;

            foreach (var pfx in descr.fullKeyPrefix)
                if (data.StartsWith(pfx))
                {
                    keyPfx = pfx;
                    return true;
                }
            return false;
        }

        public static bool ExtractFullKey(string[] keyValSep, string data, string keypfx, out string key, ref string value)
        {
            if ((keyValSep == null)
                || (keyValSep.Length == 0))
            {
                key = data.Substring(keypfx.Length);
                return false;
            }

            int i = keypfx.Length;
            foreach(var s in keyValSep)
            {
                int j = data.IndexOf(s, i);
                if (j >= 0)
                {
                    j += s.Length;
                    key = data.Substring(i, j - i-1);
                    value = data.Substring(j);
                    return true;
                } else
                {
                    key = data.Substring(keypfx.Length);
                    return false;
                }
            }
            key = data.Substring(keypfx.Length);
            return false;
        }
    }
}
