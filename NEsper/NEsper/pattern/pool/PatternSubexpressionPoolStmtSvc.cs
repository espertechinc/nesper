///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.pattern.pool
{
    public class PatternSubexpressionPoolStmtSvc
    {
        public PatternSubexpressionPoolStmtSvc(PatternSubexpressionPoolEngineSvc engineSvc, PatternSubexpressionPoolStmtHandler stmtHandler)
        {
            EngineSvc = engineSvc;
            StmtHandler = stmtHandler;
        }

        public PatternSubexpressionPoolEngineSvc EngineSvc { get; private set; }

        public PatternSubexpressionPoolStmtHandler StmtHandler { get; private set; }
    }
}