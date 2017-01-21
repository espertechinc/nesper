///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.compat.collections;
using com.espertech.esper.epl.declexpr;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.subquery;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.filter;

namespace com.espertech.esper.epl.spec
{
	/// <summary>
	/// Specification object representing a complete EPL statement including all EPL constructs.
	/// </summary>
	public class StatementSpecCompiled
	{
	    public static readonly StatementSpecCompiled DEFAULT_SELECT_ALL_EMPTY;

	    static StatementSpecCompiled()
        {
	        DEFAULT_SELECT_ALL_EMPTY = new StatementSpecCompiled();
	        DEFAULT_SELECT_ALL_EMPTY.SelectClauseSpec.SetSelectExprList(new SelectClauseElementWildcard());
        }

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="onTriggerDesc">describes on-delete statements</param>
	    /// <param name="createWindowDesc">describes create-window statements</param>
	    /// <param name="createIndexDesc">when an index get</param>
	    /// <param name="createVariableDesc">describes create-variable statements</param>
	    /// <param name="createTableDesc">The create table desc.</param>
	    /// <param name="createSchemaDesc">The create schema desc.</param>
	    /// <param name="insertIntoDesc">insert into def</param>
	    /// <param name="selectClauseStreamSelectorEnum">stream selection</param>
	    /// <param name="selectClauseSpec">select clause</param>
	    /// <param name="streamSpecs">specs for streams</param>
	    /// <param name="outerJoinDescList">outer join def</param>
	    /// <param name="filterExprRootNode">where filter expr nodes</param>
	    /// <param name="havingExprRootNode">having expression</param>
	    /// <param name="outputLimitSpec">output limit</param>
	    /// <param name="orderByList">order by</param>
	    /// <param name="subSelectExpressions">list of subqueries</param>
	    /// <param name="declaredExpressions">The declared expressions.</param>
	    /// <param name="scripts"></param>
	    /// <param name="variableReferences">variables referenced</param>
	    /// <param name="rowLimitSpec">row limit specification, or null if none supplied</param>
	    /// <param name="eventTypeReferences">event type names statically determined</param>
	    /// <param name="annotations">statement annotations</param>
	    /// <param name="updateSpec">update specification if used</param>
	    /// <param name="matchRecognizeSpec">if provided</param>
	    /// <param name="forClauseSpec">For clause spec.</param>
	    /// <param name="sqlParameters">The SQL parameters.</param>
	    /// <param name="contextDesc">The context desc.</param>
	    /// <param name="optionalContextName">Name of the optional context.</param>
	    /// <param name="createGraphDesc">The create graph desc.</param>
	    /// <param name="createExpressionDesc">The create expression desc.</param>
	    /// <param name="fireAndForgetSpec">The fire and forget spec.</param>
	    /// <param name="groupByExpressions">The group by expressions.</param>
	    /// <param name="intoTableSpec">The into table spec.</param>
	    /// <param name="tableNodes">The table nodes.</param>
	    public StatementSpecCompiled(
	        OnTriggerDesc onTriggerDesc,
	        CreateWindowDesc createWindowDesc,
	        CreateIndexDesc createIndexDesc,
	        CreateVariableDesc createVariableDesc,
	        CreateTableDesc createTableDesc,
	        CreateSchemaDesc createSchemaDesc,
	        InsertIntoDesc insertIntoDesc,
	        SelectClauseStreamSelectorEnum selectClauseStreamSelectorEnum,
	        SelectClauseSpecCompiled selectClauseSpec,
	        StreamSpecCompiled[] streamSpecs,
	        OuterJoinDesc[] outerJoinDescList,
	        ExprNode filterExprRootNode,
	        ExprNode havingExprRootNode,
	        OutputLimitSpec outputLimitSpec,
	        OrderByItem[] orderByList,
	        ExprSubselectNode[] subSelectExpressions,
	        ExprDeclaredNode[] declaredExpressions,
	        ExpressionScriptProvided[] scripts,
	        ICollection<string> variableReferences,
	        RowLimitSpec rowLimitSpec,
	        string[] eventTypeReferences,
	        Attribute[] annotations,
	        UpdateDesc updateSpec,
	        MatchRecognizeSpec matchRecognizeSpec,
	        ForClauseSpec forClauseSpec,
	        IDictionary<int, IList<ExprNode>> sqlParameters,
	        CreateContextDesc contextDesc,
	        string optionalContextName,
	        CreateDataFlowDesc createGraphDesc,
	        CreateExpressionDesc createExpressionDesc,
	        FireAndForgetSpec fireAndForgetSpec,
	        GroupByClauseExpressions groupByExpressions,
	        IntoTableSpec intoTableSpec,
	        ExprTableAccessNode[] tableNodes)
	    {
	        OnTriggerDesc = onTriggerDesc;
	        CreateWindowDesc = createWindowDesc;
	        CreateIndexDesc = createIndexDesc;
	        CreateVariableDesc = createVariableDesc;
	        CreateTableDesc = createTableDesc;
	        CreateSchemaDesc = createSchemaDesc;
	        InsertIntoDesc = insertIntoDesc;
	        SelectStreamDirEnum = selectClauseStreamSelectorEnum;
	        SelectClauseSpec = selectClauseSpec;
	        StreamSpecs = streamSpecs;
	        OuterJoinDescList = outerJoinDescList;
	        FilterExprRootNode = filterExprRootNode;
	        HavingExprRootNode = havingExprRootNode;
	        OutputLimitSpec = outputLimitSpec;
	        OrderByList = orderByList;
	        SubSelectExpressions = subSelectExpressions;
	        DeclaredExpressions = declaredExpressions;
	        Scripts = scripts;
	        VariableReferences = variableReferences;
	        RowLimitSpec = rowLimitSpec;
	        EventTypeReferences = eventTypeReferences;
	        Annotations = annotations;
	        UpdateSpec = updateSpec;
	        MatchRecognizeSpec = matchRecognizeSpec;
	        ForClauseSpec = forClauseSpec;
	        SqlParameters = sqlParameters;
	        ContextDesc = contextDesc;
	        OptionalContextName = optionalContextName;
	        CreateGraphDesc = createGraphDesc;
	        CreateExpressionDesc = createExpressionDesc;
	        FireAndForgetSpec = fireAndForgetSpec;
	        GroupByExpressions = groupByExpressions;
	        IntoTableSpec = intoTableSpec;
	        TableNodes = tableNodes;
	    }

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    public StatementSpecCompiled()
	    {
	        OnTriggerDesc = null;
	        CreateWindowDesc = null;
	        CreateIndexDesc = null;
	        CreateVariableDesc = null;
	        CreateTableDesc = null;
	        CreateSchemaDesc = null;
	        InsertIntoDesc = null;
	        SelectStreamDirEnum = SelectClauseStreamSelectorEnum.RSTREAM_ISTREAM_BOTH;
	        SelectClauseSpec = new SelectClauseSpecCompiled(false);
            StreamSpecs = StreamSpecCompiledConstants.EMPTY_STREAM_ARRAY;
	        OuterJoinDescList = OuterJoinDesc.EMPTY_OUTERJOIN_ARRAY;
	        FilterExprRootNode = null;
	        HavingExprRootNode = null;
	        OutputLimitSpec = null;
	        OrderByList = OrderByItem.EMPTY_ORDERBY_ARRAY;
	        SubSelectExpressions = ExprSubselectNode.EMPTY_SUBSELECT_ARRAY;
	        DeclaredExpressions = ExprNodeUtility.EMPTY_DECLARED_ARR;
            Scripts = ExprNodeUtility.EMPTY_SCRIPTS; 
            VariableReferences = new HashSet<string>();
	        RowLimitSpec = null;
	        EventTypeReferences = new string[0];
	        Annotations = new Attribute[0];
	        UpdateSpec = null;
	        MatchRecognizeSpec = null;
	        ForClauseSpec = null;
	        SqlParameters = null;
	        ContextDesc = null;
	        OptionalContextName = null;
	        CreateGraphDesc = null;
	        CreateExpressionDesc = null;
	        FireAndForgetSpec = null;
	        GroupByExpressions = null;
	        IntoTableSpec = null;
	        TableNodes = null;
	    }

