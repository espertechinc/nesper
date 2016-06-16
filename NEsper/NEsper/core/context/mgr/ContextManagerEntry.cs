///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

namespace com.espertech.esper.core.context.mgr
{
	public class ContextManagerEntry
    {
	    private readonly ContextManager _contextManager;
	    private readonly ISet<int> _referringStatements;

	    public ContextManagerEntry(ContextManager contextManager)
        {
	        _contextManager = contextManager;
	        _referringStatements = new HashSet<int>();
	    }

	    public ContextManager ContextManager
	    {
	        get { return _contextManager; }
	    }

	    public void AddStatement(int statementId)
        {
	        _referringStatements.Add(statementId);
	    }

	    public int StatementCount
	    {
	        get { return _referringStatements.Count; }
	    }

	    public void RemoveStatement(int statementId)
        {
	        _referringStatements.Remove(statementId);
	    }
	}
} // end of namespace
