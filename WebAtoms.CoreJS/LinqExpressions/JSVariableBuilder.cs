﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using WebAtoms.CoreJS.Core;

namespace WebAtoms.CoreJS.ExpHelper
{
    public class JSVariableBuilder
    {
        private static Type type = typeof(JSVariable);

        private static ConstructorInfo _New
            = type.Constructor(typeof(JSValue), typeof(string));

        public static Expression New(Expression value, string name)
        {
            return Expression.New(_New, value, Expression.Constant(name, typeof(string)));
        }

        private static ConstructorInfo _NewFromException
            = type.Constructor(typeof(Exception), typeof(string));

        public static Expression NewFromException(Expression value, string name)
        {
            return Expression.New(_NewFromException, value, Expression.Constant(name, typeof(string)));
        }

        private static MethodInfo _NewFromArgument
            = type.InternalMethod(nameof(JSVariable.New), typeof(Arguments).MakeByRefType(), typeof(int), typeof(string));

        public static Expression FromArgument(Expression args, int i, string name)
        {
            return Expression.Call(null, _NewFromArgument, args, Expression.Constant(i), Expression.Constant(name));
        }


        public static Expression New(string name)
        {
            return Expression.New(_New, ExpHelper.JSUndefinedBuilder.Value, Expression.Constant(name));
        }

    }
}