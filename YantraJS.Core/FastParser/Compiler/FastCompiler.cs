﻿using System;
using System.Collections.Generic;
using YantraJS.Core.LinqExpressions;
using YantraJS.Emit;
using YantraJS.ExpHelper;
using YantraJS.Expressions;
using YantraJS.Utils;
using Exp = YantraJS.Expressions.YExpression;
using Expression = YantraJS.Expressions.YExpression;
using ParameterExpression = YantraJS.Expressions.YParameterExpression;

namespace YantraJS.Core.FastParser.Compiler
{
    public partial class FastCompiler : AstMapVisitor<YExpression>
    {

        private readonly FastPool pool;

        readonly LinkedStack<FastFunctionScope> scope = new LinkedStack<FastFunctionScope>();
        private readonly string location;

        public LoopScope LoopScope => this.scope.Top.Loop.Top;

        private StringArray _keyStrings = new StringArray();

        // private FastList<object> _innerFunctions;

        public YExpression<JSFunctionDelegate> Method { get; }

        public FastCompiler(
            in StringSpan code,
            string location = null,
            IList<string> argsList = null,
            ICodeCache codeCache = null) {
            this.pool = new FastPool();

            location = location ?? "vm.js";
            this.location = location;

            // FileNameExpression = Exp.Variable(typeof(string), "_fileName");
            // CodeStringExpression = Exp.Variable(typeof(string), "code");


            // this.Code = new ParsedScript(code);


            // _innerFunctions = pool.AllocateList<object>();

            // add top level...

            using (var fx = this.scope.Push(new FastFunctionScope(pool, (AstFunctionExpression)null))) {

                var parserPool = new FastPool();
                var parser = new FastParser(new FastTokenStream(parserPool, code));
                var jScript = parser.ParseProgram();

                parser = null;
                parserPool.Dispose();
                parserPool = null;

                // System.Console.WriteLine($"Parsing done...");

                var lScope = fx.Context;

                if (argsList != null && jScript.HoistingScope != null) {
                    var list = pool.AllocateList<StringSpan>(jScript.HoistingScope.Value.Length);
                    try
                    {
                        var e = jScript.HoistingScope.Value.GetEnumerator();
                        while (e.MoveNext(out var a))
                        {
                            if (argsList.Contains(a.Value))
                                continue;
                            list.Add(a);
                        }
                        jScript.HoistingScope = list.ToSpan();
                    } finally
                    {
                        list.Clear();
                    }
                }

                var scriptInfo = fx.ScriptInfo;


                var args = fx.ArgumentsExpression;

                var te = ArgumentsBuilder.This(args);

                var stackItem = fx.StackItem;

                var vList = new SparseList<ParameterExpression>() {
                    scriptInfo,
                    lScope,
                    stackItem
                };





                if (argsList != null) {
                    int i = 0;
                    foreach (var arg in argsList) {

                        // global arguments are set here for FunctionConstructor

                        fx.CreateVariable(arg,
                            JSVariableBuilder.FromArgument(fx.ArgumentsExpression, i++, arg));
                    }
                }

                var l = fx.ReturnLabel;

                var script = Visit(jScript);

                var sList = new List<Exp>() {
                    Exp.Assign(scriptInfo, ScriptInfoBuilder.New(location,code.Value)),
                    Exp.Assign(lScope, JSContextBuilder.Current)
                };

                //sList.Add(Exp.Assign(ScriptInfoBuilder.Functions(scriptInfo),
                //    Exp.Constant(_innerFunctions.ToArray())));

                JSContextStackBuilder.Push(sList, lScope, stackItem, Exp.Constant(location), StringSpanBuilder.Empty, 0, 0);

                sList.Add(ScriptInfoBuilder.Build(scriptInfo, _keyStrings));

                // ref var keyStrings = ref _keyStrings;
                //foreach (var ks in keyStrings.AllValues())
                //{
                //    var v = ks.Value;
                //    vList.Add(v);
                //    sList.Add(Exp.Assign(v, ExpHelper.KeyStringsBuilder.GetOrCreate(Exp.Constant(ks.Key))));
                //}

                vList.AddRange(fx.VariableParameters);
                sList.AddRange(fx.InitList);
                // register globals..
                foreach (var v in fx.Variables) {
                    if (v.Variable != null && v.Variable.Type == typeof(JSVariable)) {
                        if (argsList?.Contains(v.Name) ?? false)
                            continue;
                        if (v.Name == "this")
                            continue;
                        sList.Add(JSContextBuilder.Register(lScope, v.Variable));
                    }
                }


                sList.Add(Exp.Return(l, script.ToJSValue()));
                sList.Add(Exp.Label(l, JSUndefinedBuilder.Value));

                //script = Exp.Block(vList,
                //    Exp.TryFinally(
                //        Exp.Block(sList),
                //        ExpHelper.IDisposableBuilder.Dispose(lScope))
                //);
                //var catchExp = Exp.Parameter(typeof(Exception));
                //vList.Add(catchExp);

                //var catchWithFilter = Exp.Catch(
                //    catchExp,
                //    Exp.Throw(JSExceptionBuilder.From(catchExp), typeof(JSValue)),
                //    Exp.Not(Exp.TypeIs(catchExp, typeof(JSException))));
                //script = Exp.Block(vList,
                //    Exp.TryCatchFinally(
                //        Exp.Block(sList),
                //        JSContextStackBuilder.Pop(stackItem),
                //        catchWithFilter)

                // sList.Add(JSContextStackBuilder.Pop(stackItem));

                script = Exp.Block(vList, Exp.TryFinally(Exp.Block(sList), JSContextStackBuilder.Pop(stackItem, lScope)));


                var lambda = Exp.Lambda<JSFunctionDelegate>("body", script, fx.Arguments);

                // System.Console.WriteLine($"Code Generation done...");

                this.Method = lambda;
            }
        }

