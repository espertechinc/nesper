///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.client;
using com.espertech.esper.core.service;
using com.espertech.esper.epl.core;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.epl.join.plan;
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.upd;
using com.espertech.esper.epl.updatehelper;
using com.espertech.esper.events;
using com.espertech.esper.filter;

namespace com.espertech.esper.core.start
{
    /// <summary>Starts and provides the stop method for EPL statements.</summary>
    public class EPPreparedExecuteIUDSingleStreamUpdate : EPPreparedExecuteIUDSingleStream
    {
        public EPPreparedExecuteIUDSingleStreamUpdate(StatementSpecCompiled statementSpec, EPServicesContext services, StatementContext statementContext)
            : base(statementSpec, services, statementContext)
        {
        }
    
        public override EPPreparedExecuteIUDSingleStreamExec GetExecutor(QueryGraph queryGraph, string aliasName)
        {
            var services = base.Services;
            var processor = base.Processor;
            var statementContext = base.StatementContext;
            var statementSpec = base.StatementSpec;
            var updateSpec = (FireAndForgetSpecUpdate) statementSpec.FireAndForgetSpec;
    
            var assignmentTypeService = new StreamTypeServiceImpl(
                    new EventType[]{processor.EventTypeResultSetProcessor, null, processor.EventTypeResultSetProcessor},
                    new string[]{aliasName, "", EPStatementStartMethodOnTrigger.INITIAL_VALUE_STREAM_NAME},
                    new bool[]{true, true, true}, services.EngineURI, true);
            assignmentTypeService.IsStreamZeroUnambigous = true;
            var evaluatorContextStmt = new ExprEvaluatorContextStatement(statementContext, true);
            var validationContext = new ExprValidationContext(
                statementContext.Container,
                assignmentTypeService,
                statementContext.EngineImportService,
                statementContext.StatementExtensionServicesContext, null,
                statementContext.SchedulingService,
                statementContext.VariableService,
                statementContext.TableService,
                evaluatorContextStmt,
                statementContext.EventAdapterService,
                statementContext.StatementName,
                statementContext.StatementId,
                statementContext.Annotations, 
                statementContext.ContextDescriptor,
                statementContext.ScriptingService,
                false, false, true, false, null, false);
    
            // validate update expressions
            try {
                foreach (OnTriggerSetAssignment assignment in updateSpec.Assignments) {
                    ExprNode validated = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.UPDATEASSIGN, assignment.Expression, validationContext);
                    assignment.Expression = validated;
                    EPStatementStartMethodHelperValidate.ValidateNoAggregations(validated, "Aggregation functions may not be used within an update-clause");
                }
            } catch (ExprValidationException e) {
                throw new EPException(e.Message, e);
            }
    
            // make updater
            EventBeanUpdateHelper updateHelper;
            TableUpdateStrategy tableUpdateStrategy = null;
            try {
    
                bool copyOnWrite = !(processor is FireAndForgetProcessorTable);
                updateHelper = EventBeanUpdateHelperFactory.Make(processor.NamedWindowOrTableName,
                        (EventTypeSPI) processor.EventTypeResultSetProcessor, updateSpec.Assignments, aliasName, null, copyOnWrite, statementContext.StatementName, services.EngineURI, services.EventAdapterService);
    
                if (processor is FireAndForgetProcessorTable) {
                    FireAndForgetProcessorTable tableProcessor = (FireAndForgetProcessorTable) processor;
                    tableUpdateStrategy = services.TableService.GetTableUpdateStrategy(tableProcessor.TableMetadata, updateHelper, false);
                }
            } catch (ExprValidationException e) {
                throw new EPException(e.Message, e);
            }
    
            return new EPPreparedExecuteIUDSingleStreamExecUpdate(queryGraph, statementSpec.FilterRootNode, statementSpec.Annotations, updateHelper, tableUpdateStrategy, statementSpec.TableNodes, services);
        }
    }
} // end of namespace
