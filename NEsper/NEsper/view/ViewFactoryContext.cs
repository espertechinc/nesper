///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.core.service;
using com.espertech.esper.events;
using com.espertech.esper.schedule;

namespace com.espertech.esper.view
{
	/// <summary>
	/// Context calss for specific views within a statement. Each view in a statement gets it's own context
	/// containing the statement context.
	/// </summary>
	public class ViewFactoryContext
	{
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="statementContext">is the statement-level services</param>
        /// <param name="streamNum">is the stream number from zero to N</param>
        /// <param name="namespaceName">is the view namespace</param>
        /// <param name="viewName">is the view name</param>
        /// <param name="isSubquery">if set to <c>true</c> [is subquery].</param>
        /// <param name="subqueryNumber">The subquery number.</param>
        /// <param name="isGrouped">if set to <c>true</c> [is grouped].</param>
	    public ViewFactoryContext(StatementContext statementContext, int streamNum, string namespaceName, string viewName, bool isSubquery, int subqueryNumber, bool isGrouped)
        {
	        StatementContext = statementContext;
	        StreamNum = streamNum;
	        NamespaceName = namespaceName;
	        ViewName = viewName;
	        IsSubquery = isSubquery;
	        SubqueryNumber = subqueryNumber;
	        IsGrouped = isGrouped;
	    }

	    /// <summary>
	    /// Returns service to use for schedule evaluation.
	    /// </summary>
	    /// <value>schedule evaluation service implemetation</value>
	    public SchedulingService SchedulingService
	    {
	        get { return StatementContext.SchedulingService; }
	    }

	    /// <summary>
	    /// Returns service for generating events and handling event types.
	    /// </summary>
	    /// <value>event adapter service</value>
	    public EventAdapterService EventAdapterService
	    {
	        get { return StatementContext.EventAdapterService; }
	    }

	    /// <summary>
	    /// Returns the schedule bucket for ordering schedule callbacks within this pattern.
	    /// </summary>
	    /// <value>schedule bucket</value>
	    public ScheduleBucket ScheduleBucket
	    {
	        get { return StatementContext.ScheduleBucket; }
	    }

	    /// <summary>
	    /// Returns the statement's resource locks.
	    /// </summary>
	    /// <value>statement resource lock/handle</value>
	    public EPStatementHandle EpStatementHandle
	    {
	        get { return StatementContext.EpStatementHandle; }
	    }

	    /// <summary>
	    /// Returns extension svc.
	    /// </summary>
	    /// <value>svc</value>
	    public StatementExtensionSvcContext StatementExtensionServicesContext
	    {
	        get { return StatementContext.StatementExtensionServicesContext; }
	    }

	    /// <summary>
	    /// Returns the statement id.
	    /// </summary>
	    /// <value>statement id</value>
	    public int StatementId
	    {
	        get { return StatementContext.StatementId; }
	    }

	    /// <summary>
	    /// Returns the stream number.
	    /// </summary>
	    /// <value>stream number</value>
	    public int StreamNum { get; private set; }

	    /// <summary>
	    /// Returns the view namespace name.
	    /// </summary>
	    /// <value>namespace name</value>
	    public string NamespaceName { get; private set; }

	    /// <summary>
	    /// Returns the view name.
	    /// </summary>
	    /// <value>view name</value>
	    public string ViewName { get; private set; }

	    /// <summary>
	    /// Returns the statement context.
	    /// </summary>
	    /// <value>statement context</value>
	    public StatementContext StatementContext { get; private set; }

	    public bool IsSubquery { get; private set; }

	    public int SubqueryNumber { get; private set; }

	    public bool IsGrouped { get; private set; }

	    public override string ToString()
	    {
	        return  StatementContext +
	                " streamNum=" + StreamNum +
	                " namespaceName=" + NamespaceName +
	                " viewName=" + ViewName;
	    }
	}
} // end of namespace
