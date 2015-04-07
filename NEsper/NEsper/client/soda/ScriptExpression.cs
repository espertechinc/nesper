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
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Script-expression is external scripting language expression such as JavaScript, Groovy or MVEL, for example.
    /// </summary>
    [Serializable]
    public class ScriptExpression
    {
        /// <summary>Ctor. </summary>
        public ScriptExpression()
        {
        }

        /// <summary>Ctor. </summary>
        /// <param name="name">script name</param>
        /// <param name="parameterNames">parameter list</param>
        /// <param name="expressionText">script text</param>
        /// <param name="optionalReturnType">return type</param>
        /// <param name="optionalDialect">dialect</param>
        public ScriptExpression(String name, IList<string> parameterNames, String expressionText, String optionalReturnType, String optionalDialect)
        {
            Name = name;
            ParameterNames = parameterNames;
            ExpressionText = expressionText;
            OptionalReturnType = optionalReturnType;
            OptionalDialect = optionalDialect;
        }

        /// <summary>Returns the script name. </summary>
        /// <value>script name</value>
        public string Name { get; set; }

        /// <summary>Returns the return type, if any is specified. </summary>
        /// <value>return type</value>
        public string OptionalReturnType { get; set; }

        /// <summary>Returns a dialect name, or null if none is defined and the configured default applies </summary>
        /// <value>dialect name</value>
        public string OptionalDialect { get; set; }

        /// <summary>Returns the script body. </summary>
        /// <value>script body</value>
        public string ExpressionText { get; set; }

        /// <summary>Returns the lambda expression parameters. </summary>
        /// <value>lambda expression parameters</value>
        public IList<string> ParameterNames { get; set; }

        /// <summary>Print. </summary>
        /// <param name="writer">to print to</param>
        /// <param name="scripts">scripts</param>
        /// <param name="formatter">for NewLine-whitespace formatting</param>
        public static void ToEPL(TextWriter writer, IList<ScriptExpression> scripts, EPStatementFormatter formatter)
        {
            if ((scripts == null) || (scripts.IsEmpty()))
            {
                return;
            }

            foreach (ScriptExpression part in scripts)
            {
                if (part.Name == null)
                {
                    continue;
                }
                formatter.BeginExpressionDecl(writer);
                part.ToEPL(writer);
            }
        }

        /// <summary>Print part. </summary>
        /// <param name="writer">to write to</param>
        public void ToEPL(TextWriter writer)
        {
            writer.Write("expression ");
            if (OptionalReturnType != null)
            {
                writer.Write(OptionalReturnType);
                writer.Write(" ");
            }
            if (!string.IsNullOrWhiteSpace(OptionalDialect))
            {
                writer.Write(OptionalDialect);
                writer.Write(":");
            }
            writer.Write(Name);
            writer.Write("(");

            if (ParameterNames != null && !ParameterNames.IsEmpty())
            {
                String delimiter = "";
                foreach (String name in ParameterNames)
                {
                    writer.Write(delimiter);
                    writer.Write(name);
                    delimiter = ",";
                }
            }

            writer.Write(")");

            writer.Write(" [");
            writer.Write(ExpressionText);
            writer.Write("]");
        }
    }
}
