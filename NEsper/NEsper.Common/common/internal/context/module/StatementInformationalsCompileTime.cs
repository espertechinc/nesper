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

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.metrics.audit;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.annotation.AnnotationUtil;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.epl.resultset.codegen.ResultSetProcessorCodegenNames;

namespace com.espertech.esper.common.@internal.context.module
{
    public class StatementInformationalsCompileTime
    {
        private readonly bool _allowSubscriber;
        private readonly bool _alwaysSynthesizeOutputEvents; // set when insert-into/for-clause/select-distinct
        private readonly Attribute[] _annotations;
        private readonly bool _canSelfJoin;
        private readonly bool _forClauseDelivery;
        private readonly ExprNode[] _groupDelivery;
        private readonly bool _hasMatchRecognize;
        private readonly bool _hasSubquery;
        private readonly bool _hasTableAccess;
        private readonly bool _hasVariables;
        private readonly string _insertIntoLatchName;
        private readonly bool _instrumented;
        private readonly bool _needDedup;
        private readonly int _numFilterCallbacks;
        private readonly int _numNamedWindowCallbacks;
        private readonly int _numScheduleCallbacks;
        private readonly string _optionalContextModuleName;
        private readonly string _optionalContextName;
        private readonly NameAccessModifier _optionalContextVisibility;
        private readonly CodegenPackageScope _packageScope;
        private readonly bool _preemptive;
        private readonly int _priority;
        private readonly IDictionary<StatementProperty, object> _properties;
        private readonly string[] _selectClauseColumnNames;
        private readonly Type[] _selectClauseTypes;
        private readonly bool _stateless;
        private readonly string _statementNameCompileTime;
        private readonly StatementType _statementType;
        private readonly object _userObjectCompileTime;
        private readonly bool _writesToTables;

        public StatementInformationalsCompileTime(
            string statementNameCompileTime,
            bool alwaysSynthesizeOutputEvents,
            string optionalContextName,
            string optionalContextModuleName,
            NameAccessModifier optionalContextVisibility,
            bool canSelfJoin,
            bool hasSubquery,
            bool needDedup,
            Attribute[] annotations,
            bool stateless,
            object userObjectCompileTime,
            int numFilterCallbacks,
            int numScheduleCallbacks,
            int numNamedWindowCallbacks,
            StatementType statementType, int priority, bool preemptive, bool hasVariables,
            bool writesToTables,
            bool hasTableAccess,
            Type[] selectClauseTypes,
            string[] selectClauseColumnNames,
            bool forClauseDelivery,
            ExprNode[] groupDelivery,
            IDictionary<StatementProperty, object> properties,
            bool hasMatchRecognize,
            bool instrumented,
            CodegenPackageScope packageScope,
            string insertIntoLatchName,
            bool allowSubscriber)
        {
            _statementNameCompileTime = statementNameCompileTime;
            _alwaysSynthesizeOutputEvents = alwaysSynthesizeOutputEvents;
            _optionalContextName = optionalContextName;
            _optionalContextModuleName = optionalContextModuleName;
            _optionalContextVisibility = optionalContextVisibility;
            _canSelfJoin = canSelfJoin;
            _hasSubquery = hasSubquery;
            _needDedup = needDedup;
            _annotations = annotations;
            _stateless = stateless;
            _userObjectCompileTime = userObjectCompileTime;
            _numFilterCallbacks = numFilterCallbacks;
            _numScheduleCallbacks = numScheduleCallbacks;
            _numNamedWindowCallbacks = numNamedWindowCallbacks;
            _statementType = statementType;
            _priority = priority;
            _preemptive = preemptive;
            _hasVariables = hasVariables;
            _writesToTables = writesToTables;
            _hasTableAccess = hasTableAccess;
            _selectClauseTypes = selectClauseTypes;
            _selectClauseColumnNames = selectClauseColumnNames;
            _forClauseDelivery = forClauseDelivery;
            _groupDelivery = groupDelivery;
            _properties = properties;
            _hasMatchRecognize = hasMatchRecognize;
            _instrumented = instrumented;
            _packageScope = packageScope;
            _insertIntoLatchName = insertIntoLatchName;
            _allowSubscriber = allowSubscriber;
        }