	    /// <summary>
	    /// Returns the specification for an create-window statement.
	    /// </summary>
	    /// <value>create-window spec, or null if not such a statement</value>
	    public CreateWindowDesc CreateWindowDesc { get; private set; }

	    /// <summary>
	    /// Returns the create-variable statement descriptor.
	    /// </summary>
	    /// <value>create-variable spec</value>
	    public CreateVariableDesc CreateVariableDesc { get; private set; }

	    /// <summary>
	    /// Returns the FROM-clause stream definitions.
	    /// </summary>
	    /// <value>list of stream specifications</value>
	    public StreamSpecCompiled[] StreamSpecs { get; set; }

	    /// <summary>
	    /// Returns SELECT-clause list of expressions.
	    /// </summary>
	    /// <value>list of expressions and optional name</value>
	    public SelectClauseSpecCompiled SelectClauseSpec { get; set; }

	    /// <summary>
	    /// Returns the WHERE-clause root node of filter expression.
	    /// </summary>
	    /// <value>filter expression root node</value>
	    public ExprNode FilterRootNode
	    {
	        get { return FilterExprRootNode; }
	    }

	    /// <summary>
	    /// Returns the LEFT/RIGHT/FULL OUTER JOIN-type and property name descriptor, if applicable. Returns null if regular join.
	    /// </summary>
	    /// <value>outer join type, stream names and property names</value>
	    public OuterJoinDesc[] OuterJoinDescList { get; private set; }

