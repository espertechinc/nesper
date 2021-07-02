///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.compile.stage3;
using com.espertech.esper.common.@internal.context.aifactory.core;
using com.espertech.esper.common.@internal.context.module;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.map;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.context.activator
{
    public class ViewableActivatorPatternForge : ViewableActivatorForge
    {
        private readonly EventType _eventType;
        private readonly bool _isCanIterate;
        private readonly PatternContext _patternContext;
        private readonly PatternStreamSpecCompiled _spec;

        public ViewableActivatorPatternForge(
            EventType eventType,
            PatternStreamSpecCompiled spec,
            PatternContext patternContext,
            bool isCanIterate)
        {
            _eventType = eventType;
            _spec = spec;
            _patternContext = patternContext;
            _isCanIterate = isCanIterate;
        }

        public CodegenExpression MakeCodegen(
            CodegenMethodScope parent,
            SAIFFInitializeSymbol symbols,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(ViewableActivator), typeof(ViewableActivatorPatternForge), classScope);

            var childCode = _spec.Root.MakeCodegen(method, symbols, classScope);
            method.Block
                .DeclareVar<EvalRootFactoryNode>("root", LocalMethod(childCode))
                .DeclareVar<ViewableActivatorPattern>(
                    "activator",
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Get(EPStatementInitServicesConstants.VIEWABLEACTIVATORFACTORY)
                        .Add("CreatePattern"))
                .SetProperty(Ref("activator"), "RootFactoryNode", Ref("root"))
                .SetProperty(
                    Ref("activator"),
                    "EventBeanTypedEventFactory",
                    ExprDotMethodChain(symbols.GetAddInitSvc(method))
                        .Get(EPStatementInitServicesConstants.EVENTBEANTYPEDEVENTFACTORY))
                .DeclareVar<EventType>(
                    "eventType",
                    EventTypeUtility.ResolveTypeCodegen(_eventType, symbols.GetAddInitSvc(method)))
                .SetProperty(Ref("activator"), "EventType", Ref("eventType"))
                .SetProperty(Ref("activator"), "PatternContext", _patternContext.Make(method, symbols, classScope))
                .SetProperty(Ref("activator"), "HasConsumingFilter", Constant(_spec.IsConsumingFilters))
                .SetProperty(Ref("activator"), "SuppressSameEventMatches", Constant(_spec.IsSuppressSameEventMatches))
                .SetProperty(Ref("activator"), "DiscardPartialsOnMatch", Constant(_spec.IsDiscardPartialsOnMatch))
                .SetProperty(Ref("activator"), "CanIterate", Constant(_isCanIterate))
                .MethodReturn(Ref("activator"));

            return LocalMethod(method);
        }

        public static MapEventType MakeRegisterPatternType(
            string moduleName,
            int stream,
            ICollection<string> onlyIncludeTheseTags,
            PatternStreamSpecCompiled patternStreamSpec,
            StatementCompileTimeServices services)
        {
            var patternEventTypeName = services.EventTypeNameGeneratorStatement.GetPatternTypeName(stream);
            var metadata = new EventTypeMetadata(
                patternEventTypeName,
                moduleName,
                EventTypeTypeClass.STREAM,
                EventTypeApplicationType.MAP,
                NameAccessModifier.PRIVATE,
                EventTypeBusModifier.NONBUS,
                false,
                EventTypeIdPair.Unassigned());
            IDictionary<string, object> propertyTypes = new LinkedHashMap<string, object>();
            foreach (var entry in patternStreamSpec.TaggedEventTypes) {
                if (onlyIncludeTheseTags == null || onlyIncludeTheseTags.Contains(entry.Key)) {
                    propertyTypes.Put(entry.Key, entry.Value.First);
                }
            }

            foreach (var entry in patternStreamSpec.ArrayEventTypes) {
                if (onlyIncludeTheseTags == null || onlyIncludeTheseTags.Contains(entry.Key)) {
                    propertyTypes.Put(entry.Key, new[] {entry.Value.First});
                }
            }

            var patternType = BaseNestableEventUtil.MakeMapTypeCompileTime(
                metadata,
                propertyTypes,
                null,
                null,
                null,
                null,
                services.BeanEventTypeFactoryPrivate,
                services.EventTypeCompileTimeResolver);
            services.EventTypeCompileTimeRegistry.NewType(patternType);
            return patternType;
        }
    }
} // end of namespace