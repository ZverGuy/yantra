﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using YantraJS.Core;

namespace YantraJS.ExpHelper
{
    internal class KeyStringsBuilder
    {
        private static MethodInfo _GetOrAdd =
            typeof(KeyStrings).InternalMethod(nameof(KeyStrings.GetOrCreate), typeof(string));

        public static Expression GetOrCreate(Expression text)
        {
            return Expression.Call(null, _GetOrAdd, text);
        }
    }
}