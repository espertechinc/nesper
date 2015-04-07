///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Clause for creating an expression for use across one or more statements.
    /// <para/>
    /// Both expressions and scripts can be created using this clause.
    /// </summary>
    [Serializable]
    public class CreateExpressionClause
    {
        /// <summary>Ctor. </summary>
        public CreateExpressionClause()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="expressionDeclaration">expression</param>
        public CreateExpressionClause(ExpressionDeclaration expressionDeclaration)
        {
            ExpressionDeclaration = expressionDeclaration;
        }

        /// <summary>Ctor. </summary>
        /// <param name="scriptExpression">script</param>
        public CreateExpressionClause(ScriptExpression scriptExpression)
        {
            ScriptExpression = scriptExpression;
        }

        /// <summary>Returns the expression declaration or null if script instead. </summary>
        /// <value>expression declaration</value>
        public ExpressionDeclaration ExpressionDeclaration { get; set; }

        /// <summary>Returns the script expression or null if declaring an EPL expression. </summary>
        /// <value>script expression</value>
        public ScriptExpression ScriptExpression { get; set; }

        /// <summary>EPL output </summary>
        /// <param name="writer">to write to</param>
        public void ToEPL(TextWriter writer)
        {
            writer.Write("create ");
            if (ExpressionDeclaration != null)
            {
                ExpressionDeclaration.ToEPL(writer);
            }
            else
            {
                ScriptExpression.ToEPL(writer);
            }
        }
    }
}