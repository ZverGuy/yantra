using System;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using YantraJS.Utils;

namespace YantraJS.Core
{
    internal class JSDynamicMetaData : DynamicMetaObject
    {
        internal static MethodInfo _createArguments =
            typeof(JSDynamicMetaData).GetMethod("__CreateArguments");

        internal static MethodInfo _invokeMember =
            typeof(JSDynamicMetaData).GetMethod("__InvokeMember");

        internal static MethodInfo _setMethod =
            typeof(JSDynamicMetaData).GetMethod("__SetMethod");

        internal static MethodInfo _getMethod =
            typeof(JSDynamicMetaData).GetMethod("__GetMethod");

        public static object __InvokeMember(JSValue target, string name, params JSValue[] a)
        {
            if (name == "ToString")
                return target.ToString();
            return target.InvokeMethod(name, new Arguments(JSContext.Current, target, a));
        }

        public static JSValue[] __CreateArguments(object[] args)
        {
            var alist = args.Select((p) =>
            {
                if (p == null)
                    return JSNull.Value;
                switch (p)
                {
                    case double d:
                        return new JSNumber(d);
                    case int i:
                        return new JSNumber(i);
                    case float f:
                        return new JSNumber(f);
                    case decimal ds:
                        return new JSNumber((double)ds);
                    case bool b:
                        return b ? JSBoolean.True : JSBoolean.False;
                    case string s:
                        return new JSString(s);
                    case JSValue v:
                        return v;
                    default:
                        throw new NotSupportedException($"Cannot convert type {p.GetType()} to JSValue");
                }
            }).ToList();
            return alist.ToArray();
        }

        public static object __GetMethod(JSValue value, object name)
        {
            if (name == null)
                throw new ArgumentNullException();
            var c = JSContext.Current;
            switch (name)
            {
                case string s: return value[c,s];
                case uint ui: return value[c,ui];
                case int i: return value[c,(uint)i];
                case double d: return value[c,(uint)d];
                case decimal d1: return value[c,(uint)d1];
                case float f1: return value[c,(uint)f1];
                case JSNumber jn: return value[c,(uint)jn.value];
                case JSString js: return value[c,js.ToKey()];
            }
            return value[c,name.ToString()];
        }

        public static JSValue __SetMethod(JSValue target, object name, object _value)
        {
            if (name == null)
                throw new ArgumentNullException();
            JSValue value = TypeConverter.FromBasic(_value);
            var c = JSContext.Current;
            switch (name)
            {
                case string s: return target[c, s] = value;
                case uint ui: return target[c, ui] = value;
                case int i: return target[c, (uint)i] = value;
                case double d: return target[c, (uint)d] = value;
                case decimal d1: return target[c, (uint)d1] = value;
                case float f1: return target[c, (uint)f1] = value;
                case JSNumber jn: return target[c, (uint)jn.value] = value;
                case JSString js: return target[c, js.ToKey()] = value;
            }
            return target[c, name.ToString()] = value;
        }


        internal JSDynamicMetaData(
            System.Linq.Expressions.Expression parameter,
            JSValue value) : base(parameter, BindingRestrictions.Empty, value)
        {

        }

        public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
        {
            BindingRestrictions restrictions =
                BindingRestrictions.GetTypeRestriction(Expression, LimitType);

            Expression self = Expression.Convert(Expression, LimitType);
            Expression p0 = Expression.Convert(value.Expression, typeof(object));
            Expression name = Expression.Constant(binder.Name);
            Expression call = Expression.Call(_setMethod, self, name, p0);

            return new DynamicMetaObject(call, restrictions);

        }

        public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
        {
            BindingRestrictions restrictions =
                BindingRestrictions.GetTypeRestriction(Expression, LimitType);

            Expression self = Expression.Convert(Expression, LimitType);
            Expression name = Expression.Constant(binder.Name);
            Expression call = Expression.Call(_getMethod, self, name);

            return new DynamicMetaObject(call, restrictions);
        }

        public override DynamicMetaObject BindSetIndex(
            SetIndexBinder binder,
            DynamicMetaObject[] indexes,
            DynamicMetaObject value)
        {
            BindingRestrictions restrictions =
                BindingRestrictions.GetTypeRestriction(Expression, LimitType);

            Expression self = Expression.Convert(Expression, LimitType);
            Expression p0 = Expression.Convert(value.Expression, typeof(object));
            Expression name = Expression.Convert(indexes[0].Expression, typeof(object));
            Expression call = Expression.Call(_setMethod, self, name, p0);

            return new DynamicMetaObject(call, restrictions);
        }

        public override DynamicMetaObject BindGetIndex(GetIndexBinder binder, DynamicMetaObject[] indexes)
        {
            BindingRestrictions restrictions =
                BindingRestrictions.GetTypeRestriction(Expression, LimitType);

            Expression self = Expression.Convert(Expression, LimitType);
            Expression name = Expression.Convert(indexes[0].Expression, typeof(object));
            Expression call = Expression.Call(_getMethod, self, name);

            return new DynamicMetaObject(call, restrictions);
        }

        public override DynamicMetaObject BindInvokeMember(
            InvokeMemberBinder binder,
            DynamicMetaObject[] args)
        {

            BindingRestrictions restrictions =
                BindingRestrictions.GetTypeRestriction(Expression, LimitType);

            var argList = Expression.NewArrayInit(typeof(object),
                args.Select(x => Expression.Convert(x.Expression, typeof(object))).ToArray());

            var ce = Expression.Call(_createArguments, argList);

            Expression self = Expression.Convert(Expression, LimitType);
            Expression name = Expression.Constant(binder.Name);
            Expression call = Expression.Call(_invokeMember, self, name, ce);

            return new DynamicMetaObject(call, restrictions);
        }
    }
}
