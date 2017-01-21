///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.client.annotation;
using com.espertech.esper.client.soda;
using com.espertech.esper.core.context.util;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.lookup;
using com.espertech.esper.epl.table.mgmt;
using com.espertech.esper.events;

namespace com.espertech.esper.epl.named
{
	/// <summary>
	/// View for the on-select statement that handles selecting events from a named window.
	/// </summary>
	public class NamedWindowOnSelectViewFactory : NamedWindowOnExprBaseViewFactory
	{
	    private readonly bool _deleteAndSelect;
	    private readonly string _optionalInsertIntoTableName;

	    public NamedWindowOnSelectViewFactory(EventType namedWindowEventType, InternalEventRouter internalEventRouter, bool addToFront, EPStatementHandle statementHandle, EventBeanReader eventBeanReader, bool distinct, StatementResultService statementResultService, InternalEventRouteDest internalEventRouteDest, bool deleteAndSelect, StreamSelector? optionalStreamSelector, string optionalInsertIntoTableName)
	        : base(namedWindowEventType)
        {
	        InternalEventRouter = internalEventRouter;
	        IsAddToFront = addToFront;
	        StatementHandle = statementHandle;
	        EventBeanReader = eventBeanReader;
	        IsDistinct = distinct;
	        StatementResultService = statementResultService;
	        InternalEventRouteDest = internalEventRouteDest;
	        _deleteAndSelect = deleteAndSelect;
	        OptionalStreamSelector = optionalStreamSelector;
	        _optionalInsertIntoTableName = optionalInsertIntoTableName;
	    }

	    public override NamedWindowOnExprBaseView Make(SubordWMatchExprLookupStrategy lookupStrategy, NamedWindowRootViewInstance namedWindowRootViewInstance, AgentInstanceContext agentInstanceContext, ResultSetProcessor resultSetProcessor)
        {
	        bool audit = AuditEnum.INSERT.GetAudit(agentInstanceContext.StatementContext.Annotations) != null;
	        TableStateInstance tableStateInstance = null;
	        if (_optionalInsertIntoTableName != null) {
	            tableStateInstance = agentInstanceContext.StatementContext.TableService.GetState(_optionalInsertIntoTableName, agentInstanceContext.AgentInstanceId);
	        }
	        return new NamedWindowOnSelectView(lookupStrategy, namedWindowRootViewInstance, agentInstanceContext, this, resultSetProcessor, audit, _deleteAndSelect, tableStateInstance);
	    }

	    public InternalEventRouter InternalEventRouter { get; private set; }

	    public bool IsAddToFront { get; private set; }

	    public EPStatementHandle StatementHandle { get; private set; }

	    public EventBeanReader EventBeanReader { get; private set; }

	    public bool IsDistinct { get; private set; }

	    public StatementResultService StatementResultService { get; private set; }

	    public InternalEventRouteDest InternalEventRouteDest { get; private set; }

	    public StreamSelector? OptionalStreamSelector { get; private set; }
	}
} // end of namespace
