﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using YantraJS.Core;
using AssignmentOperator = Esprima.Ast.AssignmentOperator;

namespace YantraJS.Utils
{
    public delegate Expression CaseExpression(ParameterExpression pe);

    public class SwitchExpression
    {
        protected static TypeCheckCase Case<T>(in CaseExpression e)
        {
            return new TypeCheckCase
            {
                Type = typeof(T),
                TrueCase = e
            };
        }

        protected static TypeCheckCase Case(Type type, CaseExpression e)
        {
            return new TypeCheckCase
            {
                Type = type,
                TrueCase = e
            };
        }

        protected static TypeCheckCase Default(Expression e)
        {
            return new TypeCheckCase
            {
                Type = null,
                TrueCase = (a) => e
            };
        }

        public class TypeCheckCase
        {
            public Type Type { get; set; }

            public CaseExpression TrueCase { get; set; }

        }

        protected static Expression
            Switch(Expression right,
                    params TypeCheckCase[] cases)
        {

            var defaultCase = cases.First(x => x.Type == null);
            var allCases = cases.Where(x => x.Type != null);

            Expression condition = defaultCase.TrueCase(null);
            foreach (var @case in allCases)
            {
                var bp = Expression.Parameter(@case.Type);

                if (@case.Type.IsValueType)
                {
                    condition = Expression.Condition(
                        Expression.TypeIs(right, @case.Type),
                        Expression.Block(new ParameterExpression[] { bp },
                            @case.TrueCase(bp)
                        ),
                        condition,
                        typeof(JSValue)
                        );
                    continue;
                }

                var nbt = Expression.Constant(null, @case.Type);
                condition = Expression.Block(new ParameterExpression[] { bp },
                    Expression.Assign(bp, Expression.TypeAs(right, @case.Type)),
                    Expression.Condition(
                        Expression.NotEqual(nbt, bp),
                        @case.TrueCase(bp),
                        condition,
                        typeof(JSValue)
                    ));
            }
            return condition;
        }
    }

    public class BinaryOperation: SwitchExpression
    {


        public static Expression Assign(Expression left, Expression right, AssignmentOperator assignmentOperator)
        {



            switch(assignmentOperator)
            {
                case AssignmentOperator.Assign:
                    return Assign(left, right);
                case AssignmentOperator.PlusAssign:
                    return Assign(left, ExpHelper.JSValueBuilder.Add(left,right));
            }

            var leftDouble = ExpHelper.JSValueBuilder.DoubleValue(left);
            var rightDouble = ExpHelper.JSValueBuilder.DoubleValue(right);

            var leftInt = Expression.Convert(leftDouble, typeof(int));
            var rightInt = Expression.Convert(rightDouble, typeof(int));

            var rightUInt = Expression.Convert(rightDouble, typeof(uint));

            // convert to double...
            switch (assignmentOperator)
            {
                case AssignmentOperator.MinusAssign:
                    return Assign(left, ExpHelper.JSNumberBuilder.New(Expression.Add(leftDouble, rightDouble)));
                case AssignmentOperator.TimesAssign:
                    return Assign(left, ExpHelper.JSNumberBuilder.New(Expression.Multiply(leftDouble, rightDouble)));
                case AssignmentOperator.DivideAssign:
                    return Assign(left, ExpHelper.JSNumberBuilder.New(Expression.Divide(leftDouble, rightDouble)));
                case AssignmentOperator.ModuloAssign:
                    return Assign(left, ExpHelper.JSNumberBuilder.New(Expression.Modulo(leftDouble, rightDouble)));
                case AssignmentOperator.BitwiseAndAssign:
                    return Assign(left, ExpHelper.JSNumberBuilder.New(Expression.And(leftInt, rightInt)));
                case AssignmentOperator.BitwiseOrAssign:
                    return Assign(left, ExpHelper.JSNumberBuilder.New(Expression.Or(leftInt, rightInt)));
                case AssignmentOperator.BitwiseXOrAssign:
                    return Assign(left, ExpHelper.JSNumberBuilder.New(Expression.ExclusiveOr(leftInt, rightInt)));
                case AssignmentOperator.LeftShiftAssign:
                    return Assign(left, ExpHelper.JSNumberBuilder.New(Expression.LeftShift(leftInt, rightInt)));
                case AssignmentOperator.RightShiftAssign:
                    return Assign(left, ExpHelper.JSNumberBuilder.New(Expression.RightShift(leftInt, rightInt)));
                case AssignmentOperator.UnsignedRightShiftAssign:
                    return Assign(left, ExpHelper.JSNumberBuilder.New(Expression.RightShift(leftInt, rightUInt)));
                case AssignmentOperator.ExponentiationAssign:
                    return Assign(left, ExpHelper.JSNumberBuilder.New(Expression.Power(leftInt, rightInt)));
            }

            throw new NotSupportedException();
        }

