using System;
using System.Collections.Generic;
using System.Text;

namespace CliArgs
{
    // The generic interface to apply parsed values
    public interface ICliArgApply
    {
        // an "action" has been identified
        // "isKnown" flag indicates if the action string is a part of the "known" actions
        void ActionStart(string action, bool isKnown, CliArgDescr descr);

        // called before any other paramter, but only if some arguments were provided
        // if no arguments were provided the method is never called
        void ParamsStart();
        // a simple value, if the index (valIdx is zero based)
        void AddValue(string val, int valIdx);

        // apply a known key w/o value (boolean switch)
        void ApplyKeyNoVal(CliArgKey key);
        // apply known key
        void ApplyKey(CliArgKey key, string value);

        // unknown key with a value is found
        void UnknownKey(string rawkey, string value);

        // unknown key is found
        void UnknownKeyNoVal(string rawkey);

        // we're done parsing the arguments
        void ParamsEnded();
    }
}
