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
        protected override Exp VisitTaggedTemplateExpression(AstTaggedTemplateExpression template)
        {
            var parts = pool.AllocateList<Expression>(template.Arguments.Length);
            var raw = pool.AllocateList<Expression>(template.Arguments.Length);
            var expressions = pool.AllocateList<Expression>(template.Arguments.Length);
            try
            {
                var e = template.Arguments.GetEnumerator();
                while(e.MoveNext(out var p)) {
                    if(p.Type == FastNodeType.Literal)
                    {
                        var l = p as AstLiteral;
                        if(l.TokenType == TokenTypes.TemplatePart)
                        {
                            raw.Add(Expression.Constant(l.Start.Span.Value));
                            parts.Add(Expression.Constant(l.StringValue));
                            continue;
                        }
                    }
                    expressions.Add(VisitExpression(p));
                }
                var a = JSTemplateArrayBuilder.New(parts, raw, expressions);
                return a;
            } finally {
                parts.Clear();
                raw.Clear();
                expressions.Clear();
            }
        }
    }
}