        private static Expression Assign(Expression left, Expression right)
        {
            return ExpHelper.JSValueExtensionsBuilder.Assign(left, right);
        }

        #region Add

        //public static Expression Add(Expression leftExp, Expression right)
        //{
        //    object obj = 4;

        //    var undefined = Expression.Constant("undefined");
        //    var @null = Expression.Constant("null");
        //    var nan = ExpHelper.JSContext.NaN;
        //    var zero = ExpHelper.JSContext.Zero;
        //    CaseExpression caseUndefined = (left) =>
        //        Switch(right,
        //        Case<JSUndefined>(x => nan),
        //        Case<JSNumber>(x => nan),
        //        Case<double>(x => nan),
        //        Case<string>(x => ExpHelper.JSString.ConcatBasicStrings(undefined, x)),
        //        Default(ExpHelper.JSString.ConcatBasicStrings(undefined, ExpHelper.Object.ToString(right)))
        //        );

        //    CaseExpression caseNull = (left) =>
        //        Switch(right,
        //        Case<JSUndefined>(x => nan),
        //        Case<JSNull>(x => zero),
        //        Case<JSNumber>(x => right),
        //        Case<double>(x => ExpHelper.JSNumber.New(x)),
        //        Case<string>(x => ExpHelper.JSString.ConcatBasicStrings(@null, x)),
        //        Default(ExpHelper.JSString.ConcatBasicStrings(@null, ExpHelper.Object.ToString(right)))
        //        );

        //    // string case avoids toString  of JSString by accessing value directly...
        //    CaseExpression caseJSString = (left) =>
        //        Switch(right,
        //        Case<JSUndefined>(x => ExpHelper.JSString.ConcatBasicStrings(ExpHelper.JSString.Value(left), undefined)),
        //        Case<JSNull>(x => ExpHelper.JSString.ConcatBasicStrings(ExpHelper.JSString.Value(left), @null)),
        //        Case<JSNumber>(x => ExpHelper.JSString.ConcatBasicStrings(
        //            ExpHelper.JSString.Value(left),
        //            ExpHelper.Object.ToString(ExpHelper.JSNumber.Value(x)))),
        //        Case<double>(x => ExpHelper.JSString.ConcatBasicStrings(
        //            ExpHelper.JSString.Value(left),
        //            ExpHelper.Object.ToString(x))),
        //        Case<string>(x => ExpHelper.JSString.ConcatBasicStrings(@null, x)),
        //        Default(ExpHelper.JSString.ConcatBasicStrings(@null, ExpHelper.Object.ToString(right)))
        //        );

        //    // JSNumber is the most complicated one, and will be too big, so we will
        //    // call a method on it ..
        //    // also it should be the first case as most likely we will add numbers and strings...
        //    CaseExpression caseJSNumber = (left) =>
        //        ExpHelper.JSNumber.AddValue(left, right);

        //    var StringAdd =
        //        ExpHelper.JSString.ConcatBasicStrings(
        //                ExpHelper.Object.ToString(leftExp),
        //                ExpHelper.Object.ToString(right)
        //                );

        //    return Switch(leftExp,
        //            Case<JSNumber>(caseJSNumber),
        //            Case<JSString>(caseJSString),
        //            Case<JSUndefined>(caseUndefined),
        //            Case<JSNull>(caseNull),
        //            Default(StringAdd)
        //        );


