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

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.annotation;
using com.espertech.esper.common.client.dataflow.core;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.core;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.bytecodemodel.util;
using com.espertech.esper.common.@internal.compile.multikey;
using com.espertech.esper.common.@internal.compile.stage1.spec;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.pattern.core;
using com.espertech.esper.common.@internal.filterspec;
using com.espertech.esper.common.@internal.metrics.audit;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.common.@internal.schedule;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.common.@internal.util.serde;
using com.espertech.esper.common.@internal.view.core;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;
using static com.espertech.esper.common.@internal.epl.annotation.AnnotationUtil;
using static com.espertech.esper.common.@internal.epl.expression.codegen.ExprForgeCodegenNames;
using static com.espertech.esper.common.@internal.epl.output.core.OutputProcessViewCodegenNames;

namespace com.espertech.esper.common.@internal.context.module
{
    public class StatementInformationalsCompileTime
    {
        private readonly string _statementNameCompileTime;
        private readonly bool _alwaysSynthesizeOutputEvents; // set when insert-into/for-clause/select-distinct
        private readonly string _optionalContextName;
        private readonly string _optionalContextModuleName;
        private readonly NameAccessModifier? _optionalContextVisibility;
        private readonly bool _canSelfJoin;
        private readonly bool _hasSubquery;
        private readonly bool _needDedup;
        private readonly Attribute[] _annotations;
        private readonly bool _stateless;
        private readonly object _userObjectCompileTime;
        private readonly int _numFilterCallbacks;
        private readonly int _numScheduleCallbacks;
        private readonly int _numNamedWindowCallbacks;
        private readonly StatementType _statementType;
        private readonly int _priority;
        private readonly bool _preemptive;
        private readonly bool _hasVariables;
        private readonly bool _writesToTables;
        private readonly bool _hasTableAccess;
        private readonly Type[] _selectClauseTypes;
        private readonly string[] _selectClauseColumnNames;
        private readonly bool _forClauseDelivery;
        private readonly ExprNode[] _groupDelivery;
        private readonly MultiKeyClassRef _groupDeliveryMultiKey;
        private readonly IDictionary<StatementProperty, object> _properties;
        private readonly bool _hasMatchRecognize;
        private readonly bool _instrumented;
        private readonly CodegenNamespaceScope _namespaceScope;
        private readonly string _insertIntoLatchName;
        private readonly bool _allowSubscriber;
        private readonly ExpressionScriptProvided[] _onScripts;

        private readonly IContainer _container;
        
        public StatementInformationalsCompileTime(
            IContainer container,
            string statementNameCompileTime,
            bool alwaysSynthesizeOutputEvents,
            string optionalContextName,
            string optionalContextModuleName,
            NameAccessModifier? optionalContextVisibility,
            bool canSelfJoin,
            bool hasSubquery,
            bool needDedup,
            Attribute[] annotations,
            bool stateless,
            object userObjectCompileTime,
            int numFilterCallbacks,
            int numScheduleCallbacks,
            int numNamedWindowCallbacks,
            StatementType statementType,
            int priority,
            bool preemptive,
            bool hasVariables,
            bool writesToTables,
            bool hasTableAccess,
            Type[] selectClauseTypes,
            string[] selectClauseColumnNames,
            bool forClauseDelivery,
            ExprNode[] groupDelivery,
            MultiKeyClassRef groupDeliveryMultiKey,
            IDictionary<StatementProperty, object> properties,
            bool hasMatchRecognize,
            bool instrumented,
            CodegenNamespaceScope namespaceScope,
            string insertIntoLatchName,
            bool allowSubscriber,
            ExpressionScriptProvided[] onScripts)
        {
            _container = container;
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
            _groupDeliveryMultiKey = groupDeliveryMultiKey;
            _properties = properties;
            _hasMatchRecognize = hasMatchRecognize;
            _instrumented = instrumented;
            _namespaceScope = namespaceScope;
            _insertIntoLatchName = insertIntoLatchName;
            _allowSubscriber = allowSubscriber;
            _onScripts = onScripts;
        }

