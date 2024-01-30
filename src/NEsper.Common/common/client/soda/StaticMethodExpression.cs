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
using System.Text.Json.Serialization;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Static method call consists of a class name and method name.
    /// </summary>
    public class StaticMethodExpression : ExpressionBase
    {
        private string _className;
        private IList<DotExpressionItem> _chain = new List<DotExpressionItem>();

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
            _className = className;

            IList<Expression> parameterList = new List<Expression>();
            for (var i = 0; i < parameters.Length; i++) {
                if (parameters[i] is Expression) {
                    parameterList.Add((Expression)parameters[i]);
                }
                else {
                    parameterList.Add(new ConstantExpression(parameters[i]));
                }
            }

            _chain.Add(new DotExpressionItemCall(method, parameterList));
        }

        
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="className">class name providing the static method</param>
        /// <param name="chain">method chain</param>
        [JsonConstructor]
        public StaticMethodExpression(
            string className,
            IList<DotExpressionItem> chain)
        {
            _className = className;
            _chain = chain;
        }

        /// <summary>
        /// Returns the chain of method invocations, each pair a method name and list of parameter expressions
        /// </summary>
        /// <returns>method chain</returns>
        public IList<DotExpressionItem> Chain {
            get => _chain;
            set => _chain = value;
        }

        /// <summary>
        /// Sets the chain of method invocations, each pair a method name and list of parameter expressions
        /// </summary>
        /// <param name="chain">method chain</param>
        public StaticMethodExpression SetChain(IList<DotExpressionItem> chain)
        {
            _chain = chain;
            return this;
        }

        public override ExpressionPrecedenceEnum Precedence => ExpressionPrecedenceEnum.UNARY;

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(_className);
            DotExpressionItem.Render(_chain, writer, true);
        }

        /// <summary>
        /// Returns the class name.
        /// </summary>
        /// <returns>class name</returns>
        public string ClassName {
            get => _className;
            set => _className = value;
        }

        /// <summary>
        /// Sets the class name.
        /// </summary>
        /// <param name="className">class name</param>
        public StaticMethodExpression SetClassName(string className)
        {
            _className = className;
            return this;
        }
    }
} // end of namespace