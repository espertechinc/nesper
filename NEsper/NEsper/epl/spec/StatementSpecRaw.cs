///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.expression.table;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.spec
{
    /// <summary>
    /// Specification object representing a complete EPL statement including all EPL constructs.
    /// </summary>
    [Serializable]
    public class StatementSpecRaw : MetaDefItem
    {
        /// <summary>Ctor. </summary>
        /// <param name="defaultStreamSelector">stream selection for the statement</param>
        public StatementSpecRaw(SelectClauseStreamSelectorEnum defaultStreamSelector)
        {
            OrderByList = new List<OrderByItem>();
            StreamSpecs = new List<StreamSpecRaw>();
            SelectClauseSpec = new SelectClauseSpecRaw();
            OuterJoinDescList = new List<OuterJoinDesc>();
            GroupByExpressions = new List<GroupByClauseElement>();
            Annotations = new List<AnnotationDesc>(1);
            SelectStreamDirEnum = defaultStreamSelector;
        }

        /// <summary>Returns the FROM-clause stream definitions. </summary>
        /// <value>list of stream specifications</value>
        public IList<StreamSpecRaw> StreamSpecs { get; private set; }

        /// <summary>Returns SELECT-clause list of expressions. </summary>
        /// <value>list of expressions and optional name</value>
        public SelectClauseSpecRaw SelectClauseSpec { get; set; }

        /// <summary>Returns the WHERE-clause root node of filter expression. </summary>
        /// <value>filter expression root node</value>
        public ExprNode FilterRootNode
        {
            get { return FilterExprRootNode; }
        }

        /// <summary>Returns the LEFT/RIGHT/FULL OUTER JOIN-type and property name descriptor, if applicable. Returns null if regular join. </summary>
        /// <value>outer join type, stream names and property names</value>
        public IList<OuterJoinDesc> OuterJoinDescList { get; private set; }

        /// <summary>Returns list of group-by expressions. </summary>
        /// <value>group-by expression nodes as specified in group-by clause</value>
        public IList<GroupByClauseElement> GroupByExpressions { get; private set; }

        /// <summary>Returns expression root node representing the having-clause, if present, or null if no having clause was supplied. </summary>
        /// <value>having-clause expression top node</value>
        public ExprNode HavingExprRootNode { get; set; }

        /// <summary>Returns the output limit definition, if any. </summary>
        /// <value>output limit spec</value>
        public OutputLimitSpec OutputLimitSpec { get; set; }

        /// <summary>Return a descriptor with the insert-into event name and optional list of columns. </summary>
        /// <value>insert into specification</value>
        public InsertIntoDesc InsertIntoDesc { get; set; }

        /// <summary>Returns the list of order-by expression as specified in the ORDER BY clause. </summary>
        /// <value>Returns the orderByList.</value>
        public IList<OrderByItem> OrderByList { get; private set; }

        /// <summary>Returns the stream selector (rstream/istream). </summary>
        /// <value>stream selector</value>
        public SelectClauseStreamSelectorEnum SelectStreamSelectorEnum
        {
            get { return SelectStreamDirEnum; }
        }

        /// <summary>Sets the stream selector (rstream/istream/both etc). </summary>
        /// <value>to be set</value>
        public SelectClauseStreamSelectorEnum SelectStreamDirEnum { get; set; }

        /// <summary>Returns the create-window specification. </summary>
        /// <value>descriptor for creating a named window</value>
        public CreateWindowDesc CreateWindowDesc { get; set; }

        /// <summary>Returns the on-delete statement specification. </summary>
        /// <value>descriptor for creating a an on-delete statement</value>
        public OnTriggerDesc OnTriggerDesc { get; set; }

        /// <summary>Gets the where clause. </summary>
        /// <value>where clause or null if none</value>
        public ExprNode FilterExprRootNode { get; set; }

        /// <summary>Returns true if a statement (or subquery sub-statements) use variables. </summary>
        /// <value>indicator if variables are used</value>
        public bool HasVariables { get; set; }

        /// <summary>Returns the descriptor for create-variable statements. </summary>
        /// <value>create-variable info</value>
        public CreateVariableDesc CreateVariableDesc { get; set; }

        /// <summary>Gets or sets the desciptor for create-table statements.</summary>
        /// <value>create-table info</value>
        public CreateTableDesc CreateTableDesc { get; set; }

        /// <summary>Returns the row limit, or null if none. </summary>
        /// <value>row limit</value>
        public RowLimitSpec RowLimitSpec { get; set; }

        /// <summary>Returns a list of annotation descriptors. </summary>
        /// <value>annotation descriptors</value>
        public IList<AnnotationDesc> Annotations { get; set; }

        /// <summary>Returns the Update spec. </summary>
        /// <value>Update spec</value>
        public UpdateDesc UpdateDesc { get; set; }

        /// <summary>Returns the expression text without annotations. </summary>
        /// <value>expressionNoAnnotations text</value>
        public string ExpressionNoAnnotations { get; set; }

        /// <summary>Returns the match recognize spec. </summary>
        /// <value>spec</value>
        public MatchRecognizeSpec MatchRecognizeSpec { get; set; }

        /// <summary>Returns the variables referenced </summary>
        /// <value>vars</value>
        public ICollection<string> ReferencedVariables { get; set; }

        /// <summary>Returns create-index if any. </summary>
        /// <value>index create</value>
        public CreateIndexDesc CreateIndexDesc { get; set; }

        public CreateSchemaDesc CreateSchemaDesc { get; set; }

        public ForClauseSpec ForClauseSpec { get; set; }

        public IDictionary<int, IList<ExprNode>> SqlParameters { get; set; }

        public IList<ExprSubstitutionNode> SubstitutionParameters { get; set; }

        public ExpressionDeclDesc ExpressionDeclDesc { get; set; }

        public CreateContextDesc CreateContextDesc { get; set; }

        public string OptionalContextName { get; set; }

        public IList<ExpressionScriptProvided> ScriptExpressions { get; set; }

        public CreateDataFlowDesc CreateDataFlowDesc { get; set; }

        public CreateExpressionDesc CreateExpressionDesc { get; set; }

        public FireAndForgetSpec FireAndForgetSpec { get; set; }

        public IntoTableSpec IntoTableSpec { get; set; }

        public ISet<ExprTableAccessNode> TableExpressions { get; set; }
    }
}
