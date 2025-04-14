using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace CliArgs
{

    // parsing the information of the specified type using the reflected attributes (see CliAtgRefAttr.cs)
    public class CliArgRefDescr : CliArgDescr
    {
        public Type rootType;
        public List<ValueByReflect> vals = new List<ValueByReflect>();
        public CliArgRefDescr()
        {

        }

        public CliArgRefDescr(CliArgDescr baseDecl) : base(baseDecl)
        {
        }

        private static string[] ToArray(IEnumerable<CliFullAttribute> list)
        {
            List<string> result = new List<string>();
            foreach (var a in list)
            {
                if (string.IsNullOrEmpty(a.Key))
                    continue;
                result.Add(a.Key);
            }
            return result.ToArray();
        }
        private static string[] ToArray(IEnumerable<CliShortAttribute> list)
        {
            List<string> result = new List<string>();
            foreach (var a in list)
            {
                if (string.IsNullOrEmpty(a.Key))
                    continue;
                result.Add(a.Key);
            }
            return result.ToArray();
        }

        public static void PopulateKeys(Type tp, CliArgDescr dst, List<ValueByReflect> vals)
        {
            List<CliArgKey> keys = new List<CliArgKey>();

            
            foreach (var mm in tp.GetMembers(BindingFlags.Public 
                | BindingFlags.NonPublic 
                | BindingFlags.Static 
                | BindingFlags.Instance
                ))
            {
                if ((mm.MemberType != System.Reflection.MemberTypes.Property)
                    && (mm.MemberType != System.Reflection.MemberTypes.Field))
                    continue;

                List<CliFullAttribute> fl = null;
                List<CliShortAttribute> sl = null;
                List<CliValueAttribute> vl = null;
                StringBuilder hl = null;

                object[] att = mm.GetCustomAttributes(true);
                foreach (var a in att)
                {
                    if (a is CliFullAttribute f)
                    {
                        if (fl == null) fl = new List<CliFullAttribute>();
                        fl.Add(f);
                    }
                    else if (a is CliShortAttribute s)
                    {
                        if (sl == null) sl = new List<CliShortAttribute>();
                        sl.Add(s);
                    }
                    else if (a is CliValueAttribute v)
                    {
                        if (vl == null) vl = new List<CliValueAttribute>();
                        vl.Add(v);
                    }
                    else if (a is CliHelpAttribute h)
                    {
                        if (hl == null) hl = new StringBuilder();
                        hl.AppendLine(h.Line);
                    }
                }


                if ((fl != null) || (sl != null))
                {
                    CliArgKey arg = new CliArgKey();
                    arg.logicalName = mm.Name;
                    if (fl != null) arg.fullKeys = ToArray(fl);
                    if (sl != null) arg.shortKeys = ToArray(sl);
                    arg.tag = mm;
                    if (hl != null)
                        arg.helpDescr = hl.ToString().TrimEnd();

                    arg.isBoolOnly = ((mm is FieldInfo fi) && (fi.FieldType == typeof(bool)))
                        || ((mm is PropertyInfo pi) && (pi.PropertyType == typeof(bool)));

                    keys.Add(arg);
                }
                else if (vl != null)
                {
                    var v = vl[0];
                    vals.Add(new ValueByReflect(mm, v.Index));
                }
            }
            dst.logicalKeys = keys.ToArray();
        }

        public static CliArgRefDescr Alloc(Type tp, CliArgDescr basedecl = null)
        {
            if (basedecl == null)
                basedecl = CliArgDescr.Default2000;
            CliArgRefDescr result = new CliArgRefDescr(basedecl);
            result.rootType = tp;
            CliArgRefDescr.PopulateKeys(tp, result, result.vals);
            return result;
        }


    }

    public class ValueByReflect
    {
        public MemberInfo member;
        public int index;
        public ValueByReflect()
        {

        }
        public ValueByReflect(MemberInfo member, int index)
        {
            this.member = member;
            this.index = index;
        }
    }

}
