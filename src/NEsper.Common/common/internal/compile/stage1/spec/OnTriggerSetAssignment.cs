///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.epl.expression.assign;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    ///     Descriptor for an on-set assignment.
    /// </summary>
    public class OnTriggerSetAssignment
    {
        /// <summary>Ctor. </summary>
        /// <param name="expression">expression providing new variable value</param>
        public OnTriggerSetAssignment(ExprNode expression)
        {
            Expression = expression;
        }

        /// <summary>Returns the expression providing the new variable value, or null if none </summary>
        /// <value>assignment expression</value>
        public ExprNode Expression { get; set; }

        public ExprAssignment Validated { get; set; }
    }
}