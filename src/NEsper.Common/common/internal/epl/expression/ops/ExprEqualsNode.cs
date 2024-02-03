///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    ///     Represents an equals (=, !=, &lt;&gt;, is, is not) comparator in a filter expressiun tree.
    /// </summary>
    public interface ExprEqualsNode : ExprNode
    {
        /// <summary>
        ///     Returns true if this is a NOT EQUALS node, false if this is a EQUALS node.
        /// </summary>
        /// <value>true for !=, false for =</value>
        bool IsNotEquals { get; }

        /// <summary>
        ///     Returns true if this is a "IS" or "IS NOT" node, false if this is a EQUALS or NOT EQUALS node.
        /// </summary>
        /// <value>true for !=, false for =</value>
        bool IsIs { get; }
    }
} // end of namespace