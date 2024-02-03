///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.visitor;

namespace com.espertech.esper.common.@internal.epl.expression.declared.compiletime
{
    /// <summary>
    ///     Expression instance as declared elsewhere.
    ///     <para>
    ///         (1) Statement parse: Expression tree from expression body gets deep-copied.
    ///         (2) Statement create (lifecyle event): Subselect visitor compiles Subselect-list
    ///         (3) Statement start:
    ///         a) event types of each stream determined
    ///         b) subselects filter expressions get validated and subselect started
    ///         (4) Remaining expressions get validated
    ///     </para>
    /// </summary>
    public interface ExprDeclaredNode : ExprNode
    {
        IList<ExprNode> ChainParameters { get; }

        ExpressionDeclItem Prototype { get; }

        ExprNode Body { get; }

        IDictionary<string, int> GetOuterStreamNames(IDictionary<string, int> outerStreamNames);

        void AcceptNoVisitParams(ExprNodeVisitor visitor);

        void AcceptNoVisitParams(ExprNodeVisitorWithParent visitor);
    }
} // end of namespace