using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace CliArgs
{
    // The reflection-based applicator of the parsed values
    public class CliArgRefApply: ICliArgApply
    {
        private object inst;
        private CliArgRefDescr descr;

        public List<PendingAssign> pending = new List<PendingAssign>();
        private Dictionary<MemberInfo, PendingAssign> memLookup = null;

        public CliArgRefApply(CliArgRefDescr descr, object instane)
        {
            this.inst = instane;
            this.descr = descr;
        }

        public void ParamsStart()
        { 
            // meh
        }
        public void ParamsEnded()
        {
            // meh
            if (pending.Count == 0) 
                return;

            foreach(var pa in pending)
            {
                var obj = pa.GetCurrentObject(inst);
                var newobj = pa.PopulateObject(obj);
                if (!Object.ReferenceEquals(obj, newobj))
                    pa.SetCurrentObject(obj, newobj);
            }
        }

        public void UnknownKey(string rawkey, string value)
        {
            // meh
        }

        public void UnknownKeyNoVal(string rawkey)
        {
            // meh
        }

        Dictionary<int, ValueByReflect> valLk = null;
        ValueByReflect defVal;

        private void ApplyToMemberBool(MemberInfo mm, bool boolVal)
        {
            if ((mm is FieldInfo fi)&& (fi.FieldType == typeof(bool)))
            {
                object fobj;
                fobj = fi.GetValue(inst);
                fi.SetValue(fobj, boolVal);
            } 
            else if ((mm is PropertyInfo pi) && (pi.PropertyType == typeof(bool)))
            {
                // todo: check for read-only properties
                if (pi.CanWrite)
                {
                    object fobj;
                    fobj = pi.GetValue(inst);
                    pi.SetValue(fobj, boolVal);
                }
            }
        }

        private object ConvertToTargetType(string val, Type trgType)
        {
            if (trgType == typeof(sbyte))
                return Convert.ToSByte(val);
            if (trgType == typeof(short))
                return Convert.ToInt16(val);
            else if (trgType == typeof(int))
                return Convert.ToInt32(val);
            else if (trgType == typeof(long))
                return Convert.ToInt64(val);
            else if (trgType == typeof(byte))
                return Convert.ToByte(val);
            else if (trgType == typeof(UInt16))
                return Convert.ToUInt16(val);
            else if (trgType == typeof(UInt32))
                return Convert.ToUInt32(val);
            else if (trgType == typeof(UInt64))
                return Convert.ToUInt64(val);
            else if (trgType == typeof(double))
            {
                double d;
                if (double.TryParse(val, out d))
                    return d;
            }
            else if (trgType == typeof(float))
            {
                float f;
                if (float.TryParse(val, out f))
                    return f;
            }
            return val;
        }

        private bool isPendingWanted(object vobj, Type trgType)
        {
            if (trgType == typeof(string))
                return false;
            return (trgType.IsArray)
                || (trgType.IsClass);
        }

        void SchedulePending(PropertyInfo pi, FieldInfo fi, string val)
        {
            MemberInfo m = (pi == null) ? (MemberInfo)fi : (MemberInfo)pi;
            if (memLookup == null)
            {
                memLookup = 
                    new Dictionary<MemberInfo, PendingAssign>();
            }
            PendingAssign pa;
            if (!memLookup.TryGetValue(m, out pa))
            {
                if (pi == null) pa = new PendingField(fi);
                else pa = new PendingProperty(pi);
                memLookup[m] = pa;
                pending.Add(pa);
            }
            pa.collected.Add(val);
        }

        private void ApplyToMember(MemberInfo mm, string val)
        {
            if (mm is FieldInfo fi)
            {
                if (isPendingWanted(fi, fi.FieldType))
                {
                    SchedulePending(null, fi, val);
                }
                else
                {
                    object fobj;
                    fobj = fi.GetValue(inst);
                    object vobj = ConvertToTargetType(val, fi.FieldType);
                    fi.SetValue(fobj, vobj);
                }
            }
            else if (mm is PropertyInfo pi)
            {
                // todo: check for read-only properties
                if (isPendingWanted(pi, pi.PropertyType))
                {
                    SchedulePending(pi, null, val);
                }
                else
                {
                    if (!pi.CanWrite) return;
                    object fobj;
                    fobj = pi.GetValue(inst);
                    object vobj = ConvertToTargetType(val, pi.PropertyType);
                    pi.SetValue(fobj, vobj);
                }
            }
        }

        public void AddValue(string val, int valIdx)
        {
            if (valLk == null)
            {
                valLk = new Dictionary<int, ValueByReflect>();
                foreach (var vl in descr.vals)
                {
                    valLk[vl.index] = vl;
                    if (vl.index < 0) defVal = vl;
                }
            }
            ValueByReflect v;
            if (!valLk.TryGetValue(valIdx, out v))
                v = defVal;
            if (v == null) return;
            ApplyToMember(v.member, val);
        }

        public void ApplyKeyNoVal(CliArgKey key)
        {
            var mm = key.tag as MemberInfo;
            if (mm != null)
                ApplyToMemberBool(mm, true);
        }

        public void ApplyKey(CliArgKey key, string value)
        {
            var mm = key.tag as MemberInfo;
            if (mm != null)
                ApplyToMember(mm, value);
        }

        public void ActionStart(string action, bool isKnown, CliArgDescr logicalDescr)
        {

            CliArgRefDescr r = logicalDescr as CliArgRefDescr;
            if (r == null) r = descr;
            if (r == null) return;

            ValueByReflect actval = r.defAction;
            if (isKnown)
            {
                r.acts.TryGetValue(action, out actval);
            }
            if (actval == null) return;
            
            if (actval.member.DeclaringType == typeof(bool))
                ApplyToMemberBool(actval.member, true);
            else 
                ApplyToMember(actval.member, action);
        }

    }

    public abstract class PendingAssign
    {
        public Type endObjType;
        public List<string> collected = new List<string>();

        public object PopulateObject(object current)
        {
            if (RefUtils.IsStringArray(endObjType))
                // arrays are read-only we have to recreate them
                return collected.ToArray();

            // checking ICollection<string> interface. It's implemented by IList
            if (RefUtils.IsCollectionStr(endObjType))
            {
                if (current == null)
                    current = Activator.CreateInstance(endObjType);
                var icoll = current as ICollection<string>;
                foreach (var s in collected)
                    icoll.Add(s);
                return current;
            }

            return current;
        }
        public abstract object GetCurrentObject(object host);

        public abstract void SetCurrentObject(object host, object newval);
    }

    public class PendingField : PendingAssign
    {
        public FieldInfo field;
        public PendingField(FieldInfo f)
        {
            field = f;
            endObjType = f.FieldType;
        }
        public override object GetCurrentObject(object host)
        {
            return field.GetValue(host);
        }
        public override void SetCurrentObject(object host, object newval)
        {
            field.SetValue(host, newval);
        }

    }
    public class PendingProperty : PendingAssign
    {
        public PropertyInfo prop;
        public PendingProperty(PropertyInfo f)
        {
            prop = f;
            endObjType = f.PropertyType;
        }
        public override object GetCurrentObject(object host)
        {
            return prop.GetValue(host);
        }
        public override void SetCurrentObject(object host, object newval)
        {
            prop.SetValue(host, newval);
        }
    }

}
