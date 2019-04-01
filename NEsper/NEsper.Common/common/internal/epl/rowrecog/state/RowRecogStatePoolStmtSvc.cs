///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.common.@internal.epl.rowrecog.state
{
    public class RowRecogStatePoolStmtSvc
    {
        public RowRecogStatePoolStmtSvc(
            RowRecogStatePoolRuntimeSvc runtimeSvc, RowRecogStatePoolStmtHandler stmtHandler)
        {
            RuntimeSvc = runtimeSvc;
            StmtHandler = stmtHandler;
        }

        public RowRecogStatePoolRuntimeSvc RuntimeSvc { get; }

        public RowRecogStatePoolStmtHandler StmtHandler { get; }
    }
} // end of namespace