        private Expression VisitExpression(AstExpression exp) => Visit(exp);

        private Expression VisitStatement(AstStatement exp) => Visit(exp);

        protected override Expression VisitClassStatement(AstClassExpression classStatement)
        {
            return CreateClass(classStatement.Identifier, classStatement.Base, classStatement);
        }

        protected override Expression VisitContinueStatement(AstContinueStatement continueStatement)
        {
            string name = continueStatement.Label?.Name.Value;
            if (name != null)
            {
                var target = this.LoopScope.Get(name);
                if (target == null)
                    throw JSContext.Current.NewSyntaxError($"No label found for {name}");
                return Exp.Continue(target.Break);
            }
            return Exp.Continue(this.scope.Top.Loop.Top.Continue);
        }

        protected override Expression VisitDebuggerStatement(AstDebuggerStatement debuggerStatement)
        {
            return ExpHelper.JSDebuggerBuilder.RaiseBreak();
        }



        protected override Expression VisitEmptyExpression(AstEmptyExpression emptyExpression)
        {
            return Exp.Empty;
        }

        protected override Expression VisitExpressionStatement(AstExpressionStatement expressionStatement)
        {
            return Visit(expressionStatement.Expression);
        }

        protected override Expression VisitFunctionExpression(AstFunctionExpression functionExpression)
        {
            return CreateFunction(functionExpression);
        }





        protected override Expression VisitSpreadElement(AstSpreadElement spreadElement)
        {
            throw new NotImplementedException();
        }

        protected override Expression VisitThrowStatement(AstThrowStatement throwStatement)
        {
            return ExpHelper.JSExceptionBuilder.Throw(VisitExpression(throwStatement.Argument));
        }

        protected override Expression VisitYieldExpression(AstYieldExpression yieldExpression)
        {
            var target = VisitExpression(yieldExpression.Argument);
            if (yieldExpression.Delegate)
            {
                throw new NotSupportedException();
                // return JSGeneratorBuilder.Delegate(this.scope.Top.Generator, VisitExpression(yieldExpression.Argument));
            }
            // return JSGeneratorBuilder.Yield(this.scope.Top.Generator, VisitExpression(yieldExpression.Argument));
            // return YantraJS.Core.LinqExpressions.Generators.YieldExpression.New(target);
            return YExpression.Yield(target);

        }
    }
}