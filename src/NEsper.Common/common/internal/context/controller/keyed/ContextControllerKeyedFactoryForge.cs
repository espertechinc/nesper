///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;
using System.Linq;

using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage2;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.controller.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.statemgmtsettings;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.controller.keyed
{
    public class ContextControllerKeyedFactoryForge : ContextControllerForgeBase
    {
        private readonly ContextSpecKeyed _detail;
        private StateMgmtSetting terminationStateMgmtSettings = StateMgmtSettingDefault.INSTANCE;
        private StateMgmtSetting ctxStateMgmtSettings;

        public ContextControllerKeyedFactoryForge(
            ContextControllerFactoryEnv ctx,
            ContextSpecKeyed detail)
            : base(ctx)
        {
            this._detail = detail;
        }

        public override ContextControllerPortableInfo ValidationInfo {
            get {
                var items = new ContextControllerKeyedValidationItem[_detail.Items.Count];
                for (var i = 0; i < _detail.Items.Count; i++) {
                    var props = _detail.Items[i];
                    items[i] = new ContextControllerKeyedValidationItem(
                        props.FilterSpecCompiled.FilterForEventType,
                        props.PropertyNames.ToArray());
                }

                return new ContextControllerKeyedValidation(items);
            }
        }

        public override void ValidateGetContextProps(
            IDictionary<string, object> props,
            string contextName,
            StatementRawInfo statementRawInfo,
            StatementCompileTimeServices services)
        {
            var propertyTypes = ContextControllerKeyedUtil.ValidateContextDesc(contextName, _detail);

            for (var i = 0; i < _detail.Items[0].PropertyNames.Count; i++) {
                var propertyName = ContextPropertyEventType.PROP_CTX_KEY_PREFIX + (i + 1);
                props.Put(propertyName, propertyTypes[i]);
            }

            var allTags = new LinkedHashSet<string>();
            foreach (var item in _detail.Items) {
                if (item.AliasName != null) {
                    allTags.Add(item.AliasName);
                }
            }

            if (_detail.OptionalInit != null) {
                foreach (var filter in _detail.OptionalInit) {
                    ContextPropertyEventType.AddEndpointTypes(filter, props, allTags);
                }
            }

            if (_detail.OptionalTermination != null) {
                ContextPropertyEventType.AddEndpointTypes(_detail.OptionalTermination, props, allTags);
                terminationStateMgmtSettings = services.StateMgmtSettingsProvider.GetContext(statementRawInfo, contextName, AppliesTo.CONTEXT_KEYED_TERM);
            }
            
            ctxStateMgmtSettings = services.StateMgmtSettingsProvider.GetContext(statementRawInfo, contextName, AppliesTo.CONTEXT_KEYED);
        }

        public override CodegenMethod MakeCodegen(
            CodegenClassScope classScope,
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols)
        {
            var method = parent.MakeChild(typeof(ContextControllerKeyedFactory), GetType(), classScope);
            method.Block
                .DeclareVar<ContextControllerKeyedFactory>(
                    "factory",
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Get(EPStatementInitServicesConstants.CONTEXTSERVICEFACTORY)
                        .Add("KeyedFactory", terminationStateMgmtSettings.ToExpression(), ctxStateMgmtSettings.ToExpression()))
                .SetProperty(Ref("factory"), "KeyedSpec", _detail.MakeCodegen(method, symbols, classScope))
                .MethodReturn(Ref("factory"));
            return method;
        }
    }
} // end of namespace