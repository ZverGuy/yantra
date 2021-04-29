using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using YantraJS.Core;
using YantraJS.Expressions;

namespace YantraJS.LambdaCompiler.Tests
{
    [TestClass]
    public class ILBinaryTest: BaseTest
    {
        [TestMethod]
        public void AddCompiled()
        {

            var p1 = YExpression.Parameter(typeof(int));
            var p2 = YExpression.Parameter(typeof(int));

            var lambda = YExpression.Lambda("a",
                YExpression.Binary(p1, YOperator.Add, p2), new YParameterExpression[] { p1, p2 });

            var fx = lambda.CompileInAssembly<Func<int, int, int>>();

            Assert.AreEqual(3, fx(1, 2));
        }

        [TestMethod]
        public void Add()
        {

            var p1 = YExpression.Parameter(typeof(int));
            var p2 = YExpression.Parameter(typeof(int));

            var lambda = YExpression.Lambda("a",
                YExpression.Binary(p1, YOperator.Add, p2), new YParameterExpression[] { p1, p2 });

            var fx = lambda.Compile<Func<int,int,int>>();

            Assert.AreEqual(3, fx(1, 2));
        }


        [TestMethod]
        public void Conditional()
        {

            var p1 = YExpression.Parameter(typeof(int));
            var p2 = YExpression.Parameter(typeof(int));

            var lambda = YExpression.Lambda("a",
                YExpression.Conditional(
                    YExpression.Less(p1,p2),
                    p1,
                    p2
                    ), new YParameterExpression[] { p1, p2 });

            var fx = lambda.Compile<Func<int, int, int>>();

            Assert.AreEqual(1, fx(1, 2));
        }

        [TestMethod]
        public void SaveLocal()
        {
            var p1 = YExpression.Parameter(typeof(int));
            var p2 = YExpression.Parameter(typeof(int));

            var temp = YExpression.Parameter(typeof(int));

            var @break = YExpression.Label();
            var @continue = YExpression.Label();

            var zero = YExpression.Constant(0);
            var one = YExpression.Constant(1);

            var loop = YExpression.Block(YExpression.Conditional(
                    YExpression.Binary(p2, YOperator.LessOrEqual, zero),
                    YExpression.GoTo(@break), null
                    ),
                YExpression.Assign(temp, YExpression.Binary(temp, YOperator.Add, p2)),
                YExpression.Assign(p2, YExpression.Binary(p2, YOperator.Subtract, one)));

            var body = YExpression.Block(new YParameterExpression[] { temp },
                YExpression.Assign(temp, zero),
                YExpression.Loop(loop, @break, @continue),
                temp) ;

            var lambda = YExpression.Lambda("mul",
                body, new YParameterExpression[] { p1, p2 }
                );

            var fx = lambda.Compile<Func<int, int, int>>();

            var n = fx(1, 2);
            Assert.AreEqual(3, n);
        }

    }
}
