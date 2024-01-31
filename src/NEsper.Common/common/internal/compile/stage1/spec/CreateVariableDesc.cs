///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.type;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>Descriptor for create-variable statements. </summary>
    public class CreateVariableDesc
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="variableType">type of the variable</param>
        /// <param name="variableName">name of the variable</param>
        /// <param name="assignment">expression assigning the initial value, or null if none</param>
        /// <param name="constant">if set to <c>true</c> [constant].</param>
        public CreateVariableDesc(
            ClassDescriptor variableType,
            string variableName,
            ExprNode assignment,
            bool constant)
        {
            VariableType = variableType;
            VariableName = variableName;
            Assignment = assignment;
            IsConstant = constant;
        }

        /// <summary>Returns the variable type. </summary>
        /// <value>type of variable</value>
        public ClassDescriptor VariableType { get; }

        /// <summary>Returns the variable name </summary>
        /// <value>name</value>
        public string VariableName { get; }

        /// <summary>Returns the assignment expression, or null if none </summary>
        /// <value>expression or null</value>
        public ExprNode Assignment { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether this <see cref="CreateVariableDesc" /> is constant.
        /// </summary>
        /// <value><c>true</c> if constant; otherwise, <c>false</c>.</value>
        public bool IsConstant { get; }
    }
}