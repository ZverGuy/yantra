﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using WebAtoms.CoreJS.Core;

namespace WebAtoms.CoreJS.ExpHelper
{
    public class JSObjectBuilder
    {
        private static Type type = typeof(JSObject);

        private static FieldInfo _ownProperties =
            type.InternalField(nameof(Core.JSObject.ownProperties));

        private static PropertyInfo _Index =
            typeof(BaseMap<uint, Core.JSProperty>)
                .GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(x => x.GetIndexParameters().Length > 0);

        private static MethodInfo _NewWithProperties =
            type.StaticMethod(nameof(JSObject.NewWithProperties));

        private static MethodInfo _NewWithElements =
            type.StaticMethod(nameof(JSObject.NewWithElements));

        private static MethodInfo _NewWithPropertiesAndElements =
            type.StaticMethod(nameof(JSObject.NewWithPropertiesAndElements));

        private static MethodInfo _AddElement =
            type.InternalMethod(nameof(JSObject.AddElement), new Type[] { typeof(uint), typeof(JSValue) });

        private static MethodInfo _AddProperty =
            type.InternalMethod(nameof(JSObject.AddProperty), new Type[] { typeof(KeyString), typeof(JSValue) });

        private static MethodInfo _AddExpressionProperty =
            type.InternalMethod(nameof(JSObject.AddProperty), new Type[] { typeof(JSValue), typeof(JSValue) });

        private static MethodInfo _AddPropertyAccessors =
            type.InternalMethod(nameof(JSObject.AddProperty), new Type[] { typeof(KeyString), typeof(JSFunction), typeof(JSFunction) });


        public static Expression New(IList<ExpressionHolder> keyValues)
        {
            // let the default be NewWithProperties
            bool addProperties = false;
            bool addElements = false;
            // choose best create method...
            foreach (var px in keyValues)
            {
                if (px.Key.Type == typeof(uint))
                {
                    addElements = true;
                    continue;
                }
                if (px.Key.Type == typeof(KeyString))
                {
                    addProperties = true;
                    continue;
                }
            }

            MethodInfo _new = null;
            if (addProperties && addElements)
            {
                _new = _NewWithPropertiesAndElements;
            }
            else
            {
                if (addProperties)
                {
                    _new = _NewWithProperties;
                }
                else
                {
                    _new = _NewWithElements;
                }
            }

            Expression _newObj = Expression.Call(null, _new);

            foreach (var px in keyValues)
            {
                if (px.Key.Type == typeof(uint))
                {
                    _newObj = Expression.Call(_newObj, _AddElement, px.Key, px.Value);
                    continue;
                }
                if (px.Key.Type == typeof(KeyString))
                {
                    if (px.Value != null)
                    {
                        _newObj = Expression.Call(_newObj, _AddProperty, px.Key, px.Value);
                    }
                    else
                    {
                        _newObj = Expression.Call(_newObj, _AddPropertyAccessors, px.Key, px.Getter, px.Setter);
                    }
                    continue;
                }
                _newObj = Expression.Call(_newObj, _AddExpressionProperty, px.Key, px.Value);
            }
            return _newObj;
        }

    }
}