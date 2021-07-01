///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using Antlr4.Runtime.Tree;

using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.epl.expression.core;

namespace com.espertech.esper.compiler.@internal.parse
{
    public class StatementStackItem
    {
        public StatementStackItem(
            StatementSpecRaw statementSpec,
            IDictionary<ITree, ExprNode> astExprNodeMap,
            IList<ViewSpec> viewSpecs)
        {
            StatementSpec = statementSpec;
            AstExprNodeMap = astExprNodeMap;
            ViewSpecs = viewSpecs;
        }

        public StatementSpecRaw StatementSpec { get; }

        public IDictionary<ITree, ExprNode> AstExprNodeMap { get; }

        public IList<ViewSpec> ViewSpecs { get; }
    }
} // end of namespace