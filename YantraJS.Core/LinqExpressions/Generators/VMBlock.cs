﻿#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace YantraJS.Core.LinqExpressions.Generators
{
    public class VMBlock
    {

        private List<Block> blocks = new List<Block>();

        private Block current = new Block();

        public void Add(Expression exp)
        {
            current.Add(exp);
        }

        public void AddYield(Expression exp)
        {
            if (current.Steps.Any())
            {
                blocks.Add(current);
            }
            current = new Block();
            current.Add(exp);
            blocks.Add(current);
            current = new Block();
        }

        public Expression ToExpression(Expression generator)
        {
            if (current.Steps.Any())
            {
                blocks.Add(current);
            }
            return ClrGeneratorBuilder.Block(
                generator, 
                blocks.Select(x => x.ToExpression())
                .Where(x => x != null)
                .ToList());
        }
    }
}