        public CodegenExpression Make(CodegenMethodScope parent, CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(StatementInformationalsRuntime), GetType(), classScope);
            var info = Ref("info");
            method.Block
                .DeclareVar(
                    typeof(StatementInformationalsRuntime), info.Ref,
                    NewInstance(typeof(StatementInformationalsRuntime)))
                .ExprDotMethod(info, "setStatementNameCompileTime", Constant(_statementNameCompileTime))
                .ExprDotMethod(info, "setAlwaysSynthesizeOutputEvents", Constant(_alwaysSynthesizeOutputEvents))
                .ExprDotMethod(info, "setOptionalContextName", Constant(_optionalContextName))
                .ExprDotMethod(info, "setOptionalContextModuleName", Constant(_optionalContextModuleName))
                .ExprDotMethod(info, "setOptionalContextVisibility", Constant(_optionalContextVisibility))
                .ExprDotMethod(info, "setCanSelfJoin", Constant(_canSelfJoin))
                .ExprDotMethod(info, "setHasSubquery", Constant(_hasSubquery))
                .ExprDotMethod(info, "setNeedDedup", Constant(_needDedup))
                .ExprDotMethod(info, "setStateless", Constant(_stateless))
                .ExprDotMethod(
                    info, "setAnnotations",
                    _annotations == null
                        ? ConstantNull()
                        : LocalMethod(MakeAnnotations(typeof(Attribute[]), _annotations, method, classScope)))
                .ExprDotMethod(
                    info, "setUserObjectCompileTime", SerializerUtil.ExpressionForUserObject(_userObjectCompileTime))
                .ExprDotMethod(info, "setNumFilterCallbacks", Constant(_numFilterCallbacks))
                .ExprDotMethod(info, "setNumScheduleCallbacks", Constant(_numScheduleCallbacks))
                .ExprDotMethod(info, "setNumNamedWindowCallbacks", Constant(_numNamedWindowCallbacks))
                .ExprDotMethod(info, "setStatementType", Constant(_statementType))
                .ExprDotMethod(info, "setPriority", Constant(_priority))
                .ExprDotMethod(info, "setPreemptive", Constant(_preemptive))
                .ExprDotMethod(info, "setHasVariables", Constant(_hasVariables))
                .ExprDotMethod(info, "setWritesToTables", Constant(_writesToTables))
                .ExprDotMethod(info, "setHasTableAccess", Constant(_hasTableAccess))
                .ExprDotMethod(info, "setSelectClauseTypes", Constant(_selectClauseTypes))
                .ExprDotMethod(info, "setSelectClauseColumnNames", Constant(_selectClauseColumnNames))
                .ExprDotMethod(info, "setForClauseDelivery", Constant(_forClauseDelivery))
                .ExprDotMethod(
                    info, "setGroupDeliveryEval",
                    _groupDelivery == null
                        ? ConstantNull()
                        : ExprNodeUtilityCodegen.CodegenEvaluatorMayMultiKeyWCoerce(
                            ExprNodeUtilityQuery.GetForges(_groupDelivery), null, method, GetType(), classScope))
                .ExprDotMethod(info, "setProperties", MakeProperties(_properties, method, classScope))
                .ExprDotMethod(info, "setHasMatchRecognize", Constant(_hasMatchRecognize))
                .ExprDotMethod(info, "setAuditProvider", MakeAuditProvider(method, classScope))
                .ExprDotMethod(info, "setInstrumented", Constant(_instrumented))
                .ExprDotMethod(info, "setInstrumentationProvider", MakeInstrumentationProvider(method, classScope))
                .ExprDotMethod(info, "setSubstitutionParamTypes", MakeSubstitutionParamTypes())
                .ExprDotMethod(info, "setSubstitutionParamNames", MakeSubstitutionParamNames(method, classScope))
                .ExprDotMethod(info, "setInsertIntoLatchName", Constant(_insertIntoLatchName))
                .ExprDotMethod(info, "setAllowSubscriber", Constant(_allowSubscriber))
                .MethodReturn(info);
            return LocalMethod(method);
        }

