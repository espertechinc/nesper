///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.chain;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.analyze;
using com.espertech.esper.common.@internal.epl.variable.compiletime;

namespace com.espertech.esper.common.@internal.epl.expression.dot.core
{
    /// <summary>
    ///     Represents an Dot-operator expression, for use when "(expression).method(...).method(...)"
    /// </summary>
    public interface ExprDotNode : ExprNode,
        FilterExprAnalyzerAffectorProvider,
        ExprNodeWithChainSpec
    {
        int? StreamReferencedIfAny { get; }

        IList<Chainable> ChainSpec { get; set; }

        VariableMetaData IsVariableOpGetName(VariableCompileTimeResolver variableCompileTimeResolver);

        bool IsLocalInlinedClass { get; }
    }

    public class ExprDotNodeConstants
    {
        public const string FILTERINDEX_NAMED_PARAMETER = "filterindex";
    }
} // end of namespace