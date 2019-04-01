///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Linq;

using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.spec;
using com.espertech.esper.filter;
using com.espertech.esper.util;

namespace com.espertech.esper.core.start
{
    /// <summary>Starts and provides the stop method for EPL statements.</summary>
    public class EPPreparedExecuteIUDInsertInto : EPPreparedExecuteIUDSingleStream
    {
        public EPPreparedExecuteIUDInsertInto(StatementSpecCompiled statementSpec, EPServicesContext services, StatementContext statementContext)
            : base(AssociatedFromClause(statementSpec), services, statementContext)
        {
        }
    
        private static StatementSpecCompiled AssociatedFromClause(StatementSpecCompiled statementSpec) {
            if (statementSpec.FilterRootNode != null ||
                    statementSpec.StreamSpecs.Length > 0 ||
                    statementSpec.HavingExprRootNode != null ||
                    statementSpec.OutputLimitSpec != null ||
                    statementSpec.ForClauseSpec != null ||
                    statementSpec.MatchRecognizeSpec != null ||
                    statementSpec.OrderByList.Length > 0 ||
                    statementSpec.RowLimitSpec != null) {
                throw new ExprValidationException("Insert-into fire-and-forget query can only consist of an insert-into clause and a select-clause");
            }
    
            var namedWindowName = statementSpec.InsertIntoDesc.EventTypeName;
            var namedWindowStream = new NamedWindowConsumerStreamSpec(namedWindowName, null, new ViewSpec[0], Collections.GetEmptyList<ExprNode>(),
                    StreamSpecOptions.DEFAULT, null);
            statementSpec.StreamSpecs = new StreamSpecCompiled[]{namedWindowStream};
            return statementSpec;
        }
    
        public override EPPreparedExecuteIUDSingleStreamExec GetExecutor(QueryGraph queryGraph, string aliasName)
        {
            var statementSpec = base.StatementSpec;
            var statementContext = base.StatementContext;
            var selectNoWildcard = NamedWindowOnMergeHelper.CompileSelectNoWildcard(UuidGenerator.Generate(), statementSpec.SelectClauseSpec.SelectExprList);
            var streamTypeService = new StreamTypeServiceImpl(statementContext.EngineURI, true);
            var exprEvaluatorContextStatement = new ExprEvaluatorContextStatement(statementContext, true);
    
            // assign names
            var validationContext = new ExprValidationContext(
                statementContext.Container,
                streamTypeService, 
                statementContext.EngineImportService,
                statementContext.StatementExtensionServicesContext, null, 
                statementContext.TimeProvider, 
                statementContext.VariableService, 
                statementContext.TableService, 
                exprEvaluatorContextStatement,
                statementContext.EventAdapterService, 
                statementContext.StatementName, 
                statementContext.StatementId, 
                statementContext.Annotations, 
                statementContext.ContextDescriptor,
                statementContext.ScriptingService,
                false, false, true, false, null, false);

            var processor = base.Processor;
    
            // determine whether column names are provided
            // if the "values" keyword was used, allow sequential automatic name assignment
            string[] assignedSequentialNames = null;
            if (statementSpec.InsertIntoDesc.ColumnNames.IsEmpty()) {
                var insert = (FireAndForgetSpecInsert) statementSpec.FireAndForgetSpec;
                if (insert.IsUseValuesKeyword) {
                    assignedSequentialNames = processor.EventTypePublic.PropertyNames;
                }
            }
    
            var count = -1;
            foreach (var compiled in statementSpec.SelectClauseSpec.SelectExprList) {
                count++;
                if (compiled is SelectClauseExprCompiledSpec) {
                    var expr = (SelectClauseExprCompiledSpec) compiled;
                    var validatedExpression = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.SELECT, expr.SelectExpression, validationContext);
                    expr.SelectExpression = validatedExpression;
                    if (expr.AssignedName == null) {
                        if (expr.ProvidedName == null) {
                            if (assignedSequentialNames != null && count < assignedSequentialNames.Length) {
                                expr.AssignedName = assignedSequentialNames[count];
                            } else {
                                expr.AssignedName = ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(expr.SelectExpression);
                            }
                        } else {
                            expr.AssignedName = expr.ProvidedName;
                        }
                    }
                }
            }
    
            var optionalInsertIntoEventType = processor.EventTypeResultSetProcessor;
            var selectExprEventTypeRegistry = new SelectExprEventTypeRegistry(statementContext.StatementName, statementContext.StatementEventTypeRef);
            var insertHelper = SelectExprProcessorFactory.GetProcessor(
                statementContext.Container,
                Collections.SingletonList(0),
                selectNoWildcard.ToArray(), false, 
                statementSpec.InsertIntoDesc, optionalInsertIntoEventType, null, streamTypeService,
                statementContext.EventAdapterService,
                statementContext.StatementResultService, 
                statementContext.ValueAddEventService, 
                selectExprEventTypeRegistry,
                statementContext.EngineImportService, 
                exprEvaluatorContextStatement, 
                statementContext.VariableService,
                statementContext.ScriptingService,
                statementContext.TableService, 
                statementContext.TimeProvider, 
                statementContext.EngineURI,
                statementContext.StatementId,
                statementContext.StatementName,
                statementContext.Annotations,
                statementContext.ContextDescriptor,
                statementContext.ConfigSnapshot, null,
                statementContext.NamedWindowMgmtService,
                null, null,
                statementContext.StatementExtensionServicesContext);
    
            return new EPPreparedExecuteIUDSingleStreamExecInsert(exprEvaluatorContextStatement, insertHelper, statementSpec.TableNodes, base.Services);
        }
    
    }
} // end of namespace
