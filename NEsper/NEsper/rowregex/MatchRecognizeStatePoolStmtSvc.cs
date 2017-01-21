///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.rowregex
{
	public class MatchRecognizeStatePoolStmtSvc
    {
	    private readonly MatchRecognizeStatePoolEngineSvc _engineSvc;
	    private readonly MatchRecognizeStatePoolStmtHandler _stmtHandler;

	    public MatchRecognizeStatePoolStmtSvc(MatchRecognizeStatePoolEngineSvc engineSvc, MatchRecognizeStatePoolStmtHandler stmtHandler)
        {
	        _engineSvc = engineSvc;
	        _stmtHandler = stmtHandler;
	    }

	    public MatchRecognizeStatePoolEngineSvc EngineSvc
	    {
	        get { return _engineSvc; }
	    }

	    public MatchRecognizeStatePoolStmtHandler StmtHandler
	    {
	        get { return _stmtHandler; }
	    }
    }
} // end of namespace
