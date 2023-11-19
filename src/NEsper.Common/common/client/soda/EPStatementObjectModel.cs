///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    ///     Object model of an EPL statement.
    ///     <para>
    ///         Applications can create an object model by instantiating this class and then setting the various clauses.
    ///         When done, use the administrative interface to deploy from the model.
    ///     </para>
    ///     <para>
    ///         Use the toEPL method to generate a textual EPL from an object model.
    ///     </para>
    ///     <para>
    ///         Minimally, and EPL statement consists of the select-clause and the where-clause. These are represented by
    ///         <seealso cref="SelectClause(com.espertech.esper.common.client.soda.SelectClause)" />and
    ///         <seealso cref="FromClause(com.espertech.esper.common.client.soda.FromClause)" /> respectively.
    ///     </para>
    ///     <para>
    ///         Here is a short example that create a simple EPL statement such as "select page, responseTime from PageLoad" :
    ///         EPStatementObjectModel model = new EPStatementObjectModel();
    ///         model.setSelectClause(SelectClause.create("page", "responseTime"));
    ///         model.setPropertyEvalSpec(FromClause.create(FilterStream.create("PageLoad")));
    ///     </para>
    ///     <para>
    ///         The select-clause and from-clause must be set for the statement object model to be useable by the
    ///         administrative API. All other clauses a optional.
    ///     </para>
    ///     <para>
    ///         Please see the documentation set for further examples.
    ///     </para>
    /// </summary>
    public class EPStatementObjectModel
    {
        private IList<AnnotationPart> annotations;
        private IList<ClassProvidedExpression> classProvidedExpressions;
        private string contextName;
        private CreateClassClause createClass;
        private CreateContextClause createContext;
        private CreateDataFlowClause createDataFlow;
        private CreateExpressionClause createExpression;
        private CreateIndexClause createIndex;
        private CreateSchemaClause createSchema;
        private CreateTableClause createTable;
        private CreateVariableClause createVariable;
        private CreateWindowClause createWindow;
        private IList<ExpressionDeclaration> expressionDeclarations;
        private FireAndForgetClause fireAndForgetClause;
        private ForClause forClause;
        private FromClause fromClause;
        private GroupByClause groupByClause;
        private Expression havingClause;
        private InsertIntoClause insertInto;
        private IntoTableClause intoTableClause;
        private MatchRecognizeClause matchRecognizeClause;
        private OnClause onExpr;
        private OrderByClause orderByClause;
        private OutputLimitClause outputLimitClause;
        private RowLimitClause rowLimitClause;
        private IList<ScriptExpression> scriptExpressions;
        private SelectClause selectClause;
        private string treeObjectName;
        private UpdateClause updateClause;
        private Expression whereClause;

        /// <summary>
        ///     Ctor.
        /// </summary>
        public EPStatementObjectModel()
        {
        }

        /// <summary>
        ///     Return the insert-into-clause, or null to indicate that the clause is absent.
        /// </summary>
        /// <value>specification of the insert-into-clause, or null if none present</value>
        public InsertIntoClause InsertInto {
            get => insertInto;
            set => insertInto = value;
        }

        /// <summary>
        ///     Specify a select-clause.
        /// </summary>
        /// <value>specifies the select-clause, the select-clause cannot be null and must be set</value>
        public SelectClause SelectClause {
            set => selectClause = value;
            get => selectClause;
        }

        /// <summary>
        ///     Specify a from-clause.
        /// </summary>
        /// <value>specifies the from-clause, the from-clause cannot be null and must be set</value>
        public FromClause FromClause {
            set => fromClause = value;
            get => fromClause;
        }

        /// <summary>
        ///     Return the where-clause, or null to indicate that the clause is absent.
        /// </summary>
        /// <value>specification of the where-clause, or null if none present</value>
        public Expression WhereClause {
            get => whereClause;
            set => whereClause = value;
        }

        /// <summary>
        ///     Return the group-by-clause, or null to indicate that the clause is absent.
        /// </summary>
        /// <value>specification of the group-by-clause, or null if none present</value>
        public GroupByClause GroupByClause {
            get => groupByClause;
            set => groupByClause = value;
        }

        /// <summary>
        ///     Return the having-clause, or null to indicate that the clause is absent.
        /// </summary>
        /// <value>specification of the having-clause, or null if none present</value>
        public Expression HavingClause {
            get => havingClause;
            set => havingClause = value;
        }

        /// <summary>
        ///     Return the order-by-clause, or null to indicate that the clause is absent.
        /// </summary>
        /// <value>specification of the order-by-clause, or null if none present</value>
        public OrderByClause OrderByClause {
            get => orderByClause;
            set => orderByClause = value;
        }

        /// <summary>
        ///     Return the output-rate-limiting-clause, or null to indicate that the clause is absent.
        /// </summary>
        /// <value>specification of the output-rate-limiting-clause, or null if none present</value>
        public OutputLimitClause OutputLimitClause {
            get => outputLimitClause;
            set => outputLimitClause = value;
        }

        /// <summary>
        ///     Returns the row limit specification, or null if none supplied.
        /// </summary>
        /// <value>row limit spec if any</value>
        public RowLimitClause RowLimitClause {
            get => rowLimitClause;
            set => rowLimitClause = value;
        }

        /// <summary>
        ///     Returns the update specification.
        /// </summary>
        /// <value>update spec if defined</value>
        public UpdateClause UpdateClause {
            get => updateClause;
            set => updateClause = value;
        }

        /// <summary>
        ///     Returns annotations.
        /// </summary>
        /// <value>annotations</value>
        public IList<AnnotationPart> Annotations {
            get => annotations;
            set => annotations = value;
        }

        /// <summary>
        ///     Match-recognize clause.
        /// </summary>
        /// <value>clause</value>
        public MatchRecognizeClause MatchRecognizeClause {
            get => matchRecognizeClause;
            set => matchRecognizeClause = value;
        }

        /// <summary>
        ///     Returns create-index clause.
        /// </summary>
        /// <value>clause</value>
        public CreateIndexClause CreateIndex {
            get => createIndex;
            set => createIndex = value;
        }

        /// <summary>
        ///     Returns the create-schema clause.
        /// </summary>
        /// <value>clause</value>
        public CreateSchemaClause CreateSchema {
            get => createSchema;
            set => createSchema = value;
        }

        /// <summary>
        ///     Returns the create-context clause.
        /// </summary>
        /// <value>clause</value>
        public CreateContextClause CreateContext {
            get => createContext;
            set => createContext = value;
        }

        /// <summary>
        ///     Returns the for-clause.
        /// </summary>
        /// <value>for-clause</value>
        public ForClause ForClause {
            get => forClause;
            set => forClause = value;
        }

        /// <summary>
        ///     Returns the expression declarations, if any.
        /// </summary>
        /// <value>expression declarations</value>
        public IList<ExpressionDeclaration> ExpressionDeclarations {
            get => expressionDeclarations;
            set => expressionDeclarations = value;
        }

        /// <summary>
        ///     Returns the context name if context dimensions apply to statement.
        /// </summary>
        /// <value>context name</value>
        public string ContextName {
            get => contextName;
            set => contextName = value;
        }

        /// <summary>
        ///     Returns the scripts defined.
        /// </summary>
        /// <value>scripts</value>
        public IList<ScriptExpression> ScriptExpressions {
            get => scriptExpressions;
            set => scriptExpressions = value;
        }

        /// <summary>
        ///     Returns the inlined-classes provided as part of the EPL statement
        /// </summary>
        /// <value>inlined-classes</value>
        public IList<ClassProvidedExpression> ClassProvidedExpressions {
            get => classProvidedExpressions;
            set => classProvidedExpressions = value;
        }

        /// <summary>
        ///     Returns the "create dataflow" part, if present.
        /// </summary>
        /// <value>create dataflow clause</value>
        public CreateDataFlowClause CreateDataFlow {
            get => createDataFlow;
            set => createDataFlow = value;
        }

        /// <summary>
        ///     Returns the internal expression id assigned for tools to identify the expression.
        /// </summary>
        /// <value>object name</value>
        public string TreeObjectName {
            get => treeObjectName;
            set => treeObjectName = value;
        }

        /// <summary>
        ///     Returns the create-expression clause, if any
        /// </summary>
        /// <value>clause</value>
        public CreateExpressionClause CreateExpression {
            get => createExpression;
            set => createExpression = value;
        }

        /// <summary>
        ///     Returns the create-class clause or null if not present.
        /// </summary>
        /// <value>create-class clause</value>
        public CreateClassClause CreateClass {
            get => createClass;
            set => createClass = value;
        }

        /// <summary>
        ///     Returns fire-and-forget (on-demand) query information for FAF select, insert, update and delete.
        /// </summary>
        /// <value>fire and forget query information</value>
        public FireAndForgetClause FireAndForgetClause {
            get => fireAndForgetClause;
            set => fireAndForgetClause = value;
        }

        /// <summary>
        ///     Returns the into-table clause, or null if none found.
        /// </summary>
        /// <value>into-table clause</value>
        public IntoTableClause IntoTableClause {
            get => intoTableClause;
            set => intoTableClause = value;
        }

        /// <summary>
        ///     Returns the create-table clause if present or null if not present
        /// </summary>
        /// <value>create-table clause</value>
        public CreateTableClause CreateTable {
            get => createTable;
            set => createTable = value;
        }

        /// <summary>
        ///     Returns the create-window clause for creating named windows, or null if this statement does not
        ///     create a named window.
        /// </summary>
        /// <value>named window creation clause</value>
        public CreateWindowClause CreateWindow {
            get => createWindow;
            set => createWindow = value;
        }

        /// <summary>
        ///     Returns the on-delete clause for deleting from named windows, or null if this statement
        ///     does not delete from a named window
        /// </summary>
        /// <value>on delete clause</value>
        public OnClause OnExpr {
            get => onExpr;
            set => onExpr = value;
        }

        /// <summary>
        ///     Returns the create-variable clause if this is a statement creating a variable, or null if not.
        /// </summary>
        /// <value>create-variable clause</value>
        public CreateVariableClause CreateVariable {
            get => createVariable;
            set => createVariable = value;
        }

        /// <summary>
        ///     Specify an insert-into-clause.
        /// </summary>
        /// <param name="insertInto">specifies the insert-into-clause, or null to indicate that the clause is absent</param>
        /// <returns>model</returns>
        public EPStatementObjectModel WithInsertInto(InsertIntoClause insertInto)
        {
            InsertInto = insertInto;
            return this;
        }

        /// <summary>
        ///     Specify a select-clause.
        /// </summary>
        /// <param name="selectClause">specifies the select-clause, the select-clause cannot be null and must be set</param>
        /// <returns>model</returns>
        public EPStatementObjectModel WithSelectClause(SelectClause selectClause)
        {
            this.selectClause = selectClause;
            return this;
        }

        /// <summary>
        ///     Specify a from-clause.
        /// </summary>
        /// <param name="fromClause">specifies the from-clause, the from-clause cannot be null and must be set</param>
        /// <returns>model</returns>
        public EPStatementObjectModel WithFromClause(FromClause fromClause)
        {
            this.fromClause = fromClause;
            return this;
        }

        /// <summary>
        ///     Specify a where-clause.
        /// </summary>
        /// <param name="whereClause">specifies the where-clause, which is optional and can be null</param>
        /// <returns>model</returns>
        public EPStatementObjectModel WithWhereClause(Expression whereClause)
        {
            this.whereClause = whereClause;
            return this;
        }

        /// <summary>
        ///     Specify a group-by-clause.
        /// </summary>
        /// <param name="groupByClause">specifies the group-by-clause, which is optional and can be null</param>
        /// <returns>model</returns>
        public EPStatementObjectModel WithGroupByClause(GroupByClause groupByClause)
        {
            this.groupByClause = groupByClause;
            return this;
        }

        /// <summary>
        ///     Specify a having-clause.
        /// </summary>
        /// <param name="havingClause">specifies the having-clause, which is optional and can be null</param>
        /// <returns>model</returns>
        public EPStatementObjectModel WithHavingClause(Expression havingClause)
        {
            this.havingClause = havingClause;
            return this;
        }

        /// <summary>
        ///     Specify an order-by-clause.
        /// </summary>
        /// <param name="orderByClause">specifies the order-by-clause, which is optional and can be null</param>
        /// <returns>model</returns>
        public EPStatementObjectModel WithOrderByClause(OrderByClause orderByClause)
        {
            this.orderByClause = orderByClause;
            return this;
        }

        /// <summary>
        ///     Specify an output-rate-limiting-clause.
        /// </summary>
        /// <param name="outputLimitClause">specifies the output-rate-limiting-clause, which is optional and can be null</param>
        /// <returns>model</returns>
        public EPStatementObjectModel WithOutputLimitClause(OutputLimitClause outputLimitClause)
        {
            this.outputLimitClause = outputLimitClause;
            return this;
        }

        /// <summary>
        ///     Renders the object model in it's EPL syntax textual representation.
        /// </summary>
        /// <returns>EPL representing the statement object model</returns>
        /// <throws>IllegalStateException if required clauses do not exist</throws>
        public string ToEPL()
        {
            var writer = new StringWriter();
            ToEPL(new EPStatementFormatter(false), writer);
            return writer.ToString();
        }

        /// <summary>
        ///     Rendering using the provided writer.
        /// </summary>
        /// <param name="writer">to use</param>
        public void ToEPL(TextWriter writer)
        {
            ToEPL(new EPStatementFormatter(false), writer);
        }

        /// <summary>
        ///     Rendering using the provided formatter.
        /// </summary>
        /// <param name="formatter">to use</param>
        /// <returns>rendered string</returns>
        public string ToEPL(EPStatementFormatter formatter)
        {
            var writer = new StringWriter();
            ToEPL(formatter, writer);
            return writer.ToString();
        }

        /// <summary>
        ///     Renders the object model in it's EPL syntax textual representation, using a whitespace-formatter as provided.
        /// </summary>
        /// <param name="formatter">the formatter to use</param>
        /// <param name="writer">writer to use</param>
        /// <throws>IllegalStateException if required clauses do not exist</throws>
        public void ToEPL(
            EPStatementFormatter formatter,
            TextWriter writer)
        {
            AnnotationPart.ToEPL(writer, annotations, formatter);
            ExpressionDeclaration.ToEPL(writer, expressionDeclarations, formatter);
            ScriptExpression.ToEPL(writer, scriptExpressions, formatter);
            ClassProvidedExpression.ToEPL(writer, classProvidedExpressions, formatter);

            if (contextName != null) {
                formatter.BeginContext(writer);
                writer.Write("context ");
                writer.Write(contextName);
            }

            if (createIndex != null) {
                formatter.BeginCreateIndex(writer);
                createIndex.ToEPL(writer);
                return;
            }

            if (createSchema != null) {
                formatter.BeginCreateSchema(writer);
                createSchema.ToEPL(writer);
                return;
            }

            if (createExpression != null) {
                formatter.BeginCreateExpression(writer);
                createExpression.ToEPL(writer);
                return;
            }

            if (createClass != null) {
                formatter.BeginCreateExpression(writer);
                createClass.ToEPL(writer);
                return;
            }

            if (createContext != null) {
                formatter.BeginCreateContext(writer);
                createContext.ToEPL(writer, formatter);
                return;
            }

            if (createWindow != null) {
                formatter.BeginCreateWindow(writer);
                createWindow.ToEPL(writer);

                writer.Write(" as ");
                if (selectClause == null || (selectClause.SelectList.IsEmpty() && !createWindow.Columns.IsEmpty())) {
                    createWindow.ToEPLCreateTablePart(writer);
                }
                else {
                    selectClause.ToEPL(writer, formatter, false, false);
                    if (createWindow.AsEventTypeName != null) {
                        writer.Write(" from ");
                        writer.Write(createWindow.AsEventTypeName);
                    }

                    createWindow.ToEPLInsertPart(writer);
                }

                return;
            }

            if (createVariable != null) {
                formatter.BeginCreateVariable(writer);
                createVariable.ToEPL(writer);
                return;
            }

            if (createTable != null) {
                formatter.BeginCreateTable(writer);
                createTable.ToEPL(writer);
                return;
            }

            if (createDataFlow != null) {
                formatter.BeginCreateDataFlow(writer);
                createDataFlow.ToEPL(writer, formatter);
                return;
            }

            var displayWhereClause = true;
            if (updateClause != null) {
                formatter.BeginUpdate(writer);
                updateClause.ToEPL(writer);
            }
            else if (onExpr != null) {
                formatter.BeginOnTrigger(writer);
                writer.Write("on ");
                fromClause.Streams[0].ToEPL(writer, formatter);

                if (onExpr is OnDeleteClause clause) {
                    formatter.BeginOnDelete(writer);
                    writer.Write("delete from ");
                    clause.ToEPL(writer);
                }
                else if (onExpr is OnUpdateClause onUpdateClause) {
                    formatter.BeginOnUpdate(writer);
                    writer.Write("update ");
                    onUpdateClause.ToEPL(writer);
                }
                else if (onExpr is OnSelectClause onSelect) {
                    if (InsertInto != null) {
                        InsertInto.ToEPL(writer, formatter, true);
                    }

                    selectClause.ToEPL(writer, formatter, InsertInto == null, onSelect.IsDeleteAndSelect);
                    writer.Write(" from ");
                    onSelect.ToEPL(writer);
                }
                else if (onExpr is OnSetClause onSet) {
                    onSet.ToEPL(writer, formatter);
                }
                else if (onExpr is OnMergeClause merge) {
                    merge.ToEPL(writer, whereClause, formatter);
                    displayWhereClause = false;
                }
                else {
                    var split = (OnInsertSplitStreamClause)onExpr;
                    InsertInto.ToEPL(writer, formatter, true);
                    selectClause.ToEPL(writer, formatter, false, false);
                    if (whereClause != null) {
                        writer.Write(" where ");
                        whereClause.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                    }

                    split.ToEPL(writer, formatter);
                    displayWhereClause = false;
                }
            }
            else {
                if (intoTableClause != null) {
                    intoTableClause.ToEPL(writer, formatter);
                }

                if (selectClause == null) {
                    throw new IllegalStateException("Select-clause has not been defined");
                }

                if (fromClause == null) {
                    throw new IllegalStateException("From-clause has not been defined");
                }

                if (fireAndForgetClause is FireAndForgetUpdate update) {
                    writer.Write("update ");
                    fromClause.ToEPLOptions(writer, formatter, false);
                    writer.Write(" ");
                    UpdateClause.RenderEPLAssignments(writer, update.Assignments);
                }
                else if (fireAndForgetClause is FireAndForgetInsert insert) {
                    InsertInto.ToEPL(writer, formatter, true);
                    if (insert.IsUseValuesKeyword) {
                        insert.ToEPL(writer);
                    }
                    else {
                        selectClause.ToEPL(writer, formatter, false, false);
                    }
                }
                else if (fireAndForgetClause is FireAndForgetDelete) {
                    writer.Write("delete ");
                    fromClause.ToEPLOptions(writer, formatter, true);
                }
                else {
                    if (InsertInto != null) {
                        InsertInto.ToEPL(writer, formatter, intoTableClause == null);
                    }

                    selectClause.ToEPL(writer, formatter, intoTableClause == null && InsertInto == null, false);
                    fromClause.ToEPLOptions(writer, formatter, true);
                }
            }

            if (matchRecognizeClause != null) {
                matchRecognizeClause.ToEPL(writer);
            }

            if (whereClause != null && displayWhereClause) {
                formatter.BeginWhere(writer);
                writer.Write("where ");
                whereClause.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }

            if (groupByClause != null) {
                formatter.BeginGroupBy(writer);
                writer.Write("group by ");
                groupByClause.ToEPL(writer);
            }

            if (havingClause != null) {
                formatter.BeginHaving(writer);
                writer.Write("having ");
                havingClause.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }

            if (outputLimitClause != null) {
                formatter.BeginOutput(writer);
                writer.Write("output ");
                outputLimitClause.ToEPL(writer);
            }

            if (orderByClause != null) {
                formatter.BeginOrderBy(writer);
                writer.Write("order by ");
                orderByClause.ToEPL(writer);
            }

            if (rowLimitClause != null) {
                formatter.BeginLimit(writer);
                writer.Write("limit ");
                rowLimitClause.ToEPL(writer);
            }

            if (forClause != null) {
                formatter.BeginFor(writer);
                forClause.ToEPL(writer);
            }
        }
    }
} // end of namespace