	    /// <summary>
	    /// Returns expression root node representing the having-clause, if present, or null if no having clause was supplied.
	    /// </summary>
	    /// <value>having-clause expression top node</value>
	    public ExprNode HavingExprRootNode { get; private set; }

	    /// <summary>
	    /// Returns the output limit definition, if any.
	    /// </summary>
	    /// <value>output limit spec</value>
	    public OutputLimitSpec OutputLimitSpec { get; private set; }

	    /// <summary>
	    /// Return a descriptor with the insert-into event name and optional list of columns.
	    /// </summary>
	    /// <value>insert into specification</value>
	    public InsertIntoDesc InsertIntoDesc { get; set; }

	    /// <summary>
	    /// Returns the list of order-by expression as specified in the ORDER BY clause.
	    /// </summary>
	    /// <value>Returns the orderByList.</value>
	    public OrderByItem[] OrderByList { get; private set; }

	    /// <summary>
	    /// Returns the stream selector (rstream/istream).
	    /// </summary>
	    /// <value>stream selector</value>
	    public SelectClauseStreamSelectorEnum SelectStreamSelectorEnum
	    {
	        get { return SelectStreamDirEnum; }
	    }

	    /// <summary>
	    /// Set the where clause filter node.
	    /// </summary>
	    /// <value>is the where-clause filter node</value>
	    public ExprNode FilterExprRootNode { private get; set; }

	    /// <summary>
	    /// Returns the list of lookup expression nodes.
	    /// </summary>
	    /// <value>lookup nodes</value>
	    public ExprSubselectNode[] SubSelectExpressions { get; private set; }

	    /// <summary>
	    /// Returns the specification for an on-delete or on-select statement.
	    /// </summary>
	    /// <value>on-trigger spec, or null if not such a statement</value>
	    public OnTriggerDesc OnTriggerDesc { get; private set; }

	    /// <summary>
	    /// Returns true to indicate the statement has variables.
	    /// </summary>
	    /// <value>true for statements that use variables</value>
	    public bool HasVariables
	    {
	        get { return VariableReferences != null && !VariableReferences.IsEmpty(); }
	    }

	    /// <summary>
	    /// Sets the stream selection.
	    /// </summary>
	    /// <value>stream selection</value>
	    public SelectClauseStreamSelectorEnum SelectStreamDirEnum { private get; set; }

	    /// <summary>
	    /// Returns the row limit specification, or null if none supplied.
	    /// </summary>
	    /// <value>row limit spec if any</value>
	    public RowLimitSpec RowLimitSpec { get; private set; }

	    /// <summary>
	    /// Returns the event type name in used by the statement.
	    /// </summary>
	    /// <value>set of event type name</value>
	    public string[] EventTypeReferences { get; private set; }

	    /// <summary>
	    /// Returns annotations or empty array if none.
	    /// </summary>
	    /// <value>annotations</value>
	    public Attribute[] Annotations { get; private set; }

	    /// <summary>
	    /// Returns the update spec if update clause is used.
	    /// </summary>
	    /// <value>update desc</value>
	    public UpdateDesc UpdateSpec { get; private set; }

	    /// <summary>
	    /// Returns the match recognize spec, if used
	    /// </summary>
	    /// <value>match recognize spec</value>
	    public MatchRecognizeSpec MatchRecognizeSpec { get; private set; }

	    /// <summary>
	    /// Return variables referenced.
	    /// </summary>
	    /// <value>variables</value>
	    public ICollection<string> VariableReferences { get; private set; }

	    /// <summary>
	    /// Returns create index
	    /// </summary>
	    /// <value>create index</value>
	    public CreateIndexDesc CreateIndexDesc { get; private set; }

	    public CreateSchemaDesc CreateSchemaDesc { get; private set; }

	    public ForClauseSpec ForClauseSpec { get; private set; }

	    public IDictionary<int, IList<ExprNode>> SqlParameters { get; private set; }

	    public ExprDeclaredNode[] DeclaredExpressions { get; private set; }

        public ExpressionScriptProvided[] Scripts { get; private set; }

	    public CreateContextDesc ContextDesc { get; private set; }

	    public string OptionalContextName { get; private set; }

	    public CreateDataFlowDesc CreateGraphDesc { get; private set; }

	    public CreateExpressionDesc CreateExpressionDesc { get; private set; }

	    public FireAndForgetSpec FireAndForgetSpec { get; private set; }

	    public GroupByClauseExpressions GroupByExpressions { get; private set; }

	    public IntoTableSpec IntoTableSpec { get; private set; }

	    public ExprTableAccessNode[] TableNodes { get; private set; }

	    public CreateTableDesc CreateTableDesc { get; private set; }

        public FilterSpecCompiled[] FilterSpecsOverall { get; set; }

	    public NamedWindowConsumerStreamSpec[] NamedWindowConsumersAll { get; set; }
	}
} // end of namespace
