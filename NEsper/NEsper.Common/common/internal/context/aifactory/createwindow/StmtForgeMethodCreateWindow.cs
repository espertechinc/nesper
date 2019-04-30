///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.context.aifactory.@select;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.namedwindow.core;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.resultset.core;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.virtualdw;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.aifactory.createwindow
{
    public class StmtForgeMethodCreateWindow : StmtForgeMethod
    {
        private readonly StatementBaseInfo @base;

        public StmtForgeMethodCreateWindow(StatementBaseInfo @base)
        {
            this.@base = @base;
        }

        public StmtForgeMethodResult Make(
            string packageName,
            string classPostfix,
            StatementCompileTimeServices services)
        {
            try {
                return Build(packageName, classPostfix, services);
            }
            catch (ExprValidationException ex) {
                throw;
            }
            catch (Exception t) {
                throw new ExprValidationException(
                    "Unexpected exception creating named window '" + @base.StatementSpec.Raw.CreateWindowDesc.WindowName + "': " + t.Message, t);
            }
        }

        private StmtForgeMethodResult Build(
            string packageName,
            string classPostfix,
            StatementCompileTimeServices services)
        {
            var compileResult = CreateWindowUtil.HandleCreateWindow(@base, services);
            var namedWindowType = compileResult.FilterSpecCompiled.FilterForEventType;

            // view must be non-empty list
            var createWindowDesc = @base.StatementSpec.Raw.CreateWindowDesc;
            if (createWindowDesc.ViewSpecs.IsEmpty()) {
                throw new ExprValidationException(NamedWindowManagementServiceConstants.ERROR_MSG_DATAWINDOWS);
            }

            if (services.NamedWindowCompileTimeResolver.Resolve(createWindowDesc.WindowName) != null) {
                throw new ExprValidationException("Named window named '" + createWindowDesc.WindowName + "' has already been declared");
            }

            // build forge
            var activator = new ViewableActivatorFilterForge(compileResult.FilterSpecCompiled, false, 0, false, -1);

            var viewSpecs = createWindowDesc.ViewSpecs;
            var viewArgs = new ViewFactoryForgeArgs(
                0, false, -1, createWindowDesc.StreamSpecOptions, createWindowDesc.WindowName, @base.StatementRawInfo, services);
            var viewForges = ViewFactoryForgeUtil.CreateForges(viewSpecs.ToArray(), viewArgs, namedWindowType);
            IList<ScheduleHandleCallbackProvider> schedules = new List<ScheduleHandleCallbackProvider>();
            ViewFactoryForgeUtil.DetermineViewSchedules(viewForges, schedules);
            VerifyDataWindowViewFactoryChain(viewForges);
            var optionalUniqueKeyProps =
                StreamJoinAnalysisResultCompileTime.GetUniqueCandidateProperties(viewForges, @base.StatementSpec.Annotations);
            var uniqueKeyProArray = optionalUniqueKeyProps == null ? null : optionalUniqueKeyProps.ToArray();

            NamedWindowMetaData insertFromNamedWindow = null;
            ExprNode insertFromFilter = null;
            if (createWindowDesc.IsInsert || createWindowDesc.InsertFilter != null) {
                var name = createWindowDesc.AsEventTypeName;
                insertFromNamedWindow = services.NamedWindowCompileTimeResolver.Resolve(name);
                if (insertFromNamedWindow == null) {
                    throw new ExprValidationException(
                        "A named window by name '" + name + "' could not be located, the insert-keyword requires an existing named window");
                }

                insertFromFilter = createWindowDesc.InsertFilter;

                if (insertFromFilter != null) {
                    var checkMinimal = ExprNodeUtilityValidate.IsMinimalExpression(insertFromFilter);
                    if (checkMinimal != null) {
                        throw new ExprValidationException("Create window where-clause may not have " + checkMinimal);
                    }

                    StreamTypeService streamTypeService = new StreamTypeServiceImpl(insertFromNamedWindow.EventType, name, true);
                    var validationContext = new ExprValidationContextBuilder(streamTypeService, @base.StatementRawInfo, services).Build();
                    insertFromFilter = ExprNodeUtilityValidate.GetValidatedSubtree(
                        ExprNodeOrigin.CREATEWINDOWFILTER, insertFromFilter, validationContext);
                }
            }

            // handle output format
            var defaultSelectAllSpec = new StatementSpecCompiled();
            defaultSelectAllSpec.SelectClauseCompiled.WithSelectExprList(new SelectClauseElementWildcard());
            defaultSelectAllSpec.Raw.SelectStreamDirEnum = SelectClauseStreamSelectorEnum.RSTREAM_ISTREAM_BOTH;
            StreamTypeService typeService = new StreamTypeServiceImpl(
                new[] {namedWindowType}, new[] {createWindowDesc.WindowName}, new[] {true}, false, false);
            var resultSetProcessor = ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                new ResultSetSpec(defaultSelectAllSpec),
                typeService, null, new bool[1], false, @base.ContextPropertyRegistry, false, false, @base.StatementRawInfo, services);
            var classNameRSP = CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(ResultSetProcessorFactoryProvider), classPostfix);
            var selectSubscriberDescriptor = resultSetProcessor.SelectSubscriberDescriptor;

            var forge = new StatementAgentInstanceFactoryCreateNWForge(
                activator, createWindowDesc.WindowName, viewForges,
                insertFromNamedWindow, insertFromFilter, compileResult.AsEventType, classNameRSP);

            // add named window
            var isBatchingDataWindow = DetermineBatchingDataWindow(viewForges);
            var virtualDataWindow = viewForges[0] is VirtualDWViewFactoryForge;
            var isEnableIndexShare = virtualDataWindow || HintEnum.ENABLE_WINDOW_SUBQUERY_INDEXSHARE.GetHint(@base.StatementSpec.Annotations) != null;
            var metaData = new NamedWindowMetaData(
                namedWindowType, @base.ModuleName, @base.ContextName, uniqueKeyProArray, isBatchingDataWindow, isEnableIndexShare,
                compileResult.AsEventType, virtualDataWindow);
            services.NamedWindowCompileTimeRegistry.NewNamedWindow(metaData);

            // build forge list
            IList<StmtClassForgable> forgables = new List<StmtClassForgable>(2);

            var statementFieldsClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementFields), classPostfix);
            var packageScope = new CodegenNamespaceScope(packageName, statementFieldsClassName, services.IsInstrumented);
            forgables.Add(new StmtClassForgableRSPFactoryProvider(classNameRSP, resultSetProcessor, packageScope, @base.StatementRawInfo));

            var aiFactoryProviderClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementAIFactoryProvider), classPostfix);
            var aiFactoryForgable = new StmtClassForgableAIFactoryProviderCreateNW(
                aiFactoryProviderClassName, packageScope, forge, createWindowDesc.WindowName);
            forgables.Add(aiFactoryForgable);

            var statementProviderClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementProvider), classPostfix);
            var informationals = StatementInformationalsUtil.GetInformationals(
                @base, Collections.SingletonList(compileResult.FilterSpecCompiled), schedules,
                new EmptyList<NamedWindowConsumerStreamSpec>(), true, selectSubscriberDescriptor, packageScope, services);
            forgables.Add(new StmtClassForgableStmtProvider(aiFactoryProviderClassName, statementProviderClassName, informationals, packageScope));
            forgables.Add(new StmtClassForgableStmtFields(statementFieldsClassName, packageScope, 1));

            return new StmtForgeMethodResult(
                forgables, Collections.SingletonList(compileResult.FilterSpecCompiled), schedules,
                new EmptyList<NamedWindowConsumerStreamSpec>(),
                new EmptyList<FilterSpecParamExprNodeForge>());
        }

        private static bool DetermineBatchingDataWindow(IList<ViewFactoryForge> forges)
        {
            foreach (var forge in forges) {
                if (forge is DataWindowBatchingViewForge) {
                    return true;
                }
            }

            return false;
        }

        private void VerifyDataWindowViewFactoryChain(IList<ViewFactoryForge> forges)
        {
            var hasDataWindow = new AtomicBoolean();
            ViewForgeVisitor visitor = new ProxyViewForgeVisitor() {
                ProcVisit = forge => {
                    if (forge is DataWindowViewForge) {
                        hasDataWindow.Set(true);
                    }
                }
            };

            foreach (var forge in forges) {
                forge.Accept(visitor);
            }

            if (!hasDataWindow.Get()) {
                throw new ExprValidationException(NamedWindowManagementServiceConstants.ERROR_MSG_DATAWINDOWS);
            }
        }
    }
} // end of namespace