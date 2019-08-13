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

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Represents a single expression declaration that applies to a given statement.
    /// </summary>
    [Serializable]
    public class ExpressionDeclaration
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public ExpressionDeclaration()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="name">of expression</param>
        /// <param name="parameterNames">expression paramater names</param>
        /// <param name="expression">the expression body</param>
        /// <param name="alias">indicator whether this is an expression alias or not</param>
        public ExpressionDeclaration(
            string name,
            IList<string> parameterNames,
            Expression expression,
            bool alias)
        {
            this.Name = name;
            this.ParameterNames = parameterNames;
            this.Expression = expression;
            this.IsAlias = alias;
        }

        /// <summary>
        /// Returns expression name.
        /// </summary>
        /// <value>name</value>
        public string Name { get; set; }

        /// <summary>
        /// Returns the expression body.
        /// </summary>
        /// <value>expression body</value>
        public Expression Expression { get; set; }

        /// <summary>
        /// Returns the paramater names.
        /// </summary>
        /// <value>paramater names</value>
        public IList<string> ParameterNames { get; set; }

        /// <summary>
        /// Returns indicator whether the expression is an alias or not.
        /// </summary>
        /// <value>alias indicator</value>
        public bool IsAlias { get; set; }

        /// <summary>
        /// Print.
        /// </summary>
        /// <param name="writer">to print to</param>
        /// <param name="expressionDeclarations">expression declarations</param>
        /// <param name="formatter">for newline-whitespace formatting</param>
        public static void ToEPL(
            TextWriter writer,
            IList<ExpressionDeclaration> expressionDeclarations,
            EPStatementFormatter formatter)
        {
            if ((expressionDeclarations == null) || (expressionDeclarations.IsEmpty()))
            {
                return;
            }

            foreach (var part in expressionDeclarations)
            {
                if (part.Name == null)
                {
                    continue;
                }

                formatter.BeginExpressionDecl(writer);
                part.ToEPL(writer);
            }
        }

        /// <summary>
        /// Print part.
        /// </summary>
        /// <param name="writer">to write to</param>
        public void ToEPL(TextWriter writer)
        {
            writer.Write("expression ");
            writer.Write(Name);
            if (IsAlias)
            {
                writer.Write(" alias for");
            }

            writer.Write(" {");
            if (!IsAlias)
            {
                if (ParameterNames != null && ParameterNames.Count == 1)
                {
                    writer.Write(ParameterNames[0]);
                }
                else if (ParameterNames != null && !ParameterNames.IsEmpty())
                {
                    var delimiter = "";
                    writer.Write("(");
                    foreach (var name in ParameterNames)
                    {
                        writer.Write(delimiter);
                        writer.Write(name);
                        delimiter = ",";
                    }

                    writer.Write(")");
                }

                if (ParameterNames != null && !ParameterNames.IsEmpty())
                {
                    writer.Write(" => ");
                }
            }

            if (Expression != null)
            {
                Expression.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }

            writer.Write("}");
        }
    }
}