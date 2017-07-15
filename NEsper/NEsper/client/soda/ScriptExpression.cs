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

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Script-expression is external scripting language expression such as JavaScript, Groovy or MVEL, for example.
    /// </summary>
    [Serializable]
    public class ScriptExpression  {
        private string name;
        private List<string> parameterNames;
        private string expressionText;
        private string optionalReturnType;
        private string optionalEventTypeName;
        private string optionalDialect;
    
        /// <summary>Ctor.</summary>
        public ScriptExpression() {
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
        public ScriptExpression(string name, List<string> parameterNames, string expressionText, string optionalReturnType, string optionalDialect, string optionalEventTypeName) {
            this.name = name;
            this.parameterNames = parameterNames;
            this.expressionText = expressionText;
            this.optionalReturnType = optionalReturnType;
            this.optionalDialect = optionalDialect;
            this.optionalEventTypeName = optionalEventTypeName;
        }
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="name">script name</param>
        /// <param name="parameterNames">parameter list</param>
        /// <param name="expressionText">script text</param>
        /// <param name="optionalReturnType">return type</param>
        /// <param name="optionalDialect">dialect</param>
        public ScriptExpression(string name, List<string> parameterNames, string expressionText, string optionalReturnType, string optionalDialect) {
            This(name, parameterNames, expressionText, optionalReturnType, optionalDialect, null);
        }
    
        /// <summary>
        /// Print.
        /// </summary>
        /// <param name="writer">to print to</param>
        /// <param name="scripts">scripts</param>
        /// <param name="formatter">for newline-whitespace formatting</param>
        public static void ToEPL(TextWriter writer, List<ScriptExpression> scripts, EPStatementFormatter formatter) {
            if ((scripts == null) || (scripts.IsEmpty())) {
                return;
            }
    
            foreach (ScriptExpression part in scripts) {
                if (part.Name == null) {
                    continue;
                }
                formatter.BeginExpressionDecl(writer);
                part.ToEPL(writer);
            }
        }
    
        /// <summary>
        /// Returns the script name.
        /// </summary>
        /// <returns>script name</returns>
        public string GetName() {
            return name;
        }
    
        /// <summary>
        /// Sets the script name.
        /// </summary>
        /// <param name="name">script name to set</param>
        public void SetName(string name) {
            this.name = name;
        }
    
        /// <summary>
        /// Returns the return type, if any is specified.
        /// </summary>
        /// <returns>return type</returns>
        public string GetOptionalReturnType() {
            return optionalReturnType;
        }
    
        /// <summary>
        /// Sets the return type, if any is specified.
        /// </summary>
        /// <param name="optionalReturnType">return type</param>
        public void SetOptionalReturnType(string optionalReturnType) {
            this.optionalReturnType = optionalReturnType;
        }
    
        /// <summary>
        /// Returns a dialect name, or null if none is defined and the configured default applies
        /// </summary>
        /// <returns>dialect name</returns>
        public string GetOptionalDialect() {
            return optionalDialect;
        }
    
        /// <summary>
        /// Sets a dialect name, or null if none is defined and the configured default applies
        /// </summary>
        /// <param name="optionalDialect">dialect name</param>
        public void SetOptionalDialect(string optionalDialect) {
            this.optionalDialect = optionalDialect;
        }
    
        /// <summary>
        /// Returns the script body.
        /// </summary>
        /// <returns>script body</returns>
        public string GetExpressionText() {
            return expressionText;
        }
    
        /// <summary>
        /// Sets the script body.
        /// </summary>
        /// <param name="expressionText">script body</param>
        public void SetExpressionText(string expressionText) {
            this.expressionText = expressionText;
        }
    
        /// <summary>
        /// Returns the lambda expression parameters.
        /// </summary>
        /// <returns>lambda expression parameters</returns>
        public List<string> GetParameterNames() {
            return parameterNames;
        }
    
        /// <summary>
        /// Sets the lambda expression parameters.
        /// </summary>
        /// <param name="parameterNames">lambda expression parameters</param>
        public void SetParameterNames(List<string> parameterNames) {
            this.parameterNames = parameterNames;
        }
    
        /// <summary>
        /// Returns the optional event type name.
        /// </summary>
        /// <returns>type name</returns>
        public string GetOptionalEventTypeName() {
            return optionalEventTypeName;
        }
    
        /// <summary>
        /// Sets the optional event type name.
        /// </summary>
        /// <param name="optionalEventTypeName">name</param>
        public void SetOptionalEventTypeName(string optionalEventTypeName) {
            this.optionalEventTypeName = optionalEventTypeName;
        }
    
        /// <summary>
        /// Print part.
        /// </summary>
        /// <param name="writer">to write to</param>
        public void ToEPL(TextWriter writer) {
            writer.Write("expression ");
            if (optionalReturnType != null) {
                writer.Write(optionalReturnType);
                writer.Write(" ");
            }
            if (optionalEventTypeName != null) {
                writer.Write("@Type(");
                writer.Write(optionalEventTypeName);
                writer.Write(") ");
            }
            if (optionalDialect != null && optionalDialect.Trim().Length() != 0) {
                writer.Write(optionalDialect);
                writer.Write(":");
            }
            writer.Write(name);
            writer.Write("(");
            if (parameterNames != null && !parameterNames.IsEmpty()) {
                string delimiter = "";
                foreach (string name in parameterNames) {
                    writer.Write(delimiter);
                    writer.Write(name);
                    delimiter = ",";
                }
            }
            writer.Write(")");
            writer.Write(" [");
            writer.Write(expressionText);
            writer.Write("]");
        }
    }
} // end of namespace