        private CodegenExpression MakeSubstitutionParamTypes()
        {
            var numbered = _packageScope.SubstitutionParamsByNumber;
            var named = _packageScope.SubstitutionParamsByName;
            if (numbered.IsEmpty() && named.IsEmpty()) {
                return ConstantNull();
            }

            if (!numbered.IsEmpty() && !named.IsEmpty()) {
                throw new IllegalStateException("Both named and numbered substitution parameters are non-empty");
            }

            Type[] types;
            if (!numbered.IsEmpty()) {
                types = new Type[numbered.Count];
                for (var i = 0; i < numbered.Count; i++) {
                    types[i] = numbered[i].Type;
                }
            }
            else {
                types = new Type[named.Count];
                var count = 0;
                foreach (var entry in named) {
                    types[count++] = entry.Value.Type;
                }
            }

            return Constant(types);
        }

        private CodegenExpression MakeSubstitutionParamNames(CodegenMethodScope parent, CodegenClassScope classScope)
        {
            var named = _packageScope.SubstitutionParamsByName;
            if (named.IsEmpty()) {
                return ConstantNull();
            }

            var method = parent.MakeChild(typeof(IDictionary<object, object>), GetType(), classScope);
            method.Block.DeclareVar(
                typeof(IDictionary<object, object>), "names",
                NewInstance(typeof(Dictionary<object, object>), Constant(CollectionUtil.CapacityHashMap(named.Count))));
            var count = 1;
            foreach (var entry in named) {
                method.Block.ExprDotMethod(Ref("names"), "put", Constant(entry.Key), Constant(count++));
            }

            method.Block.MethodReturn(Ref("names"));
            return LocalMethod(method);
        }

        private CodegenExpression MakeInstrumentationProvider(CodegenMethod method, CodegenClassScope classScope)
        {
            if (!_instrumented) {
                return ConstantNull();
            }

            var anonymousClass = NewAnonymousClass(
                method.Block, typeof(InstrumentationCommon));

            var activated = CodegenMethod.MakeParentNode(typeof(bool), GetType(), classScope);
            anonymousClass.AddMethod("activated", activated);
            activated.Block.MethodReturn(ConstantTrue());

            foreach (var forwarded in typeof(InstrumentationCommon).GetMethods()) {
                if (forwarded.DeclaringType == typeof(object)) {
                    continue;
                }

                if (forwarded.Name.Equals("activated")) {
                    continue;
                }

                IList<CodegenNamedParam> @params = new List<CodegenNamedParam>();
                var expressions = new CodegenExpression[forwarded.ParameterCount];

                var num = 0;
                foreach (var param in forwarded.GetParameters()) {
                    @params.Add(new CodegenNamedParam(param.ParameterType, param.Name));
                    expressions[num] = Ref(param.Name);
                    num++;
                }

                var m = CodegenMethod.MakeParentNode(typeof(void), GetType(), classScope)
                    .AddParam(@params);
                anonymousClass.AddMethod(forwarded.Name, m);
                m.Block.Apply(InstrumentationCode.Instblock(classScope, forwarded.Name, expressions));
            }

            return anonymousClass;
        }

