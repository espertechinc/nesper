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

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.fabric;
using com.espertech.esper.common.@internal.statemgmtsettings;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.controller.keyed
{
    public class ContextControllerKeyedFactoryForge : ContextControllerForgeBase
    {
        private readonly ContextSpecKeyed detail;
        private StateMgmtSetting terminationStateMgmtSettings = StateMgmtSettingDefault.INSTANCE;
        private StateMgmtSetting ctxStateMgmtSettings;

        public ContextControllerKeyedFactoryForge(
            ContextControllerFactoryEnv ctx,
            ContextSpecKeyed detail) : base(ctx)
        {
            this.detail = detail;
        }

        public override void ValidateGetContextProps(
            IDictionary<string, object> props,
            string contextName,
            int controllerLevel,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            var propertyTypes = ContextControllerKeyedUtil.ValidateContextDesc(contextName, detail);
            for (var i = 0; i < detail.Items[0].PropertyNames.Count; i++) {
                var propertyName = ContextPropertyEventType.PROP_CTX_KEY_PREFIX + (i + 1);
                props.Put(propertyName, propertyTypes[i]);
            }

            var allTags = new LinkedHashSet<string>();
            foreach (var item in detail.Items) {
                if (item.AliasName != null) {
                    allTags.Add(item.AliasName);
                }
            }

            if (detail.OptionalInit != null) {
                foreach (var filter in detail.OptionalInit) {
                    ContextPropertyEventType.AddEndpointTypes(filter, props, allTags);
                }
            }

            if (detail.OptionalTermination != null) {
                ContextPropertyEventType.AddEndpointTypes(detail.OptionalTermination, props, allTags);
            }
        }

        public override void PlanStateSettings(
            ContextMetaData detail,
            FabricCharge fabricCharge,
            int controllerLevel,
            string nestedContextName,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            if (this.detail.OptionalTermination != null) {
                terminationStateMgmtSettings = services.StateMgmtSettingsProvider.Context.ContextKeyedTerm(
                    fabricCharge,
                    detail,
                    this,
                    statementRawInfo,
                    controllerLevel);
            }

            ctxStateMgmtSettings = services.StateMgmtSettingsProvider.Context.ContextKeyed(
                fabricCharge,
                detail,
                this,
                statementRawInfo,
                controllerLevel);
        }

        public override CodegenMethod MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols)
        {
            var method = parent.MakeChild(typeof(ContextControllerKeyedFactory), GetType(), classScope);
            method.Block.DeclareVar<ContextControllerKeyedFactory>(
                    "factory",
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Get(EPStatementInitServicesConstants.CONTEXTSERVICEFACTORY)
                        .Add(
                            "KeyedFactory",
                            terminationStateMgmtSettings.ToExpression(),
                            ctxStateMgmtSettings.ToExpression()))
                .SetProperty(Ref("factory"), "KeyedSpec", detail.MakeCodegen(method, symbols, classScope))
                .MethodReturn(Ref("factory"));
            return method;
        }

        public override T Accept<T>(ContextControllerFactoryForgeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public override ContextControllerPortableInfo ValidationInfo {
            get {
                var items = new ContextControllerKeyedValidationItem[detail.Items.Count];
                for (var i = 0; i < detail.Items.Count; i++) {
                    var props = detail.Items[i];
                    items[i] = new ContextControllerKeyedValidationItem(
                        props.FilterSpecCompiled.FilterForEventType,
                        props.PropertyNames.ToArray());
                }

                return new ContextControllerKeyedValidation(items);
            }
        }

        public ContextSpecKeyed Detail => detail;
    }
} // end of namespace