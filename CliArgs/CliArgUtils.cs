using System;
using System.Collections.Generic;
using System.Text;

namespace CliArgs
{
    public static class CliArgUtils
    {
        public static bool ParseApply(string[] args, CliArgDescr descr, ICliArgApply apply)
        {
            ArgsParser p = new ArgsParser();
            p.SetDescr(descr);
            p.SetArgs(args);
            var res = p.FindNext();
            if (res == null)
                return false;

            int paramIdx = 0;
            apply.ParamsStart();
            while (res != null)
            {
                if (res.logicalKey != null)
                {
                    if (res.hasValue)
                        apply.ApplyKey(res.logicalKey, res.value);
                    else
                        apply.ApplyKeyNoVal(res.logicalKey);
                } else if (res.isKey)
                {
                    if (res.hasValue)
                        apply.UnknownKey(res.rawKeyName, res.value);
                    else
                        apply.UnknownKeyNoVal(res.rawKeyName);
                } else
                {
                    apply.AddValue(res.value, paramIdx);
                    paramIdx++;
                }
                res = p.FindNext();
            }
            apply.ParamsEnded();
            return true;
        }
    }
}
