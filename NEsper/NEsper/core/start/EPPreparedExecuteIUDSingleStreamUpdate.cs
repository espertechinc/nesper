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
using com.espertech.esper.epl.spec;
using com.espertech.esper.epl.table.upd;
using com.espertech.esper.epl.updatehelper;
using com.espertech.esper.events;
using com.espertech.esper.filter;

namespace com.espertech.esper.core.start
{
    /// <summary>
    /// Starts and provides the stop method for EPL statements.
    /// </summary>
    public class EPPreparedExecuteIUDSingleStreamUpdate : EPPreparedExecuteIUDSingleStream
    {
        public EPPreparedExecuteIUDSingleStreamUpdate(StatementSpecCompiled statementSpec, EPServicesContext services, StatementContext statementContext)
            : base(statementSpec, services, statementContext)
        {
        }
    
        public override EPPreparedExecuteIUDSingleStreamExec GetExecutor(FilterSpecCompiled filter, string aliasName)
        {
            var updateSpec = (FireAndForgetSpecUpdate) StatementSpec.FireAndForgetSpec;
    
            var assignmentTypeService = new StreamTypeServiceImpl(
                    new EventType[] {Processor.EventTypeResultSetProcessor, null, Processor.EventTypeResultSetProcessor},
                    new string[] {aliasName, "", EPStatementStartMethodOnTrigger.INITIAL_VALUE_STREAM_NAME},
                    new bool[] {true, true, true}, Services.EngineURI, true);
            assignmentTypeService.IsStreamZeroUnambigous = true;
            var evaluatorContextStmt = new ExprEvaluatorContextStatement(StatementContext, true);
            var validationContext = new ExprValidationContext(
                assignmentTypeService,
                StatementContext.EngineImportService,
                StatementContext.StatementExtensionServicesContext, null,
                StatementContext.SchedulingService,
                StatementContext.VariableService,
                StatementContext.TableService,
                evaluatorContextStmt,
                StatementContext.EventAdapterService, 
                StatementContext.StatementName,
                StatementContext.StatementId, 
                StatementContext.Annotations, 
                StatementContext.ContextDescriptor,
                StatementContext.ScriptingService, false,
                false, true, false, null, false);
    
            // validate update expressions
            try {
                foreach (var assignment in updateSpec.Assignments)
                {
                    var validated = ExprNodeUtility.GetValidatedSubtree(ExprNodeOrigin.UPDATEASSIGN, assignment.Expression, validationContext);
                    assignment.Expression = validated;
                    EPStatementStartMethodHelperValidate.ValidateNoAggregations(validated, "Aggregation functions may not be used within an update-clause");
                }
            }
            catch (ExprValidationException e) {
                throw new EPException(e.Message, e);
            }
    
            // make updater
            EventBeanUpdateHelper updateHelper;
            TableUpdateStrategy tableUpdateStrategy = null;
            try {
    
                var copyOnWrite = !(Processor is FireAndForgetProcessorTable);
                updateHelper = EventBeanUpdateHelperFactory.Make(Processor.NamedWindowOrTableName,
                        (EventTypeSPI) Processor.EventTypeResultSetProcessor, updateSpec.Assignments, aliasName, null, copyOnWrite);
    
                if (Processor is FireAndForgetProcessorTable) {
                    var tableProcessor = (FireAndForgetProcessorTable) Processor;
                    tableUpdateStrategy = Services.TableService.GetTableUpdateStrategy(tableProcessor.TableMetadata, updateHelper, false);
                    copyOnWrite = false;
                }
            }
            catch (ExprValidationException e) {
                throw new EPException(e.Message, e);
            }
    
            return new EPPreparedExecuteIUDSingleStreamExecUpdate(filter, StatementSpec.FilterRootNode, StatementSpec.Annotations, updateHelper, tableUpdateStrategy, StatementSpec.TableNodes, Services);
        }
    }
}
