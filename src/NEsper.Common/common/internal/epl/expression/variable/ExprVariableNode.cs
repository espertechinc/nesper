///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.variable.compiletime;

namespace com.espertech.esper.common.@internal.epl.expression.variable
{
    /// <summary>
    /// Represents a variable in an expression tree.
    /// </summary>
    public interface ExprVariableNode : ExprNodeDeployTimeConst
    {
        string VariableNameWithSubProp { get; }

        VariableMetaData VariableMetadata { get; }
    }
} // end of namespace