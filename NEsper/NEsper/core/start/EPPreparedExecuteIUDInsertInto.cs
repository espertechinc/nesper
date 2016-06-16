///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.named;
using com.espertech.esper.epl.spec;
using com.espertech.esper.filter;
using com.espertech.esper.util;

namespace com.espertech.esper.core.start
{
    /// <summary>
    /// Starts and provides the stop method for EPL statements.
    /// </summary>
    public class EPPreparedExecuteIUDInsertInto : EPPreparedExecuteIUDSingleStream
    {
        public EPPreparedExecuteIUDInsertInto(StatementSpecCompiled statementSpec, EPServicesContext services, StatementContext statementContext)
            : base(AssociatedFromClause(statementSpec), services, statementContext)
        {
        }
    
        public override EPPreparedExecuteIUDSingleStreamExec GetExecutor(FilterSpecCompiled filter, string aliasName)
        {
            var selectNoWildcard = NamedWindowOnMergeHelper.CompileSelectNoWildcard(UuidGenerator.Generate(), StatementSpec.SelectClauseSpec.SelectExprList);
    
            StreamTypeService streamTypeService = new StreamTypeServiceImpl(StatementContext.EngineURI, true);
            var exprEvaluatorContextStatement = new ExprEvaluatorContextStatement(StatementContext, true);
    
            // assign names
            var validationContext = new ExprValidationContext(
                streamTypeService, StatementContext.MethodResolutionService,
                null, StatementContext.TimeProvider, 
                StatementContext.VariableService, 
                StatementContext.TableService,
                exprEvaluatorContextStatement,
                StatementContext.EventAdapterService,
                StatementContext.StatementName, 
                StatementContext.StatementId,
                StatementContext.Annotations,
                StatementContext.ContextDescriptor, 
                StatementContext.ScriptingService,
                false, false, true, false, null, false);
    
            // determine whether column names are provided
            // if the "values" keyword was used, allow sequential automatic name assignment
            string[] assignedSequentialNames = null;
            if (StatementSpec.InsertIntoDesc.ColumnNames.IsEmpty()) {
                var insert = (FireAndForgetSpecInsert) StatementSpec.FireAndForgetSpec;
                if (insert.IsUseValuesKeyword) {
                    assignedSequentialNames = Processor.EventTypePublic.PropertyNames;
                }
            }
    
            var count = -1;
            foreach (var compiled in StatementSpec.SelectClauseSpec.SelectExprList) {
                count++;
                if (compiled is SelectClauseExprCompiledSpec) {
                    var expr = (SelectClauseExprCompiledSpec) compiled;
                    ExprNode validatedExpression = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.SELECT, expr.SelectExpression, validationContext);
                    expr.SelectExpression = validatedExpression;
                    if (expr.AssignedName == null) {
                        if (expr.ProvidedName == null) {
                            if (assignedSequentialNames != null && count < assignedSequentialNames.Length) {
                                expr.AssignedName = assignedSequentialNames[count];
                            }
                            else {
                                expr.AssignedName = ExprNodeUtility.ToExpressionStringMinPrecedenceSafe(expr.SelectExpression);
                            }
                        }
                        else {
                            expr.AssignedName = expr.ProvidedName;
                        }
                    }
                }
            }
    
            EventType optionalInsertIntoEventType = Processor.EventTypeResultSetProcessor;
            var selectExprEventTypeRegistry = new SelectExprEventTypeRegistry(StatementContext.StatementName, StatementContext.StatementEventTypeRef);
            var insertHelper = SelectExprProcessorFactory.GetProcessor(
                Collections.SingletonList(0),
                selectNoWildcard.ToArray(), false, 
                StatementSpec.InsertIntoDesc, optionalInsertIntoEventType, null, streamTypeService,
                StatementContext.EventAdapterService, 
                StatementContext.StatementResultService, 
                StatementContext.ValueAddEventService, selectExprEventTypeRegistry,
                StatementContext.MethodResolutionService, exprEvaluatorContextStatement, 
                StatementContext.VariableService,
                StatementContext.ScriptingService,
                StatementContext.TableService, 
                StatementContext.TimeProvider, 
                StatementContext.EngineURI, 
                StatementContext.StatementId, 
                StatementContext.StatementName, 
                StatementContext.Annotations, 
                StatementContext.ContextDescriptor,
                StatementContext.ConfigSnapshot, null, 
                StatementContext.NamedWindowMgmtService, null, null);
    
            return new EPPreparedExecuteIUDSingleStreamExecInsert(exprEvaluatorContextStatement, insertHelper, StatementSpec.TableNodes, Services);
        }
    
        private static StatementSpecCompiled AssociatedFromClause(StatementSpecCompiled statementSpec)
        {
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
            var namedWindowStream = new NamedWindowConsumerStreamSpec(namedWindowName, null, new ViewSpec[0], Collections.GetEmptyList<ExprNode>(), new StreamSpecOptions(), null);
            statementSpec.StreamSpecs = new StreamSpecCompiled[] {namedWindowStream};
            return statementSpec;
        }
    
    }
}
