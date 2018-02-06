///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.epl.datetime.eval;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.variable;

namespace com.espertech.esper.epl.expression.dot
{
    /// <summary>
    /// Represents an Dot-operator expression, for use when "(expression).Method(...).Method(...)"
    /// </summary>
    public interface ExprDotNode : ExprNode, FilterExprAnalyzerAffectorProvider
    {
        int? StreamReferencedIfAny { get; }
        IList<ExprChainedSpec> ChainSpec { get; }
        string IsVariableOpGetName(VariableService variableService);
    }
} // end of namespace