        //}
        #endregion
        public static Expression Operation(Expression left, Expression right, Esprima.Ast.BinaryOperator op)
        {
            var leftDouble = ExpHelper.JSValueBuilder.DoubleValue(left);
            var rightDouble = ExpHelper.JSValueBuilder.DoubleValue(right);

            var leftInt = Expression.Convert(leftDouble, typeof(int));
            var rightInt = Expression.Convert(rightDouble, typeof(int));

            var rightUInt = Expression.Convert(rightDouble, typeof(uint));

            switch (op)
            {
                case Esprima.Ast.BinaryOperator.StrictlyEqual:
                    return ExpHelper.JSValueBuilder.StrictEquals(left, right);
                case Esprima.Ast.BinaryOperator.StricltyNotEqual:
                    return ExpHelper.JSValueBuilder.NotStrictEquals(left, right);
                case Esprima.Ast.BinaryOperator.InstanceOf:
                    return ExpHelper.JSValueExtensionsBuilder.InstanceOf(left, right);
                case Esprima.Ast.BinaryOperator.In:
                    return ExpHelper.JSValueExtensionsBuilder.IsIn(left, right);
                case Esprima.Ast.BinaryOperator.Plus:
                    return ExpHelper.JSValueBuilder.Add(left, right);
                case Esprima.Ast.BinaryOperator.Minus:
                    return ExpHelper.JSNumberBuilder.New(Expression.Subtract(leftDouble, rightDouble));
                case Esprima.Ast.BinaryOperator.Times:
                    return ExpHelper.JSNumberBuilder.New(Expression.Multiply(leftDouble, rightDouble));
                case Esprima.Ast.BinaryOperator.Divide:
                    return ExpHelper.JSNumberBuilder.New(Expression.Divide(leftDouble, rightDouble));
                case Esprima.Ast.BinaryOperator.Modulo:
                    return ExpHelper.JSNumberBuilder.New(Expression.Modulo(leftDouble, rightDouble));
                case Esprima.Ast.BinaryOperator.Equal:
                    return ExpHelper.JSValueBuilder.Equals(left, right);
                case Esprima.Ast.BinaryOperator.NotEqual:
                    return ExpHelper.JSValueBuilder.NotEquals(left, right);
                case Esprima.Ast.BinaryOperator.Greater:
                    return ExpHelper.JSValueBuilder.Greater(left, right);
                case Esprima.Ast.BinaryOperator.GreaterOrEqual:
                    return ExpHelper.JSValueBuilder.GreaterOrEqual(left, right);
                case Esprima.Ast.BinaryOperator.Less:
                    return ExpHelper.JSValueBuilder.Less(left, right);
                case Esprima.Ast.BinaryOperator.LessOrEqual:
                    return ExpHelper.JSValueBuilder.LessOrEqual(left, right);
                case Esprima.Ast.BinaryOperator.BitwiseAnd:
                    return ExpHelper.JSNumberBuilder.New(Expression.And(leftInt, rightInt));
                case Esprima.Ast.BinaryOperator.BitwiseOr:
                    return ExpHelper.JSNumberBuilder.New(Expression.Or(leftInt, rightInt));
                case Esprima.Ast.BinaryOperator.BitwiseXOr:
                    return ExpHelper.JSNumberBuilder.New(Expression.ExclusiveOr(leftInt, rightInt));
                case Esprima.Ast.BinaryOperator.LeftShift:
                    return ExpHelper.JSNumberBuilder.New(Expression.LeftShift(leftInt, rightInt));
                case Esprima.Ast.BinaryOperator.RightShift:
                    return ExpHelper.JSNumberBuilder.New(Expression.RightShift(leftInt, rightInt));
                case Esprima.Ast.BinaryOperator.UnsignedRightShift:
                    return ExpHelper.JSNumberBuilder.New(
                        Expression.RightShift(
                            Expression.Convert(leftInt, typeof(uint)), rightInt));
                case Esprima.Ast.BinaryOperator.LogicalAnd:
                    return ExpHelper.JSValueBuilder.LogicalAnd(left, right);
                case Esprima.Ast.BinaryOperator.LogicalOr:
                    return ExpHelper.JSValueBuilder.LogicalOr(left, right);
                case Esprima.Ast.BinaryOperator.Exponentiation:
                    return ExpHelper.JSNumberBuilder.New(Expression.Power(leftDouble, rightDouble));
            }
            throw new NotImplementedException();
        }





    }
}