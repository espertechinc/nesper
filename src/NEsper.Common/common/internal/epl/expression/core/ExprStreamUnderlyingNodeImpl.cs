///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text.Json.Serialization;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.bytecodemodel.@base;
using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.context.compile;
using com.espertech.esper.common.@internal.epl.expression.codegen;
using com.espertech.esper.common.@internal.epl.streamtype;
using com.espertech.esper.common.@internal.metrics.instrumentation;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.core
{
    /// <summary>
    /// Represents an stream selector that returns the streams underlying event, or null if undefined.
    /// </summary>
    public class ExprStreamUnderlyingNodeImpl : ExprNodeBase,
        ExprForgeInstrumentable,
        ExprEvaluator,
        ExprStreamUnderlyingNode
    {
        private readonly string streamName;
        private readonly bool isWildcard;
        private int streamNum = -1;
        [JsonIgnore]
        [NonSerialized]
        private EventType eventType;

        public ExprStreamUnderlyingNodeImpl(
            string streamName,
            bool isWildcard)
        {
            if (streamName == null && !isWildcard) {
                throw new ArgumentException("Stream name is null");
            }

            this.streamName = streamName;
            this.isWildcard = isWildcard;
        }

        public Type EvaluationType {
            get {
                if (streamNum == -1) {
                    throw new IllegalStateException("Stream underlying node has not been validated");
                }

                return eventType.UnderlyingType;
            }
        }

        public ExprEvaluator ExprEvaluator => this;

        public override ExprForge Forge => this;

        public ExprNodeRenderable ExprForgeRenderable => this;

        /// <summary>
        /// Returns the stream name.
        /// </summary>
        /// <value>stream name</value>
        public string StreamName => streamName;

        public int? StreamReferencedIfAny => StreamId;

        public string RootPropertyNameIfAny => null;

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (streamName == null && isWildcard) {
                if (validationContext.StreamTypeService.StreamNames.Length > 1) {
                    throw new ExprValidationException(
                        "Wildcard must be stream wildcard if specifying multiple streams, use the 'streamname.*' syntax instead");
                }

                streamNum = 0;
            }
            else {
                streamNum = validationContext.StreamTypeService.GetStreamNumForStreamName(streamName);
            }

            if (streamNum == -1) {
                throw new ExprValidationException(
                    "Stream by name '" + streamName + "' could not be found among all streams");
            }

            eventType = validationContext.StreamTypeService.EventTypes[streamNum];
            return null;
        }

        public bool IsConstantResult => false;

        /// <summary>
        /// Returns stream id supplying the property value.
        /// </summary>
        /// <value>stream number</value>
        public int StreamId {
            get {
                if (streamNum == -1) {
                    throw new IllegalStateException("Stream underlying node has not been validated");
                }

                return streamNum;
            }
        }

        public override string ToString()
        {
            return "streamName=" +
                   streamName +
                   " streamNum=" +
                   streamNum;
        }

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var @event = eventsPerStream[streamNum];
            if (@event == null) {
                return null;
            }

            return @event.Underlying;
        }

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                eventType.UnderlyingType,
                typeof(ExprStreamUnderlyingNodeImpl),
                codegenClassScope);
            var refEPS = exprSymbol.GetAddEps(methodNode);
            methodNode.Block
                .DeclareVar<EventBean>("@event", ArrayAtIndex(refEPS, Constant(streamNum)))
                .IfRefNullReturnNull("@event")
                .MethodReturn(Cast(eventType.UnderlyingType, ExprDotName(Ref("@event"), "Underlying")));
            return LocalMethod(methodNode);
        }

        public CodegenExpression EvaluateCodegen(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            return new InstrumentationBuilderExpr(
                GetType(),
                this,
                "ExprStreamUnd",
                requiredType,
                codegenMethodScope,
                exprSymbol,
                codegenClassScope).Build();
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            writer.Write(streamName);
            if (isWildcard) {
                writer.Write(".*");
            }
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public EventType EventType => eventType;

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprStreamUnderlyingNodeImpl other)) {
                return false;
            }

            if (isWildcard != other.isWildcard) {
                return false;
            }

            if (isWildcard) {
                return true;
            }

            return streamName.Equals(other.streamName);
        }

        public ExprEnumerationForgeDesc GetEnumerationForge(
            StreamTypeService streamTypeService,
            ContextCompileTimeDescriptor contextDescriptor)
        {
            return new ExprEnumerationForgeDesc(
                new ExprStreamUnderlyingNodeEnumerationForge(streamName, streamNum, eventType),
                streamTypeService.IStreamOnly[StreamId],
                StreamId);
        }
    }
} // end of namespace