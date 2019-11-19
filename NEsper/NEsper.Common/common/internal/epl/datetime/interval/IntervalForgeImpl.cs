///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.datetime.eval;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.epl.expression.dot.core;
using com.espertech.esper.common.@internal.epl.expression.time.abacus;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.epl.table.compiletime;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.datetime.interval
{
    public class IntervalForgeImpl : IntervalForge
    {
        private readonly string parameterPropertyEnd;
        private readonly string parameterPropertyStart;

        private readonly int parameterStreamNum;

        public IntervalForgeImpl(
            DateTimeMethodEnum method,
            string methodNameUse,
            StreamTypeService streamTypeService,
            IList<ExprNode> expressions,
            TimeAbacus timeAbacus,
            TableCompileTimeResolver tableCompileTimeResolver)
        {
            ExprForge forgeEndTimestamp = null;
            Type timestampType;

            if (expressions[0] is ExprStreamUnderlyingNode) {
                var und = (ExprStreamUnderlyingNode) expressions[0];
                parameterStreamNum = und.StreamId;
                var type = streamTypeService.EventTypes[parameterStreamNum];
                parameterPropertyStart = type.StartTimestampPropertyName;
                if (parameterPropertyStart == null) {
                    throw new ExprValidationException(
                        "For date-time method '" +
                        methodNameUse +
                        "' the first parameter is event type '" +
                        type.Name +
                        "', however no timestamp property has been defined for this event type");
                }

                timestampType = type.GetPropertyType(parameterPropertyStart);
                var getter = ((EventTypeSPI) type).GetGetterSPI(parameterPropertyStart);
                var getterReturnTypeBoxed = type.GetPropertyType(parameterPropertyStart).GetBoxedType();
                ForgeTimestamp = new ExprEvaluatorStreamDTProp(parameterStreamNum, getter, getterReturnTypeBoxed);

                if (type.EndTimestampPropertyName != null) {
                    parameterPropertyEnd = type.EndTimestampPropertyName;
                    var getterEndTimestamp =
                        ((EventTypeSPI) type).GetGetterSPI(type.EndTimestampPropertyName);
                    forgeEndTimestamp = new ExprEvaluatorStreamDTProp(
                        parameterStreamNum,
                        getterEndTimestamp,
                        getterReturnTypeBoxed);
                }
                else {
                    parameterPropertyEnd = parameterPropertyStart;
                }
            }
            else {
                ForgeTimestamp = expressions[0].Forge;
                timestampType = ForgeTimestamp.EvaluationType;

                string unresolvedPropertyName = null;
                if (expressions[0] is ExprIdentNode) {
                    var identNode = (ExprIdentNode) expressions[0];
                    parameterStreamNum = identNode.StreamId;
                    parameterPropertyStart = identNode.ResolvedPropertyName;
                    parameterPropertyEnd = parameterPropertyStart;
                    unresolvedPropertyName = identNode.UnresolvedPropertyName;
                }

                if (!TypeHelper.IsDateTime(ForgeTimestamp.EvaluationType)) {
                    // ident node may represent a fragment
                    if (unresolvedPropertyName != null) {
                        var propertyDesc = ExprIdentNodeUtil.GetTypeFromStream(
                            streamTypeService,
                            unresolvedPropertyName,
                            false,
                            true,
                            tableCompileTimeResolver);
                        if (propertyDesc.First.FragmentEventType != null) {
                            var type = propertyDesc.First.FragmentEventType.FragmentType;
                            parameterPropertyStart = type.StartTimestampPropertyName;
                            if (parameterPropertyStart == null) {
                                throw new ExprValidationException(
                                    "For date-time method '" +
                                    methodNameUse +
                                    "' the first parameter is event type '" +
                                    type.Name +
                                    "', however no timestamp property has been defined for this event type");
                            }

                            timestampType = type.GetPropertyType(parameterPropertyStart);
                            var getterFragment =
                                ((EventTypeSPI) streamTypeService.EventTypes[parameterStreamNum]).GetGetterSPI(
                                    unresolvedPropertyName);
                            var getterStartTimestamp =
                                ((EventTypeSPI) type).GetGetterSPI(parameterPropertyStart);
                            ForgeTimestamp = new ExprEvaluatorStreamDTPropFragment(
                                parameterStreamNum,
                                getterFragment,
                                getterStartTimestamp);

                            if (type.EndTimestampPropertyName != null) {
                                parameterPropertyEnd = type.EndTimestampPropertyName;
                                var getterEndTimestamp =
                                    ((EventTypeSPI) type).GetGetterSPI(type.EndTimestampPropertyName);
                                forgeEndTimestamp = new ExprEvaluatorStreamDTPropFragment(
                                    parameterStreamNum,
                                    getterFragment,
                                    getterEndTimestamp);
                            }
                            else {
                                parameterPropertyEnd = parameterPropertyStart;
                            }
                        }
                    }
                    else {
                        throw new ExprValidationException(
                            "For date-time method '" +
                            methodNameUse +
                            "' the first parameter expression returns '" +
                            ForgeTimestamp.EvaluationType +
                            "', however requires a Date, DateTimeEx, Long-type return value or event (with timestamp)");
                    }
                }
            }

            var intervalComputerForge =
                IntervalComputerForgeFactory.Make(method, expressions, timeAbacus);

            // evaluation without end timestamp
            var timestampTypeBoxed = timestampType.GetBoxedType();
            if (forgeEndTimestamp == null) {
                if (TypeHelper.IsSubclassOrImplementsInterface(timestampType, typeof(DateTimeEx))) {
                    IntervalOpForge = new IntervalOpDateTimeExForge(intervalComputerForge);
                }
                else if (timestampTypeBoxed == typeof(long?)) {
                    IntervalOpForge = new IntervalOpForgeLong(intervalComputerForge);
                }
                else if (timestampTypeBoxed == typeof(DateTimeOffset?)) {
                    IntervalOpForge = new IntervalOpDateTimeOffsetForge(intervalComputerForge);
                }
                else if (timestampTypeBoxed == typeof(DateTime?)) {
                    IntervalOpForge = new IntervalOpDateTimeForge(intervalComputerForge);
                }
                else {
                    throw new ArgumentException("Invalid interval first parameter type '" + timestampType + "'");
                }
            }
            else {
                if (TypeHelper.IsSubclassOrImplementsInterface(timestampType, typeof(DateTimeEx))) {
                    IntervalOpForge = new IntervalOpDateTimeExWithEndForge(intervalComputerForge, forgeEndTimestamp);
                }
                else if (timestampTypeBoxed == typeof(long?)) {
                    IntervalOpForge = new IntervalOpLongWithEndForge(intervalComputerForge, forgeEndTimestamp);
                }
                else if (timestampTypeBoxed == typeof(DateTimeOffset?)) {
                    IntervalOpForge = new IntervalOpDateTimeOffsetWithEndForge(
                        intervalComputerForge,
                        forgeEndTimestamp);
                }
                else if (timestampTypeBoxed == typeof(DateTime?)) {
                    IntervalOpForge = new IntervalOpDateTimeWithEndForge(intervalComputerForge, forgeEndTimestamp);
                }
                else {
                    throw new ArgumentException("Invalid interval first parameter type '" + timestampType + "'");
                }
            }
        }

        public ExprForge ForgeTimestamp { get; }

        public IIntervalOpForge IntervalOpForge { get; }

        public IntervalOp Op => new IntervalForgeOp(ForgeTimestamp.ExprEvaluator, IntervalOpForge.MakeEval());

        public CodegenExpression Codegen(
            CodegenExpression start,
            CodegenExpression end,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return IntervalForgeOp.Codegen(this, start, end, codegenMethodScope, exprSymbol, codegenClassScope);
        }

        /// <summary>
        ///     Obtain information used by filter analyzer to handle this dot-method invocation as part of query planning/indexing.
        /// </summary>
        /// <param name="typesPerStream">event types</param>
        /// <param name="currentMethod">current method</param>
        /// <param name="currentParameters">current params</param>
        /// <param name="inputDesc">descriptor of what the input to this interval method is</param>
        public FilterExprAnalyzerDTIntervalAffector GetFilterDesc(
            EventType[] typesPerStream,
            DateTimeMethodEnum currentMethod,
            IList<ExprNode> currentParameters,
            ExprDotNodeFilterAnalyzerInput inputDesc)
        {
            // with intervals is not currently query planned
            if (currentParameters.Count > 1) {
                return null;
            }

            // Get input (target)
            int targetStreamNum;
            string targetPropertyStart;
            string targetPropertyEnd;
            if (inputDesc is ExprDotNodeFilterAnalyzerInputStream) {
                var targetStream = (ExprDotNodeFilterAnalyzerInputStream) inputDesc;
                targetStreamNum = targetStream.StreamNum;
                var targetType = typesPerStream[targetStreamNum];
                targetPropertyStart = targetType.StartTimestampPropertyName;
                targetPropertyEnd = targetType.EndTimestampPropertyName != null
                    ? targetType.EndTimestampPropertyName
                    : targetPropertyStart;
            }
            else if (inputDesc is ExprDotNodeFilterAnalyzerInputProp) {
                var targetStream = (ExprDotNodeFilterAnalyzerInputProp) inputDesc;
                targetStreamNum = targetStream.StreamNum;
                targetPropertyStart = targetStream.PropertyName;
                targetPropertyEnd = targetStream.PropertyName;
            }
            else {
                return null;
            }

            // check parameter info
            if (parameterPropertyStart == null) {
                return null;
            }

            return new FilterExprAnalyzerDTIntervalAffector(
                currentMethod,
                typesPerStream,
                targetStreamNum,
                targetPropertyStart,
                targetPropertyEnd,
                parameterStreamNum,
                parameterPropertyStart,
                parameterPropertyEnd);
        }

        public interface IIntervalOpForge
        {
            IIntervalOpEval MakeEval();

            CodegenExpression Codegen(
                CodegenExpression start,
                CodegenExpression end,
                CodegenExpression parameter,
                Type parameterType,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope);
        }

        public interface IIntervalOpEval
        {
            object Evaluate(
                long startTs,
                long endTs,
                object parameter,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext context);
        }

        public abstract class IntervalOpForgeBase : IIntervalOpForge
        {
            internal readonly IntervalComputerForge intervalComputer;

            public IntervalOpForgeBase(IntervalComputerForge intervalComputer)
            {
                this.intervalComputer = intervalComputer;
            }

            public abstract IIntervalOpEval MakeEval();

            public abstract CodegenExpression Codegen(
                CodegenExpression start,
                CodegenExpression end,
                CodegenExpression parameter,
                Type parameterType,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope);
        }

        public abstract class IntervalOpEvalBase : IIntervalOpEval
        {
            internal readonly IntervalComputerEval intervalComputer;

            public IntervalOpEvalBase(IntervalComputerEval intervalComputer)
            {
                this.intervalComputer = intervalComputer;
            }

            public abstract object Evaluate(
                long startTs,
                long endTs,
                object parameter,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext context);
        }

        public class IntervalOpForgeLong : IntervalOpForgeBase
        {
            public IntervalOpForgeLong(IntervalComputerForge intervalComputer)
                : base(intervalComputer)
            {
            }

            public override IIntervalOpEval MakeEval()
            {
                return new IntervalOpEvalLong(intervalComputer.MakeComputerEval());
            }

            public override CodegenExpression Codegen(
                CodegenExpression startTs,
                CodegenExpression endTs,
                CodegenExpression parameter,
                Type parameterType,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return IntervalOpEvalLong.Codegen(
                    intervalComputer,
                    startTs,
                    endTs,
                    parameter,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope);
            }
        }

        public class IntervalOpEvalLong : IntervalOpEvalBase
        {
            public IntervalOpEvalLong(IntervalComputerEval intervalComputer)
                : base(intervalComputer)
            {
            }

            public override object Evaluate(
                long startTs,
                long endTs,
                object parameter,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext context)
            {
                var time = parameter.AsLong();
                return intervalComputer.Compute(startTs, endTs, time, time, eventsPerStream, isNewData, context);
            }

            public static CodegenExpression Codegen(
                IntervalComputerForge intervalComputer,
                CodegenExpression startTs,
                CodegenExpression endTs,
                CodegenExpression parameter,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return intervalComputer.Codegen(
                    startTs,
                    endTs,
                    parameter,
                    parameter,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope);
            }
        }

        public class IntervalOpDateTimeExForge : IntervalOpForgeBase
        {
            public IntervalOpDateTimeExForge(IntervalComputerForge intervalComputer)
                : base(intervalComputer)
            {
            }

            public override IIntervalOpEval MakeEval()
            {
                return new IntervalOpDateTimeExEval(intervalComputer.MakeComputerEval());
            }

            public override CodegenExpression Codegen(
                CodegenExpression start,
                CodegenExpression end,
                CodegenExpression parameter,
                Type parameterType,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return IntervalOpDateTimeExEval.Codegen(
                    this,
                    start,
                    end,
                    parameter,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope);
            }
        }

        public class IntervalOpDateTimeExEval : IntervalOpEvalBase
        {
            public IntervalOpDateTimeExEval(IntervalComputerEval intervalComputer)
                : base(intervalComputer)
            {
            }

            public override object Evaluate(
                long startTs,
                long endTs,
                object parameter,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext context)
            {
                var time = ((DateTimeEx) parameter).UtcMillis;
                return intervalComputer.Compute(startTs, endTs, time, time, eventsPerStream, isNewData, context);
            }

            public static CodegenExpression Codegen(
                IntervalOpDateTimeExForge forge,
                CodegenExpression start,
                CodegenExpression end,
                CodegenExpression parameter,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope.MakeChild(
                        typeof(bool?),
                        typeof(IIntervalOpEval),
                        codegenClassScope)
                    .AddParam(typeof(long), "startTs")
                    .AddParam(typeof(long), "endTs")
                    .AddParam(typeof(DateTimeEx), "parameter");

                methodNode.Block
                    .DeclareVar<long>("time", ExprDotName(Ref("parameter"), "UtcMillis"))
                    .MethodReturn(
                        forge.intervalComputer.Codegen(
                            Ref("startTs"),
                            Ref("endTs"),
                            Ref("time"),
                            Ref("time"),
                            methodNode,
                            exprSymbol,
                            codegenClassScope));
                return LocalMethod(methodNode, start, end, parameter);
            }
        }

        public class IntervalOpDateTimeOffsetForge : IntervalOpForgeBase
        {
            public IntervalOpDateTimeOffsetForge(IntervalComputerForge intervalComputer)
                : base(intervalComputer)
            {
            }

            public override IIntervalOpEval MakeEval()
            {
                return new IntervalOpDateTimeOffsetEval(intervalComputer.MakeComputerEval());
            }

            public override CodegenExpression Codegen(
                CodegenExpression start,
                CodegenExpression end,
                CodegenExpression parameter,
                Type parameterType,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return IntervalOpDateTimeOffsetEval.Codegen(
                    this,
                    start,
                    end,
                    parameter,
                    parameterType,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope);
            }
        }

        public class IntervalOpDateTimeOffsetEval : IntervalOpEvalBase
        {
            public IntervalOpDateTimeOffsetEval(IntervalComputerEval intervalComputer)
                : base(intervalComputer)
            {
            }

            public override object Evaluate(
                long startTs,
                long endTs,
                object parameter,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext context)
            {
                var time = DatetimeLongCoercerDateTimeOffset.CoerceToMillis((DateTimeOffset) parameter);
                return intervalComputer.Compute(startTs, endTs, time, time, eventsPerStream, isNewData, context);
            }

            public static CodegenExpression Codegen(
                IntervalOpDateTimeOffsetForge forge,
                CodegenExpression start,
                CodegenExpression end,
                CodegenExpression parameter,
                Type parameterType,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope
                    .MakeChild(typeof(bool?), typeof(IntervalOpDateTimeOffsetEval), codegenClassScope)
                    .AddParam(typeof(long), "startTs")
                    .AddParam(typeof(long), "endTs")
                    .AddParam(typeof(DateTimeOffset), "parameter");

                methodNode.Block
                    .DeclareVar<long>(
                        "time",
                        StaticMethod(
                            typeof(DatetimeLongCoercerDateTimeOffset),
                            "CoerceToMillis",
                            Ref("parameter")))
                    .MethodReturn(
                        forge.intervalComputer.Codegen(
                            Ref("startTs"),
                            Ref("endTs"),
                            Ref("time"),
                            Ref("time"),
                            methodNode,
                            exprSymbol,
                            codegenClassScope));
                return LocalMethod(methodNode, start, end, parameter);
            }
        }

        public class IntervalOpDateTimeForge : IntervalOpForgeBase
        {
            public IntervalOpDateTimeForge(IntervalComputerForge intervalComputer)
                : base(intervalComputer)
            {
            }

            public override IIntervalOpEval MakeEval()
            {
                return new IntervalOpDateTimeEval(intervalComputer.MakeComputerEval());
            }

            public override CodegenExpression Codegen(
                CodegenExpression start,
                CodegenExpression end,
                CodegenExpression parameter,
                Type parameterType,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return IntervalOpDateTimeEval.Codegen(
                    this,
                    start,
                    end,
                    parameter,
                    codegenMethodScope,
                    exprSymbol,
                    codegenClassScope);
            }
        }

        public class IntervalOpDateTimeEval : IntervalOpEvalBase
        {
            public IntervalOpDateTimeEval(IntervalComputerEval intervalComputer)
                : base(intervalComputer)
            {
            }

            public override object Evaluate(
                long startTs,
                long endTs,
                object parameter,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext context)
            {
                var time = DatetimeLongCoercerDateTime.CoerceToMillis((DateTime) parameter);
                return intervalComputer.Compute(startTs, endTs, time, time, eventsPerStream, isNewData, context);
            }

            public static CodegenExpression Codegen(
                IntervalOpDateTimeForge forge,
                CodegenExpression start,
                CodegenExpression end,
                CodegenExpression parameter,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope
                    .MakeChild(typeof(bool?), typeof(IntervalOpDateTimeEval), codegenClassScope)
                    .AddParam(typeof(long), "startTs")
                    .AddParam(typeof(long), "endTs")
                    .AddParam(typeof(DateTime), "parameter");

                methodNode.Block
                    .DeclareVar<long>(
                        "time",
                        StaticMethod(typeof(DatetimeLongCoercerDateTime), "CoerceToMillis", Ref("parameter")))
                    .MethodReturn(
                        forge.intervalComputer.Codegen(
                            Ref("startTs"),
                            Ref("endTs"),
                            Ref("time"),
                            Ref("time"),
                            methodNode,
                            exprSymbol,
                            codegenClassScope));
                return LocalMethod(methodNode, start, end, parameter);
            }
        }

        public abstract class IntervalOpForgeDateWithEndBase : IIntervalOpForge
        {
            protected internal readonly ExprForge forgeEndTimestamp;
            protected internal readonly IntervalComputerForge intervalComputer;

            public IntervalOpForgeDateWithEndBase(
                IntervalComputerForge intervalComputer,
                ExprForge forgeEndTimestamp)
            {
                this.intervalComputer = intervalComputer;
                this.forgeEndTimestamp = forgeEndTimestamp;
            }

            public abstract IIntervalOpEval MakeEval();

            public virtual CodegenExpression Codegen(
                CodegenExpression start,
                CodegenExpression end,
                CodegenExpression parameter,
                Type parameterType,
                CodegenMethodScope codegenMethodScope,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                var methodNode = codegenMethodScope
                    .MakeChild(typeof(bool?), typeof(IntervalOpForgeDateWithEndBase), codegenClassScope)
                    .AddParam(typeof(long), "startTs")
                    .AddParam(typeof(long), "endTs")
                    .AddParam(parameterType, "paramStartTs");

                var evaluationType = forgeEndTimestamp.EvaluationType;
                methodNode.Block.DeclareVar(
                    evaluationType,
                    "paramEndTs",
                    forgeEndTimestamp.EvaluateCodegen(evaluationType, methodNode, exprSymbol, codegenClassScope));
                if (evaluationType.CanBeNull()) {
                    methodNode.Block.IfRefNullReturnNull("paramEndTs");
                }

                var expression = CodegenEvaluate(
                    Ref("startTs"),
                    Ref("endTs"),
                    Ref("paramStartTs"),
                    Ref("paramEndTs"),
                    methodNode,
                    exprSymbol,
                    codegenClassScope);
                methodNode.Block.MethodReturn(expression);
                
                return LocalMethod(methodNode, start, end, parameter);
            }

            protected abstract CodegenExpression CodegenEvaluate(
                CodegenExpressionRef startTs,
                CodegenExpressionRef endTs,
                CodegenExpressionRef paramStartTs,
                CodegenExpressionRef paramEndTs,
                CodegenMethod parentNode,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope);
        }

        public abstract class IntervalOpEvalDateWithEndBase : IIntervalOpEval
        {
            protected internal readonly ExprEvaluator evaluatorEndTimestamp;
            protected internal readonly IntervalComputerEval intervalComputer;

            protected IntervalOpEvalDateWithEndBase(
                IntervalComputerEval intervalComputer,
                ExprEvaluator evaluatorEndTimestamp)
            {
                this.intervalComputer = intervalComputer;
                this.evaluatorEndTimestamp = evaluatorEndTimestamp;
            }

            public virtual object Evaluate(
                long startTs,
                long endTs,
                object parameterStartTs,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext context)
            {
                var paramEndTs = evaluatorEndTimestamp.Evaluate(eventsPerStream, isNewData, context);
                if (paramEndTs == null) {
                    return null;
                }

                return Evaluate(startTs, endTs, parameterStartTs, paramEndTs, eventsPerStream, isNewData, context);
            }

            public abstract object Evaluate(
                long startTs,
                long endTs,
                object parameterStartTs,
                object parameterEndTs,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext context);
        }

        public class IntervalOpLongWithEndForge : IntervalOpForgeDateWithEndBase
        {
            public IntervalOpLongWithEndForge(
                IntervalComputerForge intervalComputer,
                ExprForge evaluatorEndTimestamp)
                : base(intervalComputer, evaluatorEndTimestamp)
            {
            }

            public override IIntervalOpEval MakeEval()
            {
                return new IntervalOpLongWithEndEval(
                    intervalComputer.MakeComputerEval(),
                    forgeEndTimestamp.ExprEvaluator);
            }

            protected override CodegenExpression CodegenEvaluate(
                CodegenExpressionRef startTs,
                CodegenExpressionRef endTs,
                CodegenExpressionRef paramStartTs,
                CodegenExpressionRef paramEndTs,
                CodegenMethod parentNode,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return intervalComputer.Codegen(
                    startTs,
                    endTs,
                    paramStartTs,
                    paramEndTs,
                    parentNode,
                    exprSymbol,
                    codegenClassScope);
            }
        }

        public class IntervalOpLongWithEndEval : IntervalOpEvalDateWithEndBase
        {
            public IntervalOpLongWithEndEval(
                IntervalComputerEval intervalComputer,
                ExprEvaluator evaluatorEndTimestamp)
                : base(intervalComputer, evaluatorEndTimestamp)

            {
            }

            public override object Evaluate(
                long startTs,
                long endTs,
                object parameterStartTs,
                object parameterEndTs,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext context)
            {
                return intervalComputer.Compute(
                    startTs,
                    endTs,
                    parameterStartTs.AsLong(),
                    parameterEndTs.AsLong(),
                    eventsPerStream,
                    isNewData,
                    context);
            }
        }

        public class IntervalOpDateTimeExWithEndForge : IntervalOpForgeDateWithEndBase
        {
            public IntervalOpDateTimeExWithEndForge(
                IntervalComputerForge intervalComputer,
                ExprForge forgeEndTimestamp)
                : base(intervalComputer, forgeEndTimestamp)
            {
            }

            public override IIntervalOpEval MakeEval()
            {
                return new IntervalOpCalWithEndEval(
                    intervalComputer.MakeComputerEval(),
                    forgeEndTimestamp.ExprEvaluator);
            }

            protected override CodegenExpression CodegenEvaluate(
                CodegenExpressionRef startTs,
                CodegenExpressionRef endTs,
                CodegenExpressionRef paramStartTs,
                CodegenExpressionRef paramEndTs,
                CodegenMethod parentNode,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return intervalComputer.Codegen(
                    startTs,
                    endTs,
                    ExprDotName(paramStartTs, "UtcMillis"),
                    ExprDotName(paramEndTs, "UtcMillis"),
                    parentNode,
                    exprSymbol,
                    codegenClassScope);
            }
        }

        public class IntervalOpCalWithEndEval : IntervalOpEvalDateWithEndBase
        {
            public IntervalOpCalWithEndEval(
                IntervalComputerEval intervalComputer,
                ExprEvaluator evaluatorEndTimestamp)
                : base(intervalComputer, evaluatorEndTimestamp)

            {
            }

            public override object Evaluate(
                long startTs,
                long endTs,
                object parameterStartTs,
                object parameterEndTs,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext context)
            {
                return intervalComputer.Compute(
                    startTs,
                    endTs,
                    ((DateTimeEx) parameterStartTs).UtcMillis,
                    ((DateTimeEx) parameterEndTs).UtcMillis,
                    eventsPerStream,
                    isNewData,
                    context);
            }
        }

        public class IntervalOpDateTimeOffsetWithEndForge : IntervalOpForgeDateWithEndBase
        {
            public IntervalOpDateTimeOffsetWithEndForge(
                IntervalComputerForge intervalComputer,
                ExprForge evaluatorEndTimestamp)
                : base(intervalComputer, evaluatorEndTimestamp)
            {
            }

            public override IIntervalOpEval MakeEval()
            {
                return new IntervalOpDateTimeOffsetWithEndEval(
                    intervalComputer.MakeComputerEval(),
                    forgeEndTimestamp.ExprEvaluator);
            }

            protected override CodegenExpression CodegenEvaluate(
                CodegenExpressionRef startTs,
                CodegenExpressionRef endTs,
                CodegenExpressionRef paramStartTs,
                CodegenExpressionRef paramEndTs,
                CodegenMethod parentNode,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return intervalComputer.Codegen(
                    startTs,
                    endTs,
                    StaticMethod(
                        typeof(DatetimeLongCoercerDateTimeOffset),
                        "CoerceToMillis",
                        paramStartTs),
                    StaticMethod(
                        typeof(DatetimeLongCoercerDateTimeOffset),
                        "CoerceToMillis",
                        paramEndTs),
                    parentNode,
                    exprSymbol,
                    codegenClassScope);
            }
        }

        public class IntervalOpDateTimeOffsetWithEndEval : IntervalOpEvalDateWithEndBase
        {
            public IntervalOpDateTimeOffsetWithEndEval(
                IntervalComputerEval intervalComputer,
                ExprEvaluator evaluatorEndTimestamp)
                : base(intervalComputer, evaluatorEndTimestamp)
            {
            }

            public override object Evaluate(
                long startTs,
                long endTs,
                object parameterStartTs,
                object parameterEndTs,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext context)
            {
                return intervalComputer.Compute(
                    startTs,
                    endTs,
                    DatetimeLongCoercerDateTimeOffset.CoerceToMillis((DateTimeOffset) parameterStartTs),
                    DatetimeLongCoercerDateTimeOffset.CoerceToMillis((DateTimeOffset) parameterEndTs),
                    eventsPerStream,
                    isNewData,
                    context);
            }
        }

        public class IntervalOpDateTimeWithEndForge : IntervalOpForgeDateWithEndBase
        {
            public IntervalOpDateTimeWithEndForge(
                IntervalComputerForge intervalComputer,
                ExprForge evaluatorEndTimestamp)
                : base(intervalComputer, evaluatorEndTimestamp)

            {
            }

            public override IIntervalOpEval MakeEval()
            {
                return new IntervalOpDateTimeWithEndEval(
                    intervalComputer.MakeComputerEval(),
                    forgeEndTimestamp.ExprEvaluator);
            }

            protected override CodegenExpression CodegenEvaluate(
                CodegenExpressionRef startTs,
                CodegenExpressionRef endTs,
                CodegenExpressionRef paramStartTs,
                CodegenExpressionRef paramEndTs,
                CodegenMethod parentNode,
                ExprForgeCodegenSymbol exprSymbol,
                CodegenClassScope codegenClassScope)
            {
                return intervalComputer.Codegen(
                    startTs,
                    endTs,
                    StaticMethod(typeof(DatetimeLongCoercerDateTime), "CoerceToMillis", paramStartTs),
                    StaticMethod(typeof(DatetimeLongCoercerDateTime), "CoerceToMillis", paramEndTs),
                    parentNode,
                    exprSymbol,
                    codegenClassScope);
            }
        }

        public class IntervalOpDateTimeWithEndEval : IntervalOpEvalDateWithEndBase
        {
            public IntervalOpDateTimeWithEndEval(
                IntervalComputerEval intervalComputer,
                ExprEvaluator evaluatorEndTimestamp)
                : base(intervalComputer, evaluatorEndTimestamp)

            {
            }

            public override object Evaluate(
                long startTs,
                long endTs,
                object parameterStartTs,
                object parameterEndTs,
                EventBean[] eventsPerStream,
                bool isNewData,
                ExprEvaluatorContext context)
            {
                return intervalComputer.Compute(
                    startTs,
                    endTs,
                    DatetimeLongCoercerDateTime.CoerceToMillis((DateTime) parameterStartTs),
                    DatetimeLongCoercerDateTime.CoerceToMillis((DateTime) parameterEndTs),
                    eventsPerStream,
                    isNewData,
                    context);
            }
        }
    }
} // end of namespace