        public CodegenExpression Make(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var method = parent.MakeChild(typeof(StatementInformationalsRuntime), GetType(), classScope);
            var builder = new CodegenSetterBuilder(
                typeof(StatementInformationalsRuntime),
                typeof(StatementInformationalsCompileTime),
                "info",
                classScope,
                method);

            var annotationsExpr = _annotations == null
                ? ConstantNull()
                : MakeAnnotations(typeof(Attribute[]), _annotations, method, classScope);
            
            builder
                .ConstantDefaultCheckedObj("StatementNameCompileTime", _statementNameCompileTime)
                .ConstantDefaultChecked("IsAlwaysSynthesizeOutputEvents", _alwaysSynthesizeOutputEvents)
                .ConstantDefaultCheckedObj("OptionalContextName", _optionalContextName)
                .ConstantDefaultCheckedObj("OptionalContextModuleName", _optionalContextModuleName)
                .ConstantDefaultCheckedObj("OptionalContextVisibility", _optionalContextVisibility)
                .ConstantDefaultChecked("IsCanSelfJoin", _canSelfJoin)
                .ConstantDefaultChecked("HasSubquery", _hasSubquery)
                .ConstantDefaultChecked("IsNeedDedup", _needDedup)
                .ConstantDefaultChecked("IsStateless", _stateless)
                .ConstantDefaultChecked("NumFilterCallbacks", _numFilterCallbacks)
                .ConstantDefaultChecked("NumScheduleCallbacks", _numScheduleCallbacks)
                .ConstantDefaultChecked("NumNamedWindowCallbacks", _numNamedWindowCallbacks)
                .ConstantDefaultCheckedObj("StatementType", _statementType)
                .ConstantDefaultChecked("Priority", _priority)
                .ConstantDefaultChecked("IsPreemptive", _preemptive)
                .ConstantDefaultChecked("HasVariables", _hasVariables)
                .ConstantDefaultChecked("IsWritesToTables", _writesToTables)
                .ConstantDefaultChecked("HasTableAccess", _hasTableAccess)
                .ConstantDefaultCheckedObj("SelectClauseTypes", _selectClauseTypes)
                .ConstantDefaultCheckedObj("SelectClauseColumnNames", _selectClauseColumnNames)
                .ConstantDefaultChecked("IsForClauseDelivery", _forClauseDelivery)
                .ConstantDefaultChecked("HasMatchRecognize", _hasMatchRecognize)
                .ConstantDefaultChecked("IsInstrumented", _instrumented)
                .ConstantDefaultCheckedObj("InsertIntoLatchName", _insertIntoLatchName)
                .ConstantDefaultChecked("IsAllowSubscriber", _allowSubscriber)
                .ExpressionDefaultChecked("Annotations", annotationsExpr)
                .ExpressionDefaultChecked("UserObjectCompileTime", SerializerUtil.ExpressionForUserObject(_container.SerializerFactory(), _userObjectCompileTime))
                .ExpressionDefaultChecked("GroupDeliveryEval", MultiKeyCodegen.CodegenExprEvaluatorMayMultikey(_groupDelivery, null, _groupDeliveryMultiKey, method, classScope))
                .ExpressionDefaultChecked("Properties", MakeProperties(_properties, method, classScope))
                .ExpressionDefaultChecked("AuditProvider", MakeAuditProvider(method, classScope))
                .ExpressionDefaultChecked("InstrumentationProvider", MakeInstrumentationProvider(method, classScope))
                .ExpressionDefaultChecked("SubstitutionParamTypes", MakeSubstitutionParamTypes())
                .ExpressionDefaultChecked("SubstitutionParamNames", MakeSubstitutionParamNames(method, classScope))
                .ExpressionDefaultChecked("OnScripts", MakeOnScripts(_onScripts, method, classScope));

            method.Block.MethodReturn(builder.RefName);
            return LocalMethod(method);
        }

        public IDictionary<StatementProperty, object> Properties => _properties;

