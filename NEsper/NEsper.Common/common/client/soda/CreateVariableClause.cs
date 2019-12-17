///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Represents a create-variable syntax for creating a new variable.
    /// </summary>
    [Serializable]
    public class CreateVariableClause
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        public CreateVariableClause()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="variableName">variable name</param>
        public CreateVariableClause(string variableName)
        {
            VariableName = variableName;
        }

        /// <summary>
        /// Creates a create-variable syntax for declaring a variable.
        /// </summary>
        /// <param name="variableType">is the variable type name</param>
        /// <param name="variableName">is the name of the variable</param>
        /// <returns>create-variable clause</returns>
        public static CreateVariableClause Create(
            string variableType,
            string variableName)
        {
            return new CreateVariableClause(variableType, variableName, null, false);
        }

        /// <summary>
        /// Creates a create-variable syntax for declaring a variable.
        /// </summary>
        /// <param name="variableType">is the variable type name</param>
        /// <param name="variableName">is the name of the variable</param>
        /// <param name="expression">is the assignment expression supplying the initial value</param>
        /// <returns>create-variable clause</returns>
        public static CreateVariableClause Create(
            string variableType,
            string variableName,
            Expression expression)
        {
            return new CreateVariableClause(variableType, variableName, expression, false);
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="variableType">is the variable type name</param>
        /// <param name="variableName">is the name of the variable</param>
        /// <param name="optionalAssignment">is the optional assignment expression supplying the initial value, or null if theinitial value is null
        /// </param>
        /// <param name="constant">true for constant, false for regular variable</param>
        public CreateVariableClause(
            string variableType,
            string variableName,
            Expression optionalAssignment,
            bool constant)
        {
            VariableType = variableType;
            VariableName = variableName;
            OptionalAssignment = optionalAssignment;
            IsConstant = constant;
        }

        /// <summary>
        /// Returns the variable type name.
        /// </summary>
        /// <value>type of the variable</value>
        public string VariableType { get; set; }

        /// <summary>
        /// Returns the variable name.
        /// </summary>
        /// <value>name of the variable</value>
        public string VariableName { get; set; }

        /// <summary>
        /// Returns the optional assignment expression, or null to initialize to a null value
        /// </summary>
        /// <value>assignment expression, if present</value>
        public Expression OptionalAssignment { get; set; }

        /// <summary>
        /// Returns indicator whether the variable is a constant.
        /// </summary>
        /// <value>constant false</value>
        public bool IsConstant { get; set; }

        /// <summary>
        /// Returns indictor whether array or not array.
        /// </summary>
        /// <value>array indicator</value>
        public bool IsArray { get; set; }

        /// <summary>
        /// Returns true for array of primitive values (also set the array flag)
        /// </summary>
        /// <value>indicator</value>
        public bool IsArrayOfPrimitive { get; set; }

        /// <summary>
        /// RenderAny as EPL.
        /// </summary>
        /// <param name="writer">to output to</param>
        public virtual void ToEPL(TextWriter writer)
        {
            writer.Write("create");
            if (IsConstant)
            {
                writer.Write(" constant");
            }

            writer.Write(" variable ");
            if (VariableType != null)
            {
                writer.Write(VariableType);
                if (IsArray)
                {
                    if (IsArrayOfPrimitive)
                    {
                        writer.Write("[primitive]");
                    }
                    else
                    {
                        writer.Write("[]");
                    }
                }

                writer.Write(" ");
            }

            writer.Write(VariableName);
            if (OptionalAssignment != null)
            {
                writer.Write(" = ");
                OptionalAssignment.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }
        }
    }
}