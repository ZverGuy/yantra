﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using YantraJS.Core.LinqExpressions;
using YantraJS.ExpHelper;

using Exp = YantraJS.Expressions.YExpression;
using Expression = YantraJS.Expressions.YExpression;
using ParameterExpression = YantraJS.Expressions.YParameterExpression;
using LambdaExpression = YantraJS.Expressions.YLambdaExpression;
using LabelTarget = YantraJS.Expressions.YLabelTarget;
using SwitchCase = YantraJS.Expressions.YSwitchCaseExpression;
using GotoExpression = YantraJS.Expressions.YGoToExpression;
using TryExpression = YantraJS.Expressions.YTryCatchFinallyExpression;

namespace YantraJS.Core.FastParser.Compiler
{
    partial class FastCompiler
    {

        protected override Expression VisitLiteral(AstLiteral literal)
        {
            switch (literal.TokenType)
            {
                case TokenTypes.True:
                    return ExpHelper.JSBooleanBuilder.True;
                case TokenTypes.False:
                    return ExpHelper.JSBooleanBuilder.False;
                case TokenTypes.String:
                    return ExpHelper.JSStringBuilder.New(Exp.Constant(literal.StringValue));
                case TokenTypes.RegExLiteral:
                    return ExpHelper.JSRegExpBuilder.New(
                        Exp.Constant(literal.Regex.Pattern),
                        Exp.Constant(literal.Regex.Flags));
                case TokenTypes.Null:
                    return ExpHelper.JSNullBuilder.Value;
                case TokenTypes.Number:
                    return ExpHelper.JSNumberBuilder.New(Exp.Constant(literal.NumericValue));
            }
            throw new NotImplementedException();
        }
    }
}