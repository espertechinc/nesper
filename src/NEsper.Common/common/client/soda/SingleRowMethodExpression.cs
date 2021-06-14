///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Generic single-row method call consists of a method name and parameters, possibly chained.
    /// </summary>
    [Serializable]
    public class SingleRowMethodExpression : ExpressionBase
    {
        private IList<DotExpressionItem> chain = new List<DotExpressionItem>();

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="method">method name</param>
        /// <param name="parameters">an optiona array of parameters</param>
        public SingleRowMethodExpression(
            string method,
            object[] parameters)
        {
            IList<Expression> parameterList = new List<Expression>();
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] is Expression)
                {
                    parameterList.Add((Expression) parameters[i]);
                }
                else
                {
                    parameterList.Add(new ConstantExpression(parameters[i]));
                }
            }

            chain.Add(new DotExpressionItemCall(method, parameterList));
        }

        /// <summary>
        /// Returns the optional method invocation chain for the single-row method consisting of
        /// pairs of method name and list of parameters.
        /// </summary>
        /// <returns>chain of method invocations</returns>
        public IList<DotExpressionItem> Chain
        {
            get => chain;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="chain">of method invocations with at least one element, each pair a method name and list of parameter expressions</param>
        public SingleRowMethodExpression(IList<DotExpressionItem> chain)
        {
            this.chain = chain;
        }

        public override ExpressionPrecedenceEnum Precedence
        {
            get => ExpressionPrecedenceEnum.UNARY;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            DotExpressionItem.Render(chain, writer, false);
        }
    }
} // end of namespace