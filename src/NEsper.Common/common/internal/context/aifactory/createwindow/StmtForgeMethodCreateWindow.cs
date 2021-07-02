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
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.activator;
using com.espertech.esper.common.@internal.context.aifactory.select;
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
        private readonly StatementBaseInfo _base;

        public StmtForgeMethodCreateWindow(StatementBaseInfo @base)
        {
            _base = @base;
        }

        public StmtForgeMethodResult Make(
            string @namespace,
            string classPostfix,
            StatementCompileTimeServices services)
        {
            try {
                return Build(@namespace, classPostfix, services);
            }
            catch (ExprValidationException) {
                throw;
            }
            catch (Exception t) {
                throw new ExprValidationException(
                    "Unexpected exception creating named window '" +
                    _base.StatementSpec.Raw.CreateWindowDesc.WindowName +
                    "': " +
                    t.Message,
                    t);
            }
        }

        private StmtForgeMethodResult Build(
            string @namespace,
            string classPostfix,
            StatementCompileTimeServices services)
        {
            var additionalForgeables = new List<StmtClassForgeableFactory>();
            var compileResult = CreateWindowUtil.HandleCreateWindow(_base, services);
            additionalForgeables.AddRange(compileResult.AdditionalForgeables);
            var namedWindowType = compileResult.FilterSpecCompiled.FilterForEventType;

            // view must be non-empty list
            var createWindowDesc = _base.StatementSpec.Raw.CreateWindowDesc;
            if (createWindowDesc.ViewSpecs.IsEmpty()) {
                throw new ExprValidationException(NamedWindowManagementServiceConstants.ERROR_MSG_DATAWINDOWS);
            }

            if (services.NamedWindowCompileTimeResolver.Resolve(createWindowDesc.WindowName) != null) {
                throw new ExprValidationException(
                    "Named window named '" + createWindowDesc.WindowName + "' has already been declared");
            }

            // build forge
            var activator = new ViewableActivatorFilterForge(compileResult.FilterSpecCompiled, false, 0, false, -1);

            var viewSpecs = createWindowDesc.ViewSpecs;
            var viewArgs = new ViewFactoryForgeArgs(
                0,
                false,
                -1,
                createWindowDesc.StreamSpecOptions,
                createWindowDesc.WindowName,
                _base.StatementRawInfo,
                services);
            ViewFactoryForgeDesc viewForgeDesc = ViewFactoryForgeUtil.CreateForges(viewSpecs.ToArray(), viewArgs, namedWindowType);
            additionalForgeables.AddRange(viewForgeDesc.MultikeyForges);

            IList<ViewFactoryForge> viewForges = viewForgeDesc.Forges;
            IList<ScheduleHandleCallbackProvider> schedules = new List<ScheduleHandleCallbackProvider>();
            ViewFactoryForgeUtil.DetermineViewSchedules(viewForges, schedules);
            VerifyDataWindowViewFactoryChain(viewForges);
            var optionalUniqueKeyProps =
                StreamJoinAnalysisResultCompileTime.GetUniqueCandidateProperties(
                    viewForges,
                    _base.StatementSpec.Annotations);
            var uniqueKeyProArray = optionalUniqueKeyProps?.ToArray();

            NamedWindowMetaData insertFromNamedWindow = null;
            ExprNode insertFromFilter = null;
            if (createWindowDesc.IsInsert || createWindowDesc.InsertFilter != null) {
                var name = createWindowDesc.AsEventTypeName;
                insertFromNamedWindow = services.NamedWindowCompileTimeResolver.Resolve(name);
                if (insertFromNamedWindow == null) {
                    throw new ExprValidationException(
                        "A named window by name '" +
                        name +
                        "' could not be located, the insert-keyword requires an existing named window");
                }

                insertFromFilter = createWindowDesc.InsertFilter;

                if (insertFromFilter != null) {
                    var checkMinimal = ExprNodeUtilityValidate.IsMinimalExpression(insertFromFilter);
                    if (checkMinimal != null) {
                        throw new ExprValidationException("Create window where-clause may not have " + checkMinimal);
                    }

                    StreamTypeService streamTypeService = new StreamTypeServiceImpl(
                        insertFromNamedWindow.EventType,
                        name,
                        true);
                    var validationContext = new ExprValidationContextBuilder(
                        streamTypeService,
                        _base.StatementRawInfo,
                        services).Build();
                    insertFromFilter = ExprNodeUtilityValidate.GetValidatedSubtree(
                        ExprNodeOrigin.CREATEWINDOWFILTER,
                        insertFromFilter,
                        validationContext);
                }
            }

            // handle output format
            var defaultSelectAllSpec = new StatementSpecCompiled();
            defaultSelectAllSpec.SelectClauseCompiled.WithSelectExprList(new SelectClauseElementWildcard());
            defaultSelectAllSpec.Raw.SelectStreamDirEnum = SelectClauseStreamSelectorEnum.RSTREAM_ISTREAM_BOTH;
            StreamTypeService typeService = new StreamTypeServiceImpl(
                new[] {namedWindowType},
                new[] {createWindowDesc.WindowName},
                new[] {true},
                false,
                false);
            var resultSetProcessor = ResultSetProcessorFactoryFactory.GetProcessorPrototype(
                new ResultSetSpec(defaultSelectAllSpec),
                typeService,
                null,
                new bool[1],
                false,
                _base.ContextPropertyRegistry,
                false,
                false,
                _base.StatementRawInfo,
                services);
            var classNameRSP = CodeGenerationIDGenerator.GenerateClassNameSimple(
                typeof(ResultSetProcessorFactoryProvider),
                classPostfix);
            var selectSubscriberDescriptor = resultSetProcessor.SelectSubscriberDescriptor;

            var forge = new StatementAgentInstanceFactoryCreateNWForge(
                activator,
                createWindowDesc.WindowName,
                viewForges,
                insertFromNamedWindow,
                insertFromFilter,
                compileResult.AsEventType,
                classNameRSP);

            // add named window
            var isBatchingDataWindow = DetermineBatchingDataWindow(viewForges);
            var virtualDataWindow = viewForges[0] is VirtualDWViewFactoryForge;
            var isEnableIndexShare = virtualDataWindow ||
                                     HintEnum.ENABLE_WINDOW_SUBQUERY_INDEXSHARE.GetHint(
                                         _base.StatementSpec.Annotations) !=
                                     null;
            var metaData = new NamedWindowMetaData(
                namedWindowType,
                _base.ModuleName,
                _base.ContextName,
                uniqueKeyProArray,
                isBatchingDataWindow,
                isEnableIndexShare,
                compileResult.AsEventType,
                virtualDataWindow);
            services.NamedWindowCompileTimeRegistry.NewNamedWindow(metaData);

            // build forge list

            var statementFieldsClassName =
                CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementFields), classPostfix);
            var namespaceScope = new CodegenNamespaceScope(
                @namespace,
                statementFieldsClassName,
                services.IsInstrumented);

            var forgeables = additionalForgeables
                .Select(additional => additional.Make(namespaceScope, classPostfix))
                .ToList();

            forgeables.Add(new StmtClassForgeableRSPFactoryProvider(classNameRSP, resultSetProcessor, namespaceScope, _base.StatementRawInfo));

            var aiFactoryProviderClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(
                typeof(StatementAIFactoryProvider),
                classPostfix);
            var aiFactoryForgeable = new StmtClassForgeableAIFactoryProviderCreateNW(
                aiFactoryProviderClassName,
                namespaceScope,
                forge,
                createWindowDesc.WindowName);
            forgeables.Add(aiFactoryForgeable);

            var statementProviderClassName =
                CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementProvider), classPostfix);
            var informationals = StatementInformationalsUtil.GetInformationals(
                _base,
                Collections.SingletonList(compileResult.FilterSpecCompiled),
                schedules,
                EmptyList<NamedWindowConsumerStreamSpec>.Instance,
                true,
                selectSubscriberDescriptor,
                namespaceScope,
                services);
            informationals.Properties.Put(StatementProperty.CREATEOBJECTNAME, createWindowDesc.WindowName);

            forgeables.Add(
                new StmtClassForgeableStmtProvider(
                    aiFactoryProviderClassName,
                    statementProviderClassName,
                    informationals,
                    namespaceScope));
            forgeables.Add(
                new StmtClassForgeableStmtFields(
                    statementFieldsClassName,
                    namespaceScope,
                    1));

            return new StmtForgeMethodResult(
                forgeables,
                Collections.SingletonList(compileResult.FilterSpecCompiled),
                schedules,
                EmptyList<NamedWindowConsumerStreamSpec>.Instance,
                EmptyList<FilterSpecParamExprNodeForge>.Instance);
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