///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.@join.plan;
using com.espertech.esper.filter;

namespace com.espertech.esper.epl.core
{
    public interface EngineImportApplicationDotMethod
    {
        ExprEvaluator ExprEvaluator { get; }

        string LhsName { get; }
        IList<ExprNode> Lhs { get; }
        string DotMethodName { get; }
        string RhsName { get; }
        IList<ExprNode> Rhs { get; }

        ExprNode Validate(ExprValidationContext validationContext);

        FilterExprAnalyzerAffector GetFilterExprAnalyzerAffector();
        FilterSpecCompilerAdvIndexDesc GetFilterSpecCompilerAdvIndexDesc();
    }
} // end of namespace