        private CodegenExpression MakeSubstitutionParamTypes()
        {
            var numbered = _namespaceScope.SubstitutionParamsByNumber;
            var named = _namespaceScope.SubstitutionParamsByName;
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
                    types[i] = numbered[i].EntryType;
                }
            }
            else {
                types = new Type[named.Count];
                var count = 0;
                foreach (var entry in named) {
                    types[count++] = entry.Value.EntryType;
                }
            }

            return Constant(types);
        }

        private CodegenExpression MakeSubstitutionParamNames(
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            var named = _namespaceScope.SubstitutionParamsByName;
            if (named.IsEmpty()) {
                return ConstantNull();
            }

            var method = parent.MakeChild(typeof(IDictionary<string, int>), GetType(), classScope);
            method.Block.DeclareVar<IDictionary<string, int>>(
                "names",
                NewInstance(typeof(Dictionary<string, int>), Constant(CollectionUtil.CapacityHashMap(named.Count))));
            new CodegenRepetitiveValueBuilder<string>(named.Keys, method, classScope, GetType())
                .AddParam(typeof(IDictionary<string, int>), "names")
                .SetConsumer((value, index, leaf) => leaf.Block.ExprDotMethod(Ref("names"), "Put", Constant(value), Constant(index + 1)))
                .Build();

            method.Block.MethodReturn(Ref("names"));
            return LocalMethod(method);
        }

        private CodegenExpression MakeInstrumentationProvider(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            if (!_instrumented) {
                return ConstantNull();
            }

            var instrumentation = Ref("instrumentation");
            method.Block.AssignRef(instrumentation, NewInstance<ProxyInstrumentationCommon>());

            //CodegenExpressionNewAnonymousClass anonymousClass = NewAnonymousClass(
            //	method.Block,
            //	typeof(InstrumentationCommon));

            //var activated = CodegenMethod.MakeParentNode(typeof(bool), GetType(), classScope);
            //anonymousClass.AddMethod("activated", activated);
            //activated.Block.MethodReturn(ConstantTrue());

            method.Block.SetProperty(
                instrumentation,
                "ProcActivated",
                new CodegenExpressionLambda(method.Block)
                    .WithBody(
                        block => block.BlockReturn(
                            ConstantTrue())));

            foreach (var forwarded in typeof(InstrumentationCommon).GetMethods()) {
                if (forwarded.DeclaringType == typeof(object)) {
                    continue;
                }

                if (forwarded.Name == "Activated") {
                    continue;
                }

                IList<CodegenNamedParam> @params = new List<CodegenNamedParam>();
                var forwardedParameters = forwarded.GetParameters();
                var expressions = new CodegenExpression[forwardedParameters.Length];

                var num = 0;
                foreach (var param in forwardedParameters) {
                    @params.Add(new CodegenNamedParam(param.ParameterType, param.Name));
                    expressions[num] = Ref(param.Name);
                    num++;
                }

                //var m = CodegenMethod.MakeParentNode(typeof(void), GetType(), classScope)
                //	.AddParam(@params);

                // Now we need a lambda to associate with the instrumentation and tie them together
                var proc = $"Proc{forwarded.Name}";

                method.Block.SetProperty(
                    instrumentation,
                    "ProcActivated",
                    new CodegenExpressionLambda(method.Block)
                        .WithParams(@params)
                        .WithBody(
                            block => block
                                .Apply(
                                    InstrumentationCode.Instblock(
                                        classScope,
                                        forwarded.Name,
                                        expressions))));

                //anonymousClass.AddMethod(forwarded.Name, m);
                //m.Block.Apply(InstrumentationCode.Instblock(classScope, forwarded.Name, expressions));
            }

            return instrumentation;
        }

        private CodegenExpression MakeAuditProvider(
            CodegenMethod method,
            CodegenClassScope classScope)
        {
            if (!HasAnnotation<AuditAttribute>(_annotations)) {
                return ConstantNull();
            }

            var auditProviderVar = method.Block.DeclareVar<ProxyAuditProvider>(
                "auditProvider",
                NewInstance<ProxyAuditProvider>());

            // var anonymousClass = NewAnonymousClass(method.Block, typeof(AuditProvider));

            //var activated = CodegenMethod.MakeParentNode(typeof(bool), GetType(), classScope);
            //anonymousClass.AddMethod("activated", activated);
            //activated.Block.MethodReturn(ConstantTrue());

            method.Block.SetProperty(
                Ref("auditProvider"),
                "ProcActivated",
                new CodegenExpressionLambda(method.Block)
                    .WithBody(block => block.BlockReturn(ConstantTrue())));

            var lambdaView = new CodegenExpressionLambda(method.Block)
                .WithParam<EventBean[]>("newData")
                .WithParam<EventBean[]>("oldData")
                .WithParam<AgentInstanceContext>(MEMBER_AGENTINSTANCECONTEXT.Ref)
                .WithParam<ViewFactory>("viewFactory");

            if (AuditEnum.VIEW.GetAudit(_annotations) != null) {
                lambdaView = lambdaView.WithBody(
                    block => block.StaticMethod(
                        typeof(AuditPath),
                        "AuditView",
                        Ref("newData"),
                        Ref("oldData"),
                        MEMBER_AGENTINSTANCECONTEXT,
                        Ref("viewFactory")));
            }

            method.Block.SetProperty(Ref("auditProvider"), "ProcView", lambdaView);

            var lambdaStreamSingle = new CodegenExpressionLambda(method.Block)
                .WithParam<EventBean>("@event")
                .WithParam<ExprEvaluatorContext>(REF_EXPREVALCONTEXT.Ref)
                .WithParam<string>("filterText");

            var lambdaStreamMulti = new CodegenExpressionLambda(method.Block)
                .WithParam<EventBean[]>("newData")
                .WithParam<EventBean[]>("oldData")
                .WithParam<ExprEvaluatorContext>(REF_EXPREVALCONTEXT.Ref)
                .WithParam<string>("filterText");
            
            if (AuditEnum.STREAM.GetAudit(_annotations) != null) {
                lambdaStreamSingle = lambdaStreamSingle.WithBody(
                    block => block.StaticMethod(
                        typeof(AuditPath),
                        "AuditStream",
                        Ref("@event"),
                        REF_EXPREVALCONTEXT,
                        Ref("filterText")));

                lambdaStreamMulti = lambdaStreamMulti.WithBody(
                    block => block.StaticMethod(
                        typeof(AuditPath),
                        "AuditStream",
                        Ref("newData"),
                        Ref("oldData"),
                        REF_EXPREVALCONTEXT,
                        Ref("filterText")));
            }

            method.Block.SetProperty(Ref("auditProvider"), "ProcStreamSingle", lambdaStreamSingle);
            method.Block.SetProperty(Ref("auditProvider"), "ProcStreamMulti", lambdaStreamMulti);

            var lambdaScheduleAdd = new CodegenExpressionLambda(method.Block)
                .WithParam<long>("time")
                .WithParam<AgentInstanceContext>(MEMBER_AGENTINSTANCECONTEXT.Ref)
                .WithParam<ScheduleHandle>("scheduleHandle")
                .WithParam<ScheduleObjectType>("type")
                .WithParam<string>("name");

            var lambdaScheduleRemove = new CodegenExpressionLambda(method.Block)
                .WithParam<AgentInstanceContext>(MEMBER_AGENTINSTANCECONTEXT.Ref)
                .WithParam<ScheduleHandle>("scheduleHandle")
                .WithParam<ScheduleObjectType>("type")
                .WithParam<string>("name");

            var lambdaScheduleFire = new CodegenExpressionLambda(method.Block)
                .WithParam<AgentInstanceContext>(MEMBER_AGENTINSTANCECONTEXT.Ref)
                .WithParam<ScheduleObjectType>("type")
                .WithParam<string>("name");
            
            if (AuditEnum.SCHEDULE.GetAudit(_annotations) != null) {
                lambdaScheduleAdd = lambdaScheduleAdd.WithBody(
                    block => block.StaticMethod(
                        typeof(AuditPath),
                        "AuditScheduleAdd",
                        Ref("time"),
                        MEMBER_AGENTINSTANCECONTEXT,
                        Ref("scheduleHandle"),
                        Ref("type"),
                        Ref("name")));

                lambdaScheduleRemove = lambdaScheduleRemove.WithBody(
                    block => block.StaticMethod(
                        typeof(AuditPath),
                        "AuditScheduleRemove",
                        MEMBER_AGENTINSTANCECONTEXT,
                        Ref("scheduleHandle"),
                        Ref("type"),
                        Ref("name")));

                lambdaScheduleFire = lambdaScheduleFire.WithBody(
                    block => block.StaticMethod(
                        typeof(AuditPath),
                        "AuditScheduleFire",
                        MEMBER_AGENTINSTANCECONTEXT,
                        Ref("type"),
                        Ref("name")));
            }

            method.Block.SetProperty(Ref("auditProvider"), "ProcScheduleAdd", lambdaScheduleAdd);
            method.Block.SetProperty(Ref("auditProvider"), "ProcScheduleRemove", lambdaScheduleRemove);
            method.Block.SetProperty(Ref("auditProvider"), "ProcScheduleFire", lambdaScheduleFire);

            var lambdaProperty = new CodegenExpressionLambda(method.Block)
                .WithParam<string>("name")
                .WithParam<object>("value")
                .WithParam<ExprEvaluatorContext>(REF_EXPREVALCONTEXT.Ref);

            if (AuditEnum.PROPERTY.GetAudit(_annotations) != null) {
                lambdaProperty = lambdaProperty.WithBody(
                    block => block.StaticMethod(
                        typeof(AuditPath),
                        "AuditProperty",
                        Ref("name"),
                        Ref("value"),
                        REF_EXPREVALCONTEXT));
            }
            
            method.Block.SetProperty(Ref("auditProvider"), "ProcProperty", lambdaProperty);

            var lambdaInsert = new CodegenExpressionLambda(method.Block)
                .WithParam<EventBean>("@event")
                .WithParam<ExprEvaluatorContext>(REF_EXPREVALCONTEXT.Ref);

            if (AuditEnum.INSERT.GetAudit(_annotations) != null) {
                lambdaInsert = lambdaInsert.WithBody(
                    block => block.StaticMethod(
                        typeof(AuditPath),
                        "AuditInsert",
                        Ref("@event"),
                        REF_EXPREVALCONTEXT));
            }

            method.Block.SetProperty(Ref("auditProvider"), "ProcInsert", lambdaInsert);

            var lambdaExpression = new CodegenExpressionLambda(method.Block)
                .WithParam<string>("text")
                .WithParam<object>("value")
                .WithParam<ExprEvaluatorContext>(REF_EXPREVALCONTEXT.Ref);

            if (AuditEnum.EXPRESSION.GetAudit(_annotations) != null ||
                AuditEnum.EXPRESSION_NESTED.GetAudit(_annotations) != null) {
                lambdaExpression = lambdaExpression.WithBody(
                    block => block.StaticMethod(
                        typeof(AuditPath),
                        "AuditExpression",
                        Ref("text"),
                        Ref("value"),
                        REF_EXPREVALCONTEXT));
            }

            method.Block.SetProperty(Ref("auditProvider"), "ProcExpression", lambdaExpression);

            var lambdaPatternTrue = new CodegenExpressionLambda(method.Block)
                .WithParam<EvalFactoryNode>("factoryNode")
                .WithParam<object>("from")
                .WithParam<MatchedEventMapMinimal>("matchEvent")
                .WithParam<bool>("isQuitted")
                .WithParam<AgentInstanceContext>(NAME_AGENTINSTANCECONTEXT);

            var lambdaPatternFalse = new CodegenExpressionLambda(method.Block)
                .WithParam<EvalFactoryNode>("factoryNode")
                .WithParam<object>("from")
                .WithParam<AgentInstanceContext>(NAME_AGENTINSTANCECONTEXT);
            
            if (AuditEnum.PATTERN.GetAudit(_annotations) != null) {
                lambdaPatternTrue = lambdaPatternTrue.WithBody(
                    block => block.StaticMethod(
                        typeof(AuditPath),
                        "AuditPatternTrue",
                        Ref("factoryNode"),
                        Ref("from"),
                        Ref("matchEvent"),
                        Ref("isQuitted"),
                        MEMBER_AGENTINSTANCECONTEXT));

                lambdaPatternFalse = lambdaPatternFalse.WithBody(
                    block => block.StaticMethod(
                        typeof(AuditPath),
                        "AuditPatternFalse",
                        Ref("factoryNode"),
                        Ref("from"),
                        MEMBER_AGENTINSTANCECONTEXT));
            }

            method.Block.SetProperty(Ref("auditProvider"), "ProcPatternTrue", lambdaPatternTrue);
            method.Block.SetProperty(Ref("auditProvider"), "ProcPatternFalse", lambdaPatternFalse);

            var lambdaPatternInstance = new CodegenExpressionLambda(method.Block)
                .WithParam<bool>("increase")
                .WithParam<EvalFactoryNode>("factoryNode")
                .WithParam<AgentInstanceContext>(NAME_AGENTINSTANCECONTEXT);
            
            if (AuditEnum.PATTERNINSTANCES.GetAudit(_annotations) != null) {
                lambdaPatternInstance = lambdaPatternInstance.WithBody(
                    block => block.StaticMethod(
                        typeof(AuditPath),
                        "AuditPatternInstance",
                        Ref("increase"),
                        Ref("factoryNode"),
                        MEMBER_AGENTINSTANCECONTEXT));
            }

            method.Block.SetProperty(Ref("auditProvider"), "ProcPatternInstance", lambdaPatternInstance);

            var lambdaExprdef = new CodegenExpressionLambda(method.Block)
                .WithParam<string>("name")
                .WithParam<object>("value")
                .WithParam<ExprEvaluatorContext>(REF_EXPREVALCONTEXT.Ref);
            
            if (AuditEnum.EXPRDEF.GetAudit(_annotations) != null) {
                lambdaExprdef = lambdaExprdef.WithBody(
                    block => block.StaticMethod(
                        typeof(AuditPath),
                        "AuditExprDef",
                        Ref("name"),
                        Ref("value"),
                        REF_EXPREVALCONTEXT));
            }
            
            method.Block.SetProperty(Ref("auditProvider"), "ProcExprdef", lambdaExprdef);

            var lambdaDataflowTransition = new CodegenExpressionLambda(method.Block)
                .WithParam<string>("name")
                .WithParam<string>("instance")
                .WithParam<EPDataFlowState>("state")
                .WithParam<EPDataFlowState>("newState")
                .WithParam<AgentInstanceContext>(MEMBER_AGENTINSTANCECONTEXT.Ref);

            if (AuditEnum.DATAFLOW_TRANSITION.GetAudit(_annotations) != null) {
                lambdaDataflowTransition = lambdaDataflowTransition.WithBody(
                    block => block.StaticMethod(
                        typeof(AuditPath),
                        "AuditDataflowTransition",
                        Ref("name"),
                        Ref("instance"),
                        Ref("state"),
                        Ref("newState"),
                        MEMBER_AGENTINSTANCECONTEXT));
            }

            method.Block.SetProperty(Ref("auditProvider"), "ProcDataflowTransition", lambdaDataflowTransition);

            var lambdaDataflowSource = new CodegenExpressionLambda(method.Block)
                .WithParam<string>("name")
                .WithParam<string>("instance")
                .WithParam<string>("operatorName")
                .WithParam<int>("operatorNum")
                .WithParam<AgentInstanceContext>(MEMBER_AGENTINSTANCECONTEXT.Ref);

            if (AuditEnum.DATAFLOW_SOURCE.GetAudit(_annotations) != null) {
                lambdaDataflowSource = lambdaDataflowSource.WithBody(
                    block => block.StaticMethod(
                        typeof(AuditPath),
                        "AuditDataflowSource",
                        Ref("name"),
                        Ref("instance"),
                        Ref("operatorName"),
                        Ref("operatorNum"),
                        MEMBER_AGENTINSTANCECONTEXT));
            }
            
            method.Block.SetProperty(Ref("auditProvider"), "ProcDataflowSource", lambdaDataflowSource);

            var lambdaDataflowOp = new CodegenExpressionLambda(method.Block)
                .WithParam<string>("name")
                .WithParam<string>("instance")
                .WithParam<string>("operatorName")
                .WithParam<int>("operatorNum")
                .WithParam<object[]>("parameters")
                .WithParam<AgentInstanceContext>(MEMBER_AGENTINSTANCECONTEXT.Ref);
            
            if (AuditEnum.DATAFLOW_OP.GetAudit(_annotations) != null) {
                lambdaDataflowOp = lambdaDataflowOp.WithBody(
                    block => block.StaticMethod(
                        typeof(AuditPath),
                        "AuditDataflowOp",
                        Ref("name"),
                        Ref("instance"),
                        Ref("operatorName"),
                        Ref("operatorNum"),
                        Ref("parameters"),
                        MEMBER_AGENTINSTANCECONTEXT));
            }
            
            method.Block.SetProperty(Ref("auditProvider"), "ProcDataflowOp", lambdaDataflowOp);

            var lambdaContextPartition = new CodegenExpressionLambda(method.Block)
                .WithParam<bool>("allocate")
                .WithParam<AgentInstanceContext>(MEMBER_AGENTINSTANCECONTEXT.Ref);

            if (AuditEnum.CONTEXTPARTITION.GetAudit(_annotations) != null) {
                lambdaContextPartition = lambdaContextPartition.WithBody(
                    block => block.StaticMethod(
                        typeof(AuditPath),
                        "AuditContextPartition",
                        Ref("allocate"),
                        MEMBER_AGENTINSTANCECONTEXT));
            }
            
            method.Block.SetProperty(Ref("auditProvider"), "ProcContextPartition", lambdaContextPartition);

            return Ref("auditProvider");
        }

        private CodegenExpression MakeProperties(
            IDictionary<StatementProperty, object> properties,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            if (properties.IsEmpty()) {
                return StaticMethod(typeof(Collections), "GetEmptyDataMap");
            }

            Func<StatementProperty, CodegenExpression> field = x => EnumValue(typeof(StatementProperty), x.GetName());
            Func<object, CodegenExpression> value = Constant;
            if (properties.Count == 1) {
                var first = properties.First();
                return StaticMethod(
                    typeof(Collections),
                    "SingletonMap",
                    field.Invoke(first.Key),
                    Cast(typeof(object), value.Invoke(first.Value)));
            }

            var method = parent.MakeChild(
                typeof(IDictionary<StatementProperty, object>),
                typeof(StatementInformationalsCompileTime),
                classScope);
            method.Block
                .DeclareVar<IDictionary<StatementProperty, object>>(
                    "properties",
                    NewInstance<Dictionary<StatementProperty, object>>());
            foreach (var entry in properties) {
                method.Block.ExprDotMethod(
                    Ref("properties"),
                    "Put",
                    field.Invoke(entry.Key),
                    value.Invoke(entry.Value));
            }

            method.Block.MethodReturn(Ref("properties"));
            return LocalMethod(method);
        }

        private CodegenExpression MakeOnScripts(
            ExpressionScriptProvided[] onScripts,
            CodegenMethodScope parent,
            CodegenClassScope classScope)
        {
            if (onScripts == null || onScripts.Length == 0) {
                return ConstantNull();
            }

            var init = new CodegenExpression[onScripts.Length];
            for (var i = 0; i < onScripts.Length; i++) {
                init[i] = onScripts[i].Make(parent, classScope);
            }

            return NewArrayWithInit(typeof(ExpressionScriptProvided), init);
        }
    }
} // end of namespace