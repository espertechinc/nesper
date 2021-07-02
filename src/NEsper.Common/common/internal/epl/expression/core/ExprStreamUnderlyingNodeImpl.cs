///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

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
    ///     Represents an stream selector that returns the streams underlying event, or null if undefined.
    /// </summary>
    public class ExprStreamUnderlyingNodeImpl : ExprNodeBase,
        ExprForgeInstrumentable,
        ExprEvaluator,
        ExprStreamUnderlyingNode
    {
        private readonly bool _isWildcard;
        [NonSerialized] private EventType _eventType;
        private int _streamNum = -1;

        public ExprStreamUnderlyingNodeImpl(
            string streamName,
            bool isWildcard)
        {
            if (streamName == null && !isWildcard) {
                throw new ArgumentException("Stream name is null");
            }

            StreamName = streamName;
            this._isWildcard = isWildcard;
        }

        public ExprNode ForgeRenderable => this;

        /// <summary>
        ///     Returns the stream name.
        /// </summary>
        /// <returns>stream name</returns>
        public string StreamName { get; }

        public bool IsConstantResult => false;

        public object Evaluate(
            EventBean[] eventsPerStream,
            bool isNewData,
            ExprEvaluatorContext exprEvaluatorContext)
        {
            var @event = eventsPerStream[_streamNum];

            return @event?.Underlying;
        }

        ExprNodeRenderable ExprForge.ExprForgeRenderable => ForgeRenderable;

        public ExprEvaluator ExprEvaluator => this;

        public ExprForgeConstantType ForgeConstantType => ExprForgeConstantType.NONCONST;

        public CodegenExpression EvaluateCodegenUninstrumented(
            Type requiredType,
            CodegenMethodScope codegenMethodScope,
            ExprForgeCodegenSymbol exprSymbol,
            CodegenClassScope codegenClassScope)
        {
            var methodNode = codegenMethodScope.MakeChild(
                _eventType.UnderlyingType,
                typeof(ExprStreamUnderlyingNodeImpl),
                codegenClassScope);
            var refEPS = exprSymbol.GetAddEPS(methodNode);
            methodNode.Block
                .DeclareVar<EventBean>("@event", ArrayAtIndex(refEPS, Constant(_streamNum)))
                .IfRefNullReturnNull("@event")
                .MethodReturn(Cast(_eventType.UnderlyingType, ExprDotName(Ref("@event"), "Underlying")));
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
                    codegenClassScope)
                .Build();
        }

        public Type EvaluationType {
            get {
                if (_streamNum == -1) {
                    throw new IllegalStateException("Stream underlying node has not been validated");
                }

                return _eventType.UnderlyingType;
            }
        }

        public override ExprForge Forge => this;

        public int? StreamReferencedIfAny => StreamId;

        public string RootPropertyNameIfAny => null;

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (StreamName == null && _isWildcard) {
                if (validationContext.StreamTypeService.StreamNames.Length > 1) {
                    throw new ExprValidationException(
                        "Wildcard must be stream wildcard if specifying multiple streams, use the 'streamname.*' syntax instead");
                }

                _streamNum = 0;
            }
            else {
                _streamNum = validationContext.StreamTypeService.GetStreamNumForStreamName(StreamName);
            }

            if (_streamNum == -1) {
                throw new ExprValidationException(
                    "Stream by name '" + StreamName + "' could not be found among all streams");
            }

            _eventType = validationContext.StreamTypeService.EventTypes[_streamNum];
            return null;
        }

        /// <summary>
        ///     Returns stream id supplying the property value.
        /// </summary>
        /// <value>stream number</value>
        public int StreamId {
            get {
                if (_streamNum == -1) {
                    throw new IllegalStateException("Stream underlying node has not been validated");
                }

                return _streamNum;
            }
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.UNARY;

        public EventType EventType => _eventType;

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprStreamUnderlyingNodeImpl)) {
                return false;
            }

            var other = (ExprStreamUnderlyingNodeImpl) node;
            if (_isWildcard != other._isWildcard) {
                return false;
            }

            if (_isWildcard) {
                return true;
            }

            return StreamName.Equals(other.StreamName);
        }

        public override string ToString()
        {
            return "streamName=" +
                   StreamName +
                   " streamNum=" +
                   _streamNum;
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            writer.Write(StreamName);
            if (_isWildcard) {
                writer.Write(".*");
            }
        }
        
        public ExprEnumerationForgeDesc GetEnumerationForge(StreamTypeService streamTypeService, ContextCompileTimeDescriptor contextDescriptor) {
            return new ExprEnumerationForgeDesc(
                new ExprStreamUnderlyingNodeEnumerationForge(StreamName, _streamNum, _eventType),
                streamTypeService.IStreamOnly[StreamId],
                StreamId);
        }
    }
} // end of namespace