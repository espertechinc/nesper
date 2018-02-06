///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.events;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.prev
{
    /// <summary>
    /// Represents the 'prev' previous event function in an expression node tree.
    /// </summary>
    [Serializable]
    public class ExprPreviousNode
        : ExprNodeBase
        , ExprEvaluator
        , ExprEvaluatorEnumeration
    {
        private readonly ExprPreviousNodePreviousType? _previousType;

        private Type _resultType;
        [NonSerialized] private EventType _enumerationMethodType;

        [NonSerialized] private ExprPreviousEvalStrategy _evaluator;

        public ExprPreviousNode(ExprPreviousNodePreviousType? previousType)
        {
            _previousType = previousType;
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public ExprPreviousEvalStrategy Evaluator
        {
            get { return _evaluator; }
            set { _evaluator = value; }
        }

        public int StreamNumber { get; private set; }

        public int? ConstantIndexNumber { get; private set; }

        public bool IsConstantIndex { get; private set; }

        public Type ResultType
        {
            get { return _resultType; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if ((ChildNodes.Count > 2) || (ChildNodes.Count == 0))
            {
                throw new ExprValidationException("Previous node must have 1 or 2 parameters");
            }

            // add constant of 1 for previous index
            if (ChildNodes.Count == 1)
            {
                if (_previousType == ExprPreviousNodePreviousType.PREV)
                {
                    AddChildNodeToFront(new ExprConstantNodeImpl(1));
                }
                else
                {
                    AddChildNodeToFront(new ExprConstantNodeImpl(0));
                }
            }

            // the row recognition patterns allows "Prev(prop, index)", we switch index the first position
            if (ExprNodeUtility.IsConstantValueExpr(ChildNodes[1]))
            {
                var first = ChildNodes[0];
                var second = ChildNodes[1];
                SetChildNodes(second, first);
            }

            // Determine if the index is a constant value or an expression to evaluate
            if (ChildNodes[0].IsConstantResult)
            {
                var constantNode = ChildNodes[0];
                var value = constantNode.ExprEvaluator.Evaluate(new EvaluateParams(null, false, validationContext.ExprEvaluatorContext));
                if (!(value.IsNumber()))
                {
                    throw new ExprValidationException(
                        "Previous function requires an integer index parameter or expression");
                }

                if (TypeHelper.IsFloatingPointNumber(value))
                {
                    throw new ExprValidationException(
                        "Previous function requires an integer index parameter or expression");
                }

                ConstantIndexNumber = value.AsInt();
                IsConstantIndex = true;
            }

            // Determine stream number
            if (ChildNodes[1] is ExprIdentNode)
            {
                var identNode = (ExprIdentNode) ChildNodes[1];
                StreamNumber = identNode.StreamId;
                _resultType = ChildNodes[1].ExprEvaluator.ReturnType.GetBoxedType();
            }
            else if (ChildNodes[1] is ExprStreamUnderlyingNode)
            {
                var streamNode = (ExprStreamUnderlyingNode) ChildNodes[1];
                StreamNumber = streamNode.StreamId;
                _resultType = ChildNodes[1].ExprEvaluator.ReturnType.GetBoxedType();
                _enumerationMethodType = validationContext.StreamTypeService.EventTypes[streamNode.StreamId];
            }
            else
            {
                throw new ExprValidationException("Previous function requires an event property as parameter");
            }

            if (_previousType == ExprPreviousNodePreviousType.PREVCOUNT)
            {
                _resultType = typeof (long);
            }
            if (_previousType == ExprPreviousNodePreviousType.PREVWINDOW)
            {
                _resultType = TypeHelper.GetArrayType(_resultType);
            }

            if (validationContext.ViewResourceDelegate == null)
            {
                throw new ExprValidationException("Previous function cannot be used in this context");
            }
            validationContext.ViewResourceDelegate.AddPreviousRequest(this);
            return null;
        }

        public ExprPreviousNodePreviousType? PreviousType
        {
            get { return _previousType; }
        }

        public Type ReturnType
        {
            get { return _resultType; }
        }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        public ICollection<EventBean> EvaluateGetROCollectionEvents(EvaluateParams evaluateParams)
        {
            if (!evaluateParams.IsNewData)
            {
                return null;
            }
            return Evaluator.EvaluateGetCollEvents(evaluateParams.EventsPerStream, evaluateParams.ExprEvaluatorContext);
        }

        public EventBean EvaluateGetEventBean(EvaluateParams evaluateParams)
        {
            if (!evaluateParams.IsNewData)
            {
                return null;
            }
            return Evaluator.EvaluateGetEventBean(evaluateParams.EventsPerStream, evaluateParams.ExprEvaluatorContext);
        }

        public ICollection<object> EvaluateGetROCollectionScalar(EvaluateParams evaluateParams)
        {
            if (!evaluateParams.IsNewData)
            {
                return null;
            }
            return Evaluator.EvaluateGetCollScalar(evaluateParams.EventsPerStream, evaluateParams.ExprEvaluatorContext);
        }

        public EventType GetEventTypeCollection(EventAdapterService eventAdapterService, int statementId)
        {
            if (_previousType == ExprPreviousNodePreviousType.PREV ||
                _previousType == ExprPreviousNodePreviousType.PREVTAIL)
            {
                return null;
            }
            return _enumerationMethodType;
        }

        public EventType GetEventTypeSingle(EventAdapterService eventAdapterService, int statementId)
        {
            if (_previousType == ExprPreviousNodePreviousType.PREV ||
                _previousType == ExprPreviousNodePreviousType.PREVTAIL)
            {
                return _enumerationMethodType;
            }
            return null;
        }

        public Type ComponentTypeCollection
        {
            get
            {
                if (_resultType.IsArray)
                {
                    return ResultType.GetElementType();
                }
                return _resultType;
            }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            return Evaluate(
                evaluateParams.EventsPerStream,
                evaluateParams.IsNewData,
                evaluateParams.ExprEvaluatorContext
                );
        }

        public Object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            if (InstrumentationHelper.ENABLED)
            {
                InstrumentationHelper.Get().QExprPrev(this, isNewData);
                Object result = null;
                if (isNewData)
                {
                    result = _evaluator.Evaluate(eventsPerStream, exprEvaluatorContext);
                }
                InstrumentationHelper.Get().AExprPrev(result);
                return result;
            }

            if (!isNewData)
            {
                return null;
            }
            return Evaluator.Evaluate(eventsPerStream, exprEvaluatorContext);
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(_previousType.ToString().ToLowerInvariant());
            writer.Write("(");
            if (_previousType == ExprPreviousNodePreviousType.PREVCOUNT ||
                _previousType == ExprPreviousNodePreviousType.PREVWINDOW)
            {
                ChildNodes[1].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
            }
            else
            {
                ChildNodes[0].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
                if (ChildNodes.Count > 1)
                {
                    writer.Write(",");
                    ChildNodes[1].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
                }
            }
            writer.Write(')');
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        public override int GetHashCode()
        {
            return _previousType != null ? _previousType.GetHashCode() : 0;
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            var that = node as ExprPreviousNode;
            if (that == null)
            {
                return false;
            }

            if (_previousType != that._previousType)
            {
                return false;
            }

            return true;
        }
    }
} // end of namespace
