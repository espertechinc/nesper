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
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Static method call consists of a class name and method name.
    /// </summary>
    [Serializable]
    public class StaticMethodExpression : ExpressionBase
    {
        private string className;
        private IList<DotExpressionItem> chain = new List<DotExpressionItem>();

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="className">class name providing the static method</param>
        /// <param name="method">method name</param>
        /// <param name="parameters">an optional array of parameters</param>
        public StaticMethodExpression(
            string className,
            string method,
            object[] parameters)
        {
            this.className = className;

            IList<Expression> parameterList = new List<Expression>();
            for (int i = 0; i < parameters.Length; i++) {
                if (parameters[i] is Expression) {
                    parameterList.Add((Expression) parameters[i]);
                }
                else {
                    parameterList.Add(new ConstantExpression(parameters[i]));
                }
            }

            chain.Add(new DotExpressionItem(method, parameterList, false));
        }

        /// <summary>
        /// Returns the chain of method invocations, each pair a method name and list of parameter expressions
        /// </summary>
        /// <returns>method chain</returns>
        public IList<DotExpressionItem> Chain {
            get => chain;
        }

        /// <summary>
        /// Sets the chain of method invocations, each pair a method name and list of parameter expressions
        /// </summary>
        /// <param name="chain">method chain</param>
        public void SetChain(IList<DotExpressionItem> chain)
        {
            this.chain = chain;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="className">class name providing the static method</param>
        /// <param name="chain">method chain</param>
        public StaticMethodExpression(
            string className,
            IList<DotExpressionItem> chain)
        {
            this.className = className;
            this.chain = chain;
        }

        public override ExpressionPrecedenceEnum Precedence {
            get => ExpressionPrecedenceEnum.UNARY;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(className);
            DotExpressionItem.Render(chain, writer, true);
        }

        /// <summary>
        /// Returns the class name.
        /// </summary>
        /// <returns>class name</returns>
        public string ClassName {
            get => className;
        }

        /// <summary>
        /// Sets the class name.
        /// </summary>
        /// <param name="className">class name</param>
        public void SetClassName(string className)
        {
            this.className = className;
        }
    }
} // end of namespace