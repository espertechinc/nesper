///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.join.analyze;
using com.espertech.esper.common.@internal.filterspec;

namespace com.espertech.esper.common.@internal.settings
{
    public interface SettingsApplicationDotMethod
    {
        FilterExprAnalyzerAffector FilterExprAnalyzerAffector { get; }

        FilterSpecCompilerAdvIndexDesc FilterSpecCompilerAdvIndexDesc { get; }

        ExprForge Forge { get; }

        string LhsName { get; }

        ExprNode[] Lhs { get; }

        string DotMethodName { get; }

        string RhsName { get; }

        ExprNode[] Rhs { get; }
        ExprNode Validate(ExprValidationContext validationContext);
    }
} // end of namespace