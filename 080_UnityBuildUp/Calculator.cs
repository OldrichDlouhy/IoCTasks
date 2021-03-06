﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace UnityBuildUp
{
    public interface ICalculator
    {
        int Evaluate(IEnumerable<string> expression);
    }

    public class Calculator : ICalculator
    {
        private readonly IOperationFactory _factory;

        /// <summary>
        /// What we are performing here is basically a constructor dependency injection.
        /// Calculator depends on <see cref="IOperationFactory"/> and instead of creating
        /// the instance itself, the <see cref="Container"/> does so.
        /// </summary>
        /// <param name="factory"></param>
        public Calculator(IOperationFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Implementation of this method has basically no significance from
        /// IoC point of view, except for using <see cref="_factory"/> to
        /// create instances of various numeric operations.
        /// </summary>
        public int Evaluate(IEnumerable<string> expression)
        {
            var operands = new Stack<int>();
            var operations = new Stack<IOperation>();

            // Simple variable controlling the syntactic correctness of
            // the input expression.
            var expectingConstant = true;

            // Process the expression in reverse order to fill in the stacks
            // that the expression will be evaluated from left to right.
            foreach (var token in expression.Reverse())
            {
                if (expectingConstant)
                {
                    int constant;
                    if (int.TryParse(token, out constant)) operands.Push(constant);
                    else throw new Exception("invalid expression");

                    expectingConstant = false;
                    continue;
                }

                operations.Push(_factory.Create(token));
                expectingConstant = true;
            }

            if (expectingConstant) throw new Exception("invalid expression");
            if (operands.Count == 0) throw new Exception("invalid expression");

            while (operands.Count > 1)
            {
                var operand1 = operands.Pop();
                var operand2 = operands.Pop();
                var operation = operations.Pop();

                var result = operation.Evaluate(operand1, operand2);
                operands.Push(result);
            }

            return operands.Peek();
        }
    }
}