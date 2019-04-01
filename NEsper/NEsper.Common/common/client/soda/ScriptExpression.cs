///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
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
    /// Script-expression is external scripting language expression such as JavaScript, Groovy or MVEL, for example.
    /// </summary>
    [Serializable]
    public class ScriptExpression
    {
        private string _name;
        private IList<string> _parameterNames;
        private string _expressionText;
        private string _optionalReturnType;
        private string _optionalEventTypeName;
        private string _optionalDialect;

        /// <summary>Ctor.</summary>
        public ScriptExpression()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="name">script name</param>
        /// <param name="parameterNames">parameter list</param>
        /// <param name="expressionText">script text</param>
        /// <param name="optionalReturnType">return type</param>
        /// <param name="optionalDialect">dialect</param>
        /// <param name="optionalEventTypeName">optional event type name</param>
        public ScriptExpression(
            string name,
            IList<string> parameterNames,
            string expressionText,
            string optionalReturnType,
            string optionalDialect,
            string optionalEventTypeName)
        {
            _name = name;
            _parameterNames = parameterNames;
            _expressionText = expressionText;
            _optionalReturnType = optionalReturnType;
            _optionalDialect = optionalDialect;
            _optionalEventTypeName = optionalEventTypeName;
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="name">script name</param>
        /// <param name="parameterNames">parameter list</param>
        /// <param name="expressionText">script text</param>
        /// <param name="optionalReturnType">return type</param>
        /// <param name="optionalDialect">dialect</param>
        public ScriptExpression(
            string name,
            IList<string> parameterNames,
            string expressionText,
            string optionalReturnType,
            string optionalDialect)
            : this(name, parameterNames, expressionText, optionalReturnType, optionalDialect, null)
        {
        }

        /// <summary>
        /// Print.
        /// </summary>
        /// <param name="writer">to print to</param>
        /// <param name="scripts">scripts</param>
        /// <param name="formatter">for newline-whitespace formatting</param>
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

        /// <summary>
        /// Returns the script name.
        /// </summary>
        /// <value>script name</value>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// Returns the return type, if any is specified.
        /// </summary>
        /// <value>return type</value>
        public string OptionalReturnType
        {
            get { return _optionalReturnType; }
            set { _optionalReturnType = value; }
        }

        /// <summary>
        /// Returns a dialect name, or null if none is defined and the configured default applies
        /// </summary>
        /// <value>dialect name</value>
        public string OptionalDialect
        {
            get { return _optionalDialect; }
            set { _optionalDialect = value; }
        }

        /// <summary>
        /// Returns the script body.
        /// </summary>
        /// <value>script body</value>
        public string ExpressionText
        {
            get { return _expressionText; }
            set { _expressionText = value; }
        }

        /// <summary>
        /// Returns the lambda expression parameters.
        /// </summary>
        /// <value>lambda expression parameters</value>
        public IList<string> ParameterNames
        {
            get { return _parameterNames; }
            set { _parameterNames = value; }
        }

        /// <summary>
        /// Returns the optional event type name.
        /// </summary>
        /// <value>type name</value>
        public string OptionalEventTypeName
        {
            get { return _optionalEventTypeName; }
            set { _optionalEventTypeName = value; }
        }

        /// <summary>
        /// Print part.
        /// </summary>
        /// <param name="writer">to write to</param>
        public void ToEPL(TextWriter writer)
        {
            writer.Write("expression ");
            if (_optionalReturnType != null)
            {
                writer.Write(_optionalReturnType);
                writer.Write(" ");
            }
            if (_optionalEventTypeName != null)
            {
                writer.Write("@type(");
                writer.Write(_optionalEventTypeName);
                writer.Write(") ");
            }
            if (!string.IsNullOrWhiteSpace(_optionalDialect))
            {
                writer.Write(_optionalDialect);
                writer.Write(":");
            }
            writer.Write(_name);
            writer.Write("(");
            if (_parameterNames != null && !_parameterNames.IsEmpty())
            {
                string delimiter = "";
                foreach (string name in _parameterNames)
                {
                    writer.Write(delimiter);
                    writer.Write(name);
                    delimiter = ",";
                }
            }
            writer.Write(")");
            writer.Write(" [");
            writer.Write(_expressionText);
            writer.Write("]");
        }
    }
} // end of namespace