        private CodegenExpression MakeAuditProvider(CodegenMethod method, CodegenClassScope classScope)
        {
            if (FindAnnotation(_annotations, typeof(AuditAttribute)) == null) {
                return PublicConstValue(typeof(AuditProviderDefault), "INSTANCE");
            }

            var anonymousClass = NewAnonymousClass(method.Block, typeof(AuditProvider));

            var activated = CodegenMethod.MakeParentNode(typeof(bool), GetType(), classScope);
            anonymousClass.AddMethod("activated", activated);
            activated.Block.MethodReturn(ConstantTrue());

            var view = CodegenMethod.MakeParentNode(typeof(void), GetType(), classScope)
                .AddParam(typeof(EventBean[]), "newData").AddParam(typeof(EventBean[]), "oldData")
                .AddParam(typeof(AgentInstanceContext), REF_AGENTINSTANCECONTEXT.Ref)
                .AddParam(typeof(ViewFactory), "viewFactory");
            anonymousClass.AddMethod("view", view);
            if (AuditEnum.VIEW.GetAudit(_annotations) != null) {
                view.Block.StaticMethod(
                    typeof(AuditPath), "auditView", Ref("newData"), Ref("oldData"), REF_AGENTINSTANCECONTEXT,
                    Ref("viewFactory"));
            }

            var streamOne = CodegenMethod.MakeParentNode(typeof(void), GetType(), classScope)
                .AddParam(typeof(EventBean), "event").AddParam(typeof(ExprEvaluatorContext), REF_EXPREVALCONTEXT.Ref)
                .AddParam(typeof(string), "filterText");
            anonymousClass.AddMethod("stream", streamOne);
            var streamTwo = CodegenMethod.MakeParentNode(typeof(void), GetType(), classScope)
                .AddParam(typeof(EventBean[]), "newData").AddParam(typeof(EventBean[]), "oldData")
                .AddParam(typeof(ExprEvaluatorContext), REF_EXPREVALCONTEXT.Ref).AddParam(typeof(string), "filterText");
            anonymousClass.AddMethod("stream", streamTwo);
            if (AuditEnum.STREAM.GetAudit(_annotations) != null) {
                streamOne.Block.StaticMethod(
                    typeof(AuditPath), "auditStream", Ref("event"), REF_EXPREVALCONTEXT, Ref("filterText"));
                streamTwo.Block.StaticMethod(
                    typeof(AuditPath), "auditStream", Ref("newData"), Ref("oldData"), REF_EXPREVALCONTEXT,
                    Ref("filterText"));
            }

            var scheduleAdd = CodegenMethod.MakeParentNode(typeof(void), GetType(), classScope)
                .AddParam(typeof(long), "time").AddParam(typeof(AgentInstanceContext), REF_AGENTINSTANCECONTEXT.Ref)
                .AddParam(typeof(ScheduleHandle), "scheduleHandle").AddParam(typeof(ScheduleObjectType), "type")
                .AddParam(typeof(string), "name");
            var scheduleRemove = CodegenMethod.MakeParentNode(typeof(void), GetType(), classScope)
                .AddParam(typeof(AgentInstanceContext), REF_AGENTINSTANCECONTEXT.Ref)
                .AddParam(typeof(ScheduleHandle), "scheduleHandle").AddParam(typeof(ScheduleObjectType), "type")
                .AddParam(typeof(string), "name");
            var scheduleFire = CodegenMethod.MakeParentNode(typeof(void), GetType(), classScope)
                .AddParam(typeof(AgentInstanceContext), REF_AGENTINSTANCECONTEXT.Ref)
                .AddParam(typeof(ScheduleObjectType), "type").AddParam(typeof(string), "name");
            anonymousClass.AddMethod("scheduleAdd", scheduleAdd);
            anonymousClass.AddMethod("scheduleRemove", scheduleRemove);
            anonymousClass.AddMethod("scheduleFire", scheduleFire);
            if (AuditEnum.SCHEDULE.GetAudit(_annotations) != null) {
                scheduleAdd.Block.StaticMethod(
                    typeof(AuditPath), "auditScheduleAdd", Ref("time"), REF_AGENTINSTANCECONTEXT,
                    Ref("scheduleHandle"), Ref("type"), Ref("name"));
                scheduleRemove.Block.StaticMethod(
                    typeof(AuditPath), "auditScheduleRemove", REF_AGENTINSTANCECONTEXT, Ref("scheduleHandle"),
                    Ref("type"), Ref("name"));
                scheduleFire.Block.StaticMethod(
                    typeof(AuditPath), "auditScheduleFire", REF_AGENTINSTANCECONTEXT, Ref("type"), Ref("name"));
            }

            var property = CodegenMethod.MakeParentNode(typeof(void), GetType(), classScope)
                .AddParam(typeof(string), "name").AddParam(typeof(object), "value").AddParam(
                    typeof(ExprEvaluatorContext), REF_EXPREVALCONTEXT.Ref);
            anonymousClass.AddMethod("property", property);
            if (AuditEnum.PROPERTY.GetAudit(_annotations) != null) {
                property.Block.StaticMethod(
                    typeof(AuditPath), "auditProperty", Ref("name"), Ref("value"), REF_EXPREVALCONTEXT);
            }

            var insert = CodegenMethod.MakeParentNode(typeof(void), GetType(), classScope)
                .AddParam(typeof(EventBean), "event").AddParam(typeof(ExprEvaluatorContext), REF_EXPREVALCONTEXT.Ref);
            anonymousClass.AddMethod("insert", insert);
            if (AuditEnum.INSERT.GetAudit(_annotations) != null) {
                insert.Block.StaticMethod(typeof(AuditPath), "auditInsert", Ref("event"), REF_EXPREVALCONTEXT);
            }

            var expression = CodegenMethod.MakeParentNode(typeof(void), GetType(), classScope)
                .AddParam(typeof(string), "text").AddParam(typeof(object), "value").AddParam(
                    typeof(ExprEvaluatorContext), REF_EXPREVALCONTEXT.Ref);
            anonymousClass.AddMethod("expression", expression);
            if (AuditEnum.EXPRESSION.GetAudit(_annotations) != null ||
                AuditEnum.EXPRESSION_NESTED.GetAudit(_annotations) != null) {
                expression.Block.StaticMethod(
                    typeof(AuditPath), "auditExpression", Ref("text"), Ref("value"), REF_EXPREVALCONTEXT);
            }

            var patternTrue = CodegenMethod.MakeParentNode(typeof(void), GetType(), classScope)
                .AddParam(typeof(EvalFactoryNode), "factoryNode").AddParam(typeof(object), "from")
                .AddParam(typeof(MatchedEventMapMinimal), "matchEvent").AddParam(typeof(bool), "isQuitted").AddParam(
                    typeof(AgentInstanceContext), NAME_AGENTINSTANCECONTEXT);
            var patternFalse = CodegenMethod.MakeParentNode(typeof(void), GetType(), classScope)
                .AddParam(typeof(EvalFactoryNode), "factoryNode").AddParam(typeof(object), "from").AddParam(
                    typeof(AgentInstanceContext), NAME_AGENTINSTANCECONTEXT);
            anonymousClass.AddMethod("patternTrue", patternTrue);
            anonymousClass.AddMethod("patternFalse", patternFalse);
            if (AuditEnum.PATTERN.GetAudit(_annotations) != null) {
                patternTrue.Block.StaticMethod(
                    typeof(AuditPath), "auditPatternTrue", Ref("factoryNode"), Ref("from"), Ref("matchEvent"),
                    Ref("isQuitted"), REF_AGENTINSTANCECONTEXT);
                patternFalse.Block.StaticMethod(
                    typeof(AuditPath), "auditPatternFalse", Ref("factoryNode"), Ref("from"),
                    REF_AGENTINSTANCECONTEXT);
            }

            var patternInstance = CodegenMethod.MakeParentNode(typeof(void), GetType(), classScope)
                .AddParam(typeof(bool), "increase").AddParam(typeof(EvalFactoryNode), "factoryNode").AddParam(
                    typeof(AgentInstanceContext), NAME_AGENTINSTANCECONTEXT);
            anonymousClass.AddMethod("patternInstance", patternInstance);
            if (AuditEnum.PATTERNINSTANCES.GetAudit(_annotations) != null) {
                patternInstance.Block.StaticMethod(
                    typeof(AuditPath), "auditPatternInstance", Ref("increase"), Ref("factoryNode"),
                    REF_AGENTINSTANCECONTEXT);
            }

            var exprdef = CodegenMethod.MakeParentNode(typeof(void), GetType(), classScope)
                .AddParam(typeof(string), "name").AddParam(typeof(object), "value").AddParam(
                    typeof(ExprEvaluatorContext), REF_EXPREVALCONTEXT.Ref);
            anonymousClass.AddMethod("exprdef", exprdef);
            if (AuditEnum.EXPRDEF.GetAudit(_annotations) != null) {
                exprdef.Block.StaticMethod(
                    typeof(AuditPath), "auditExprDef", Ref("name"), Ref("value"), REF_EXPREVALCONTEXT);
            }

            var dataflowTransition = CodegenMethod.MakeParentNode(typeof(void), GetType(), classScope)
                .AddParam(typeof(string), "name").AddParam(typeof(string), "instance")
                .AddParam(typeof(EPDataFlowState), "state").AddParam(typeof(EPDataFlowState), "newState").AddParam(
                    typeof(AgentInstanceContext), REF_AGENTINSTANCECONTEXT.Ref);
            anonymousClass.AddMethod("dataflowTransition", dataflowTransition);
            if (AuditEnum.DATAFLOW_TRANSITION.GetAudit(_annotations) != null) {
                dataflowTransition.Block.StaticMethod(
                    typeof(AuditPath), "auditDataflowTransition", Ref("name"), Ref("instance"), Ref("state"),
                    Ref("newState"), REF_AGENTINSTANCECONTEXT);
            }

            var dataflowSource = CodegenMethod.MakeParentNode(typeof(void), GetType(), classScope)
                .AddParam(typeof(string), "name").AddParam(typeof(string), "instance")
                .AddParam(typeof(string), "operatorName").AddParam(typeof(int), "operatorNum").AddParam(
                    typeof(AgentInstanceContext), REF_AGENTINSTANCECONTEXT.Ref);
            anonymousClass.AddMethod("dataflowSource", dataflowSource);
            if (AuditEnum.DATAFLOW_SOURCE.GetAudit(_annotations) != null) {
                dataflowSource.Block.StaticMethod(
                    typeof(AuditPath), "auditDataflowSource", Ref("name"), Ref("instance"), Ref("operatorName"),
                    Ref("operatorNum"), REF_AGENTINSTANCECONTEXT);
            }

            var dataflowOp = CodegenMethod.MakeParentNode(typeof(void), GetType(), classScope)
                .AddParam(typeof(string), "name").AddParam(typeof(string), "instance")
                .AddParam(typeof(string), "operatorName").AddParam(typeof(int), "operatorNum")
                .AddParam(typeof(object[]), "params").AddParam(
                    typeof(AgentInstanceContext), REF_AGENTINSTANCECONTEXT.Ref);
            anonymousClass.AddMethod("dataflowOp", dataflowOp);
            if (AuditEnum.DATAFLOW_OP.GetAudit(_annotations) != null) {
                dataflowOp.Block.StaticMethod(
                    typeof(AuditPath), "auditDataflowOp", Ref("name"), Ref("instance"), Ref("operatorName"),
                    Ref("operatorNum"), Ref("params"), REF_AGENTINSTANCECONTEXT);
            }

            var contextPartition = CodegenMethod.MakeParentNode(typeof(void), GetType(), classScope)
                .AddParam(typeof(bool), "allocate").AddParam(
                    typeof(AgentInstanceContext), REF_AGENTINSTANCECONTEXT.Ref);
            anonymousClass.AddMethod("contextPartition", contextPartition);
            if (AuditEnum.CONTEXTPARTITION.GetAudit(_annotations) != null) {
                contextPartition.Block.StaticMethod(
                    typeof(AuditPath), "auditContextPartition", Ref("allocate"), REF_AGENTINSTANCECONTEXT);
            }

            return anonymousClass;
        }

        private CodegenExpression MakeProperties(
            IDictionary<StatementProperty, object> properties, CodegenMethodScope parent, CodegenClassScope classScope)
        {
            if (properties.IsEmpty()) {
                return StaticMethod(typeof(Collections), "emptyMap");
            }

            Func<StatementProperty, CodegenExpression> field = x => EnumValue(typeof(StatementProperty), x.GetName());
            Func<object, CodegenExpression> value = Constant;
            if (properties.Count == 1) {
                var first = properties.First();
                return StaticMethod(
                    typeof(Collections), "singletonMap", field.Invoke(first.Key), value.Invoke(first.Value));
            }

            var method = parent.MakeChild(
                typeof(IDictionary<object, object>), typeof(StatementInformationalsCompileTime), classScope);
            method.Block
                .DeclareVar(
                    typeof(IDictionary<object, object>), "properties",
                    NewInstance(
                        typeof(Dictionary<object, object>),
                        Constant(CollectionUtil.CapacityHashMap(properties.Count))));
            foreach (var entry in properties) {
                method.Block.ExprDotMethod(
                    Ref("properties"), "put", field.Invoke(entry.Key), value.Invoke(entry.Value));
            }

            return LocalMethod(method);
        }
    }
} // end of namespace