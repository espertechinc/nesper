///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.epl.pattern.pool
{
    public class PatternSubexpressionPoolStmtSvc
    {
        private readonly PatternSubexpressionPoolRuntimeSvc runtimeSvc;
        private readonly PatternSubexpressionPoolStmtHandler stmtHandler;

        public PatternSubexpressionPoolStmtSvc(
            PatternSubexpressionPoolRuntimeSvc runtimeSvc,
            PatternSubexpressionPoolStmtHandler stmtHandler)
        {
            this.runtimeSvc = runtimeSvc;
            this.stmtHandler = stmtHandler;
        }

        public PatternSubexpressionPoolRuntimeSvc RuntimeSvc {
            get => runtimeSvc;
        }

        public PatternSubexpressionPoolStmtHandler StmtHandler {
            get => stmtHandler;
        }
    }
} // end of namespace