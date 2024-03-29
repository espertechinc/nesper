///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
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
        public InsertIntoClause InsertInto { get; set; }

        /// <summary>
        ///     Specify a select-clause.
        /// </summary>
        /// <value>specifies the select-clause, the select-clause cannot be null and must be set</value>
        public SelectClause SelectClause { set; get; }

        /// <summary>
        ///     Specify a from-clause.
        /// </summary>
        /// <value>specifies the from-clause, the from-clause cannot be null and must be set</value>
        public FromClause FromClause { set; get; }

        /// <summary>
        ///     Return the where-clause, or null to indicate that the clause is absent.
        /// </summary>
        /// <value>specification of the where-clause, or null if none present</value>
        public Expression WhereClause { get; set; }

        /// <summary>
        ///     Return the group-by-clause, or null to indicate that the clause is absent.
        /// </summary>
        /// <value>specification of the group-by-clause, or null if none present</value>
        public GroupByClause GroupByClause { get; set; }

        /// <summary>
        ///     Return the having-clause, or null to indicate that the clause is absent.
        /// </summary>
        /// <value>specification of the having-clause, or null if none present</value>
        public Expression HavingClause { get; set; }

        /// <summary>
        ///     Return the order-by-clause, or null to indicate that the clause is absent.
        /// </summary>
        /// <value>specification of the order-by-clause, or null if none present</value>
        public OrderByClause OrderByClause { get; set; }

        /// <summary>
        ///     Return the output-rate-limiting-clause, or null to indicate that the clause is absent.
        /// </summary>
        /// <value>specification of the output-rate-limiting-clause, or null if none present</value>
        public OutputLimitClause OutputLimitClause { get; set; }

        /// <summary>
        ///     Returns the row limit specification, or null if none supplied.
        /// </summary>
        /// <value>row limit spec if any</value>
        public RowLimitClause RowLimitClause { get; set; }

        /// <summary>
        ///     Returns the update specification.
        /// </summary>
        /// <value>update spec if defined</value>
        public UpdateClause UpdateClause { get; set; }

        /// <summary>
        ///     Returns annotations.
        /// </summary>
        /// <value>annotations</value>
        public IList<AnnotationPart> Annotations { get; set; }

        /// <summary>
        ///     Match-recognize clause.
        /// </summary>
        /// <value>clause</value>
        public MatchRecognizeClause MatchRecognizeClause { get; set; }

        /// <summary>
        ///     Returns create-index clause.
        /// </summary>
        /// <value>clause</value>
        public CreateIndexClause CreateIndex { get; set; }

        /// <summary>
        ///     Returns the create-schema clause.
        /// </summary>
        /// <value>clause</value>
        public CreateSchemaClause CreateSchema { get; set; }

        /// <summary>
        ///     Returns the create-context clause.
        /// </summary>
        /// <value>clause</value>
        public CreateContextClause CreateContext { get; set; }

        /// <summary>
        ///     Returns the for-clause.
        /// </summary>
        /// <value>for-clause</value>
        public ForClause ForClause { get; set; }

        /// <summary>
        ///     Returns the expression declarations, if any.
        /// </summary>
        /// <value>expression declarations</value>
        public IList<ExpressionDeclaration> ExpressionDeclarations { get; set; }

        /// <summary>
        ///     Returns the context name if context dimensions apply to statement.
        /// </summary>
        /// <value>context name</value>
        public string ContextName { get; set; }

        /// <summary>
        ///     Returns the scripts defined.
        /// </summary>
        /// <value>scripts</value>
        public IList<ScriptExpression> ScriptExpressions { get; set; }

        /// <summary>
        ///     Returns the inlined-classes provided as part of the EPL statement
        /// </summary>
        /// <value>inlined-classes</value>
        public IList<ClassProvidedExpression> ClassProvidedExpressions { get; set; }

        /// <summary>
        ///     Returns the "create dataflow" part, if present.
        /// </summary>
        /// <value>create dataflow clause</value>
        public CreateDataFlowClause CreateDataFlow { get; set; }

        /// <summary>
        ///     Returns the internal expression id assigned for tools to identify the expression.
        /// </summary>
        /// <value>object name</value>
        public string TreeObjectName { get; set; }

        /// <summary>
        ///     Returns the create-expression clause, if any
        /// </summary>
        /// <value>clause</value>
        public CreateExpressionClause CreateExpression { get; set; }

        /// <summary>
        ///     Returns the create-class clause or null if not present.
        /// </summary>
        /// <value>create-class clause</value>
        public CreateClassClause CreateClass { get; set; }

        /// <summary>
        ///     Returns fire-and-forget (on-demand) query information for FAF select, insert, update and delete.
        /// </summary>
        /// <value>fire and forget query information</value>
        public FireAndForgetClause FireAndForgetClause { get; set; }

        /// <summary>
        ///     Returns the into-table clause, or null if none found.
        /// </summary>
        /// <value>into-table clause</value>
        public IntoTableClause IntoTableClause { get; set; }

        /// <summary>
        ///     Returns the create-table clause if present or null if not present
        /// </summary>
        /// <value>create-table clause</value>
        public CreateTableClause CreateTable { get; set; }

        /// <summary>
        ///     Returns the create-window clause for creating named windows, or null if this statement does not
        ///     create a named window.
        /// </summary>
        /// <value>named window creation clause</value>
        public CreateWindowClause CreateWindow { get; set; }

        /// <summary>
        ///     Returns the on-delete clause for deleting from named windows, or null if this statement
        ///     does not delete from a named window
        /// </summary>
        /// <value>on delete clause</value>
        public OnClause OnExpr { get; set; }

        /// <summary>
        ///     Returns the create-variable clause if this is a statement creating a variable, or null if not.
        /// </summary>
        /// <value>create-variable clause</value>
        public CreateVariableClause CreateVariable { get; set; }

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
            this.SelectClause = selectClause;
            return this;
        }

        /// <summary>
        ///     Specify a from-clause.
        /// </summary>
        /// <param name="fromClause">specifies the from-clause, the from-clause cannot be null and must be set</param>
        /// <returns>model</returns>
        public EPStatementObjectModel WithFromClause(FromClause fromClause)
        {
            this.FromClause = fromClause;
            return this;
        }

        /// <summary>
        ///     Specify a where-clause.
        /// </summary>
        /// <param name="whereClause">specifies the where-clause, which is optional and can be null</param>
        /// <returns>model</returns>
        public EPStatementObjectModel WithWhereClause(Expression whereClause)
        {
            this.WhereClause = whereClause;
            return this;
        }

        /// <summary>
        ///     Specify a group-by-clause.
        /// </summary>
        /// <param name="groupByClause">specifies the group-by-clause, which is optional and can be null</param>
        /// <returns>model</returns>
        public EPStatementObjectModel WithGroupByClause(GroupByClause groupByClause)
        {
            this.GroupByClause = groupByClause;
            return this;
        }

        /// <summary>
        ///     Specify a having-clause.
        /// </summary>
        /// <param name="havingClause">specifies the having-clause, which is optional and can be null</param>
        /// <returns>model</returns>
        public EPStatementObjectModel WithHavingClause(Expression havingClause)
        {
            this.HavingClause = havingClause;
            return this;
        }

        /// <summary>
        ///     Specify an order-by-clause.
        /// </summary>
        /// <param name="orderByClause">specifies the order-by-clause, which is optional and can be null</param>
        /// <returns>model</returns>
        public EPStatementObjectModel WithOrderByClause(OrderByClause orderByClause)
        {
            this.OrderByClause = orderByClause;
            return this;
        }

        /// <summary>
        ///     Specify an output-rate-limiting-clause.
        /// </summary>
        /// <param name="outputLimitClause">specifies the output-rate-limiting-clause, which is optional and can be null</param>
        /// <returns>model</returns>
        public EPStatementObjectModel WithOutputLimitClause(OutputLimitClause outputLimitClause)
        {
            this.OutputLimitClause = outputLimitClause;
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
            AnnotationPart.ToEPL(writer, Annotations, formatter);
            ExpressionDeclaration.ToEPL(writer, ExpressionDeclarations, formatter);
            ScriptExpression.ToEPL(writer, ScriptExpressions, formatter);
            ClassProvidedExpression.ToEPL(writer, ClassProvidedExpressions, formatter);

            if (ContextName != null) {
                formatter.BeginContext(writer);
                writer.Write("context ");
                writer.Write(ContextName);
            }

            if (CreateIndex != null) {
                formatter.BeginCreateIndex(writer);
                CreateIndex.ToEPL(writer);
                return;
            }

            if (CreateSchema != null) {
                formatter.BeginCreateSchema(writer);
                CreateSchema.ToEPL(writer);
                return;
            }

            if (CreateExpression != null) {
                formatter.BeginCreateExpression(writer);
                CreateExpression.ToEPL(writer);
                return;
            }

            if (CreateClass != null) {
                formatter.BeginCreateExpression(writer);
                CreateClass.ToEPL(writer);
                return;
            }

            if (CreateContext != null) {
                formatter.BeginCreateContext(writer);
                CreateContext.ToEPL(writer, formatter);
                return;
            }

            if (CreateWindow != null) {
                formatter.BeginCreateWindow(writer);
                CreateWindow.ToEPL(writer);

                writer.Write(" as ");
                if (SelectClause == null || (SelectClause.SelectList.IsEmpty() && !CreateWindow.Columns.IsEmpty())) {
                    CreateWindow.ToEPLCreateTablePart(writer);
                }
                else {
                    SelectClause.ToEPL(writer, formatter, false, false);
                    if (CreateWindow.AsEventTypeName != null) {
                        writer.Write(" from ");
                        writer.Write(CreateWindow.AsEventTypeName);
                    }

                    CreateWindow.ToEPLInsertPart(writer);
                }

                return;
            }

            if (CreateVariable != null) {
                formatter.BeginCreateVariable(writer);
                CreateVariable.ToEPL(writer);
                return;
            }

            if (CreateTable != null) {
                formatter.BeginCreateTable(writer);
                CreateTable.ToEPL(writer);
                return;
            }

            if (CreateDataFlow != null) {
                formatter.BeginCreateDataFlow(writer);
                CreateDataFlow.ToEPL(writer, formatter);
                return;
            }

            var displayWhereClause = true;
            if (UpdateClause != null) {
                formatter.BeginUpdate(writer);
                UpdateClause.ToEPL(writer);
            }
            else if (OnExpr != null) {
                formatter.BeginOnTrigger(writer);
                writer.Write("on ");
                FromClause.Streams[0].ToEPL(writer, formatter);

                if (OnExpr is OnDeleteClause clause) {
                    formatter.BeginOnDelete(writer);
                    writer.Write("delete from ");
                    clause.ToEPL(writer);
                }
                else if (OnExpr is OnUpdateClause onUpdateClause) {
                    formatter.BeginOnUpdate(writer);
                    writer.Write("update ");
                    onUpdateClause.ToEPL(writer);
                }
                else if (OnExpr is OnSelectClause onSelect) {
                    if (InsertInto != null) {
                        InsertInto.ToEPL(writer, formatter, true);
                    }

                    SelectClause.ToEPL(writer, formatter, InsertInto == null, onSelect.IsDeleteAndSelect);
                    writer.Write(" from ");
                    onSelect.ToEPL(writer);
                }
                else if (OnExpr is OnSetClause onSet) {
                    onSet.ToEPL(writer, formatter);
                }
                else if (OnExpr is OnMergeClause merge) {
                    merge.ToEPL(writer, WhereClause, formatter);
                    displayWhereClause = false;
                }
                else {
                    var split = (OnInsertSplitStreamClause)OnExpr;
                    InsertInto.ToEPL(writer, formatter, true);
                    SelectClause.ToEPL(writer, formatter, false, false);
                    if (WhereClause != null) {
                        writer.Write(" where ");
                        WhereClause.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                    }

                    split.ToEPL(writer, formatter);
                    displayWhereClause = false;
                }
            }
            else {
                if (IntoTableClause != null) {
                    IntoTableClause.ToEPL(writer, formatter);
                }

                if (SelectClause == null) {
                    throw new IllegalStateException("Select-clause has not been defined");
                }

                if (FromClause == null) {
                    throw new IllegalStateException("From-clause has not been defined");
                }

                if (FireAndForgetClause is FireAndForgetUpdate update) {
                    writer.Write("update ");
                    FromClause.ToEPLOptions(writer, formatter, false);
                    writer.Write(" ");
                    UpdateClause.RenderEPLAssignments(writer, update.Assignments);
                }
                else if (FireAndForgetClause is FireAndForgetInsert insert) {
                    InsertInto.ToEPL(writer, formatter, true);
                    if (insert.IsUseValuesKeyword) {
                        insert.ToEPL(writer);
                    }
                    else {
                        SelectClause.ToEPL(writer, formatter, false, false);
                    }
                }
                else if (FireAndForgetClause is FireAndForgetDelete) {
                    writer.Write("delete ");
                    FromClause.ToEPLOptions(writer, formatter, true);
                }
                else {
                    if (InsertInto != null) {
                        InsertInto.ToEPL(writer, formatter, IntoTableClause == null);
                    }

                    SelectClause.ToEPL(writer, formatter, IntoTableClause == null && InsertInto == null, false);
                    FromClause.ToEPLOptions(writer, formatter, true);
                }
            }

            if (MatchRecognizeClause != null) {
                MatchRecognizeClause.ToEPL(writer);
            }

            if (WhereClause != null && displayWhereClause) {
                formatter.BeginWhere(writer);
                writer.Write("where ");
                WhereClause.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }

            if (GroupByClause != null) {
                formatter.BeginGroupBy(writer);
                writer.Write("group by ");
                GroupByClause.ToEPL(writer);
            }

            if (HavingClause != null) {
                formatter.BeginHaving(writer);
                writer.Write("having ");
                HavingClause.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
            }

            if (OutputLimitClause != null) {
                formatter.BeginOutput(writer);
                writer.Write("output ");
                OutputLimitClause.ToEPL(writer);
            }

            if (OrderByClause != null) {
                formatter.BeginOrderBy(writer);
                writer.Write("order by ");
                OrderByClause.ToEPL(writer);
            }

            if (RowLimitClause != null) {
                formatter.BeginLimit(writer);
                writer.Write("limit ");
                RowLimitClause.ToEPL(writer);
            }

            if (ForClause != null) {
                formatter.BeginFor(writer);
                ForClause.ToEPL(writer);
            }
        }
    }
} // end of namespace