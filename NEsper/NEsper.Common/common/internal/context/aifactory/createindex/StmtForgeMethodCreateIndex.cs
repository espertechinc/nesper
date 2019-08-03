///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.index.compile;
using com.espertech.esper.common.@internal.epl.@join.lookup;
using com.espertech.esper.common.@internal.epl.@join.queryplan;
using com.espertech.esper.common.@internal.epl.lookup;
using com.espertech.esper.common.@internal.epl.lookupplansubord;
using com.espertech.esper.common.@internal.epl.namedwindow.path;
using com.espertech.esper.common.@internal.epl.resultset.@select.core;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.epl.util;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.context.aifactory.createindex
{
    /// <summary>
    ///     Starts and provides the stop method for EPL statements.
    /// </summary>
    public class StmtForgeMethodCreateIndex : StmtForgeMethod
    {
        private readonly StatementBaseInfo @base;

        public StmtForgeMethodCreateIndex(StatementBaseInfo @base)
        {
            this.@base = @base;
        }

        public StmtForgeMethodResult Make(
            string @namespace,
            string classPostfix,
            StatementCompileTimeServices services)
        {
            var spec = @base.StatementSpec.Raw.CreateIndexDesc;

            var infraName = spec.WindowName;
            var namedWindow = services.NamedWindowCompileTimeResolver.Resolve(infraName);
            var table = services.TableCompileTimeResolver.Resolve(infraName);
            if (namedWindow == null && table == null) {
                throw new ExprValidationException("A named window or table by name '" + infraName + "' does not exist");
            }

            if (namedWindow != null && table != null) {
                throw new ExprValidationException("A named window or table by name '" + infraName + "' are both found");
            }

            string infraModuleName;
            NameAccessModifier infraVisibility;
            EventType indexedEventType;
            string infraContextName;
            if (namedWindow != null) {
                infraModuleName = namedWindow.NamedWindowModuleName;
                infraVisibility = namedWindow.EventType.Metadata.AccessModifier;
                indexedEventType = namedWindow.EventType;
                infraContextName = namedWindow.ContextName;
            }
            else {
                infraModuleName = table.TableModuleName;
                infraVisibility = table.TableVisibility;
                indexedEventType = table.InternalEventType;
                infraContextName = table.OptionalContextName;

                if (!table.IsKeyed) {
                    throw new ExprValidationException(
                        "Tables without primary key column(s) do not allow creating an index");
                }
            }

            EPLValidationUtil.ValidateContextName(
                namedWindow == null,
                infraName,
                infraContextName,
                @base.StatementSpec.Raw.OptionalContextName,
                true);

            // validate index
            var explicitIndexDesc = EventTableIndexUtil.ValidateCompileExplicitIndex(
                spec.IndexName,
                spec.IsUnique,
                spec.Columns,
                indexedEventType,
                @base.StatementRawInfo,
                services);
            var advancedIndexDesc = explicitIndexDesc.AdvancedIndexProvisionDesc == null
                ? null
                : explicitIndexDesc.AdvancedIndexProvisionDesc.IndexDesc.AdvancedIndexDescRuntime;
            var imk = new IndexMultiKey(
                spec.IsUnique,
                explicitIndexDesc.HashPropsAsList,
                explicitIndexDesc.BtreePropsAsList,
                advancedIndexDesc);

            // add index as a new index to module-init
            var indexKey = new IndexCompileTimeKey(
                infraModuleName,
                infraName,
                infraVisibility,
                namedWindow != null,
                spec.IndexName,
                @base.ModuleName);
            services.IndexCompileTimeRegistry.NewIndex(indexKey, new IndexDetailForge(imk, explicitIndexDesc));

            // add index current named window information
            if (namedWindow != null) {
                namedWindow.AddIndex(spec.IndexName, @base.ModuleName, imk, explicitIndexDesc.ToRuntime());
            }
            else {
                table.AddIndex(spec.IndexName, @base.ModuleName, imk, explicitIndexDesc.ToRuntime());
            }

            var packageScope = new CodegenNamespaceScope(@namespace, null, services.IsInstrumented);

            var aiFactoryProviderClassName = CodeGenerationIDGenerator.GenerateClassNameSimple(
                typeof(StatementAIFactoryProvider),
                classPostfix);
            var forge = new StatementAgentInstanceFactoryCreateIndexForge(
                indexedEventType,
                spec.IndexName,
                @base.ModuleName,
                explicitIndexDesc,
                imk,
                namedWindow,
                table);
            var aiFactoryForgable = new StmtClassForgableAIFactoryProviderCreateIndex(
                aiFactoryProviderClassName,
                packageScope,
                forge);

            var selectSubscriberDescriptor = new SelectSubscriberDescriptor();
            var informationals = StatementInformationalsUtil.GetInformationals(
                @base,
                new EmptyList<FilterSpecCompiled>(),
                new EmptyList<ScheduleHandleCallbackProvider>(),
                new EmptyList<NamedWindowConsumerStreamSpec>(),
                true,
                selectSubscriberDescriptor,
                packageScope,
                services);
            var statementProviderClassName =
                CodeGenerationIDGenerator.GenerateClassNameSimple(typeof(StatementProvider), classPostfix);
            var stmtProvider = new StmtClassForgableStmtProvider(
                aiFactoryProviderClassName,
                statementProviderClassName,
                informationals,
                packageScope);

            IList<StmtClassForgable> forgables = new List<StmtClassForgable>();
            forgables.Add(aiFactoryForgable);
            forgables.Add(stmtProvider);
            return new StmtForgeMethodResult(
                forgables,
                new EmptyList<FilterSpecCompiled>(),
                new EmptyList<ScheduleHandleCallbackProvider>(),
                new EmptyList<NamedWindowConsumerStreamSpec>(),
                new EmptyList<FilterSpecParamExprNodeForge>());
        }
    }
} // end of namespace