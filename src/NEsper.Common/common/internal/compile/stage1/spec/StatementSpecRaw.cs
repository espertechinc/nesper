///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.table;

namespace com.espertech.esper.common.@internal.compile.stage1.spec
{
    /// <summary>
    ///     Specification object representing a complete EPL statement including all EPL constructs.
    /// </summary>
    [Serializable]
    public class StatementSpecRaw
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="defaultStreamSelector">stream selection for the statement</param>
        public StatementSpecRaw(SelectClauseStreamSelectorEnum defaultStreamSelector)
        {
            SelectStreamSelectorEnum = defaultStreamSelector;
        }

        /// <summary>
        ///     Returns the FROM-clause stream definitions.
        /// </summary>
        /// <returns>list of stream specifications</returns>
        public IList<StreamSpecRaw> StreamSpecs { get; set; } = new List<StreamSpecRaw>();

        /// <summary>
        ///     Returns SELECT-clause list of expressions.
        /// </summary>
        /// <returns>list of expressions and optional name</returns>
        public SelectClauseSpecRaw SelectClauseSpec { get; set; } = new SelectClauseSpecRaw();

        /// <summary>
        ///     Returns the LEFT/RIGHT/FULL OUTER JOIN-type and property name descriptor, if applicable. Returns null if regular
        ///     join.
        /// </summary>
        /// <returns>outer join type, stream names and property names</returns>
        public IList<OuterJoinDesc> OuterJoinDescList { get; set; } = new List<OuterJoinDesc>();

        /// <summary>
        ///     Returns list of group-by expressions.
        /// </summary>
        /// <returns>group-by expression nodes as specified in group-by clause</returns>
        public IList<GroupByClauseElement> GroupByExpressions { get; set; } = new List<GroupByClauseElement>(2);

        /// <summary>
        ///     Returns expression root node representing the having-clause, if present, or null if no having clause was supplied.
        /// </summary>
        /// <returns>having-clause expression top node</returns>
        public ExprNode HavingClause { get; set; }

        /// <summary>
        ///     Returns the output limit definition, if any.
        /// </summary>
        /// <returns>output limit spec</returns>
        public OutputLimitSpec OutputLimitSpec { get; set; }

        /// <summary>
        ///     Return a descriptor with the insert-into event name and optional list of columns.
        /// </summary>
        /// <returns>insert into specification</returns>
        public InsertIntoDesc InsertIntoDesc { get; set; }

        /// <summary>
        ///     Returns the list of order-by expression as specified in the ORDER BY clause.
        /// </summary>
        /// <returns>Returns the orderByList.</returns>
        public IList<OrderByItem> OrderByList { get; } = new List<OrderByItem>();

        /// <summary>
        ///     Returns the stream selector (rstream/istream).
        /// </summary>
        /// <returns>stream selector</returns>
        public SelectClauseStreamSelectorEnum SelectStreamSelectorEnum { get; set; }

        /// <summary>
        ///     Returns the create-window specification.
        /// </summary>
        /// <returns>descriptor for creating a named window</returns>
        public CreateWindowDesc CreateWindowDesc { get; set; }

        /// <summary>
        ///     Returns the on-delete statement specification.
        /// </summary>
        /// <returns>descriptor for creating a an on-delete statement</returns>
        public OnTriggerDesc OnTriggerDesc { get; set; }

        /// <summary>
        ///     Gets the where clause.
        /// </summary>
        /// <returns>where clause or null if none</returns>
        public ExprNode WhereClause { get; set; }

        /// <summary>
        ///     Returns the descriptor for create-variable statements.
        /// </summary>
        /// <returns>create-variable info</returns>
        public CreateVariableDesc CreateVariableDesc { get; set; }

        /// <summary>
        ///     Returns the row limit, or null if none.
        /// </summary>
        /// <returns>row limit</returns>
        public RowLimitSpec RowLimitSpec { get; set; }

        /// <summary>
        ///     Returns a list of annotation descriptors.
        /// </summary>
        /// <returns>annotation descriptors</returns>
        public IList<AnnotationDesc> Annotations { get; set; } = new List<AnnotationDesc>(1);

        /// <summary>
        ///     Returns the update spec.
        /// </summary>
        /// <returns>update spec</returns>
        public UpdateDesc UpdateDesc { get; set; }

        /// <summary>
        ///     Returns the expression text without annotations.
        /// </summary>
        /// <returns>expressionNoAnnotations text</returns>
        public string ExpressionNoAnnotations { get; set; }

        /// <summary>
        ///     Returns the match recognize spec.
        /// </summary>
        /// <returns>spec</returns>
        public MatchRecognizeSpec MatchRecognizeSpec { get; set; }

        /// <summary>
        ///     Returns variables referenced
        /// </summary>
        /// <returns>vars</returns>
        public ISet<string> ReferencedVariables { get; set; } = new HashSet<string>();

        /// <summary>
        ///     Returns create-index if any.
        /// </summary>
        /// <returns>index create</returns>
        public CreateIndexDesc CreateIndexDesc { get; set; }

        public CreateSchemaDesc CreateSchemaDesc { get; set; }

        public ForClauseSpec ForClauseSpec { get; set; }

        public IDictionary<int, IList<ExprNode>> SqlParameters { get; set; }

        public IList<ExprSubstitutionNode> SubstitutionParameters { get; set; }

        public ExpressionDeclDesc ExpressionDeclDesc { get; set; }

        public CreateContextDesc CreateContextDesc { get; set; }

        public string OptionalContextName { get; set; }

        public IList<ExpressionScriptProvided> ScriptExpressions { get; set; }
        
        public IList<String> ClassProvidedList { get; set; }

        public CreateDataFlowDesc CreateDataFlowDesc { get; set; }

        public CreateExpressionDesc CreateExpressionDesc { get; set; }
        
        public String CreateClassProvided { get; set; }

        public FireAndForgetSpec FireAndForgetSpec {
            get;
            set;
        }

        public IntoTableSpec IntoTableSpec { get; set; }

        public ISet<ExprTableAccessNode> TableExpressions { get; set; } = new HashSet<ExprTableAccessNode>();

        public CreateTableDesc CreateTableDesc { get; set; }

        public bool HasPriorExpressions { get; set; }

        /// <summary>
        ///     Sets the stream selector (rstream/istream/both etc).
        /// </summary>
        /// <value>to be set</value>
        public SelectClauseStreamSelectorEnum SelectStreamDirEnum {
            set => SelectStreamSelectorEnum = value;
        }
    }
} // end of namespac