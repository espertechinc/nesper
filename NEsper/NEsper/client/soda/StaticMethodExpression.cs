///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;


namespace com.espertech.esper.client.soda
{
    /// <summary>Static method call consists of a class name and method name. </summary>
    [Serializable]
    public class StaticMethodExpression : ExpressionBase
    {
        /// <summary>Ctor. </summary>
        /// <param name="className">class name providing the static method</param>
        /// <param name="method">method name</param>
        /// <param name="parameters">an optiona array of parameters</param>
        public StaticMethodExpression(String className, String method, Object[] parameters)
        {
            Chain = new List<DotExpressionItem>();
            ClassName = className;
    
            var parameterList = new List<Expression>();
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] is Expression)
                {
                    parameterList.Add((Expression)parameters[i]);
                }
                else
                {
                    parameterList.Add(new ConstantExpression(parameters[i]));
                }
            }
            Chain.Add(new DotExpressionItem(method, parameterList, false));
        }

        /// <summary>Returns the chain of method invocations, each pair a method name and list of parameter expressions </summary>
        /// <value>method chain</value>
        public List<DotExpressionItem> Chain { get; set; }

        /// <summary>Ctor. </summary>
        /// <param name="className">class name providing the static method</param>
        /// <param name="chain">method chain</param>
        public StaticMethodExpression(String className, List<DotExpressionItem> chain)
        {
            ClassName = className;
            Chain = chain;
        }

        public override ExpressionPrecedenceEnum Precedence
        {
            get { return ExpressionPrecedenceEnum.UNARY; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(ClassName);
            DotExpressionItem.Render(Chain, writer, true);
        }

        /// <summary>Returns the class name. </summary>
        /// <value>class name</value>
        public string ClassName { get; set; }
    }
}
