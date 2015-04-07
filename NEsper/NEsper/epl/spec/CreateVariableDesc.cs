///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.spec
{
    /// <summary>Descriptor for create-variable statements. </summary>
    [Serializable]
    public class CreateVariableDesc
        : MetaDefItem
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="variableType">type of the variable</param>
        /// <param name="variableName">name of the variable</param>
        /// <param name="assignment">expression assigning the initial value, or null if none</param>
        /// <param name="constant">if set to <c>true</c> [constant].</param>
        /// <param name="isArray">if set to <c>true</c> [is array].</param>
        /// <param name="isArrayOfPrimitive"></param>
        public CreateVariableDesc(string variableType, string variableName, ExprNode assignment, bool constant, bool isArray, bool isArrayOfPrimitive)
        {
            VariableType = variableType;
            VariableName = variableName;
            Assignment = assignment;
            IsConstant = constant;
            IsArray = isArray;
            IsArrayOfPrimitive = isArrayOfPrimitive;
        }

        /// <summary>Returns the variable type. </summary>
        /// <value>type of variable</value>
        public string VariableType { get; private set; }

        /// <summary>Returns the variable name </summary>
        /// <value>name</value>
        public string VariableName { get; private set; }

        /// <summary>Returns the assignment expression, or null if none </summary>
        /// <value>expression or null</value>
        public ExprNode Assignment { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="CreateVariableDesc"/> is constant.
        /// </summary>
        /// <value><c>true</c> if constant; otherwise, <c>false</c>.</value>
        public bool IsConstant { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is array.
        /// </summary>
        /// <value><c>true</c> if this instance is array; otherwise, <c>false</c>.</value>
        public bool IsArray { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance is array of primitive.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is array of primitive; otherwise, <c>false</c>.
        /// </value>
        public bool IsArrayOfPrimitive { get; private set; }
    }
}