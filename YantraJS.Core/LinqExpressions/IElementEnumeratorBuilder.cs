﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using YantraJS.Core;

using Exp = YantraJS.Expressions.YExpression;
using Expression = YantraJS.Expressions.YExpression;
using ParameterExpression = YantraJS.Expressions.YParameterExpression;
using LambdaExpression = YantraJS.Expressions.YLambdaExpression;
using LabelTarget = YantraJS.Expressions.YLabelTarget;
using SwitchCase = YantraJS.Expressions.YSwitchCaseExpression;
using GotoExpression = YantraJS.Expressions.YGoToExpression;
using TryExpression = YantraJS.Expressions.YTryCatchFinallyExpression;

namespace YantraJS.ExpHelper
{
    public class IElementEnumeratorBuilder
    {
        private static readonly Type type = typeof(IElementEnumerator);

        private static MethodInfo getMethod =
            typeof(JSValue).PublicMethod(
                nameof(JSValue.GetElementEnumerator));

        private static MethodInfo moveNext =
            type.PublicMethod(nameof(IElementEnumerator.MoveNext), typeof(JSValue).MakeByRefType());

        private static MethodInfo moveNextOrDefault =
            type.PublicMethod(nameof(IElementEnumerator.MoveNextOrDefault), 
                typeof(JSValue).MakeByRefType(),
                typeof(JSValue));


        public static Expression Get(Expression target)
        {
            if (typeof(JSValue).IsAssignableFrom(target.Type))
            {
                return Expression.Call(target, getMethod);
            }
            if (ArgumentsBuilder.refType == target.Type || target.Type == typeof(Arguments))
                return ArgumentsBuilder.GetElementEnumerator(target);
            throw new NotImplementedException();
        }

        public static Expression MoveNext(Expression target, Expression item)
        {
            return Expression.Call(target, moveNext, item);
        }

        public static Expression AssignMoveNext(
            Expression assignee,
            Expression target)
        {
            return Expression.Call(target, moveNextOrDefault, assignee, JSUndefinedBuilder.Value);
        }
    }
}
