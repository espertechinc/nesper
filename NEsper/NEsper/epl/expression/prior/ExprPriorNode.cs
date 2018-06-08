///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.prior
{
    /// <summary>
    /// Represents the 'prior' prior event function in an expression node tree.
    /// </summary>
    [Serializable]
    public class ExprPriorNode : ExprNodeBase, ExprEvaluator
    {
        private Type _resultType;
        private int _streamNumber;
        private int _constantIndexNumber;
        [NonSerialized] private ExprPriorEvalStrategy _priorStrategy;
        [NonSerialized] private ExprEvaluator _innerEvaluator;

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public int StreamNumber
        {
            get { return _streamNumber; }
        }

        public int ConstantIndexNumber
        {
            get { return _constantIndexNumber; }
        }

        public ExprPriorEvalStrategy PriorStrategy
        {
            set { _priorStrategy = value; }
        }

        public ExprEvaluator InnerEvaluator
        {
            get { return _innerEvaluator; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Count != 2)
            {
                throw new ExprValidationException("Prior node must have 2 parameters");
            }
            if (!(ChildNodes[0].IsConstantResult))
            {
                throw new ExprValidationException("Prior function requires a constant-value integer-typed index expression as the first parameter");
            }

            var constantNode = ChildNodes[0];
            if (constantNode.ExprEvaluator.ReturnType.IsNotInt32())
            {
                throw new ExprValidationException("Prior function requires an integer index parameter");
            }

            var value = constantNode.ExprEvaluator.Evaluate(
                    new EvaluateParams(null, false, validationContext.ExprEvaluatorContext));
            _constantIndexNumber = value.AsInt();
            _innerEvaluator = ChildNodes[1].ExprEvaluator;
    
            // Determine stream number
            // Determine stream number
            if (ChildNodes[1] is ExprIdentNode) {
                var identNode = (ExprIdentNode) ChildNodes[1];
                _streamNumber = identNode.StreamId;
                _resultType = _innerEvaluator.ReturnType;
            }
            else if (ChildNodes[1] is ExprStreamUnderlyingNode) {
                var streamNode = (ExprStreamUnderlyingNode) ChildNodes[1];
                _streamNumber = streamNode.StreamId;
                _resultType = _innerEvaluator.ReturnType;
            }
            else
            {
                throw new ExprValidationException("Previous function requires an event property as parameter");
            }
    
            // add request
            if (validationContext.ViewResourceDelegate == null) {
                throw new ExprValidationException("Prior function cannot be used in this context");
            }
            validationContext.ViewResourceDelegate.AddPriorNodeRequest(this);

            return null;
        }

        public Type ReturnType
        {
            get { return _resultType; }
        }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            return Evaluate(
                evaluateParams.EventsPerStream,
                evaluateParams.IsNewData,
                evaluateParams.ExprEvaluatorContext);
        }

        public Object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            var result = new Mutable<object>();

            using(Instrument.With(
                i => i.QExprPrior(this),
                i => i.AExprPrior(result)))
            {
                result.Value = _priorStrategy.Evaluate(eventsPerStream, isNewData, exprEvaluatorContext, _streamNumber, _innerEvaluator, _constantIndexNumber);
                return result.Value;
            }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("prior(");
            ChildNodes[0].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
            writer.Write(',');
            ChildNodes[1].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
            writer.Write(')');
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            return node is ExprPriorNode;
        }
    }
}
