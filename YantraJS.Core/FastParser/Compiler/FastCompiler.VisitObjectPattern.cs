﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using YantraJS.Core.LinqExpressions;
using YantraJS.ExpHelper;
using Exp = System.Linq.Expressions.Expression;

namespace YantraJS.Core.FastParser.Compiler
{
    partial class FastCompiler
    {
        protected override Exp VisitArrayPattern(AstArrayPattern arrayPattern)
        {
            throw new NotImplementedException();
        }

        protected override Expression VisitObjectPattern(AstObjectPattern objectPattern)
        {
            throw new NotImplementedException();
        }
    }
}
