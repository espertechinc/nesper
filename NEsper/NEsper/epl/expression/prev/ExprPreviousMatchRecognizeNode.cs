///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.rowregex;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.prev
{
    /// <summary>
    /// Represents the 'prev' previous event function in match-recognize "define" item.
    /// </summary>
    [Serializable]
    public class ExprPreviousMatchRecognizeNode : ExprNodeBase, ExprEvaluator
    {
        private Type _resultType;
        private int _streamNumber;
        private int? _constantIndexNumber;
    
        [NonSerialized] private RegexExprPreviousEvalStrategy _strategy;
        [NonSerialized] private ExprEvaluator _evaluator;
        private int _assignedIndex;
    
        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Count != 2)
            {
                throw new ExprValidationException("Match-Recognize Previous expression must have 2 parameters");
            }
    
            if (!(ChildNodes[0] is ExprIdentNode))
            {
                throw new ExprValidationException("Match-Recognize Previous expression requires an property identifier as the first parameter");
            }
    
            if (!ChildNodes[1].IsConstantResult || (!ChildNodes[1].ExprEvaluator.ReturnType.IsNumericNonFP()))
            {
                throw new ExprValidationException("Match-Recognize Previous expression requires an integer index parameter or expression as the second parameter");
            }
    
            var constantNode = ChildNodes[1];
            var value = constantNode.ExprEvaluator.Evaluate(new EvaluateParams(null, false, validationContext.ExprEvaluatorContext));
            if (!value.IsNumber())
            {
                throw new ExprValidationException("Match-Recognize Previous expression requires an integer index parameter or expression as the second parameter");
            }
            _constantIndexNumber = value.AsInt();
    
            // Determine stream number
            var identNode = (ExprIdentNode) ChildNodes[0];
            _streamNumber = identNode.StreamId;
            _evaluator = ChildNodes[0].ExprEvaluator;
            _resultType = _evaluator.ReturnType;

            return null;
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        /// <summary>Returns the index number. </summary>
        /// <value>index number</value>
        public int ConstantIndexNumber
        {
            get
            {
                if (_constantIndexNumber == null)
                {
                    var constantNode = ChildNodes[1];
                    var value = constantNode.ExprEvaluator.Evaluate(new EvaluateParams(null, false, null));
                    _constantIndexNumber = value.AsInt();
                }
                return _constantIndexNumber.Value;
            }
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
            var access = _strategy.GetAccess(evaluateParams.ExprEvaluatorContext);
            var substituteEvent = access.GetPreviousEvent(_assignedIndex);
    
            if (substituteEvent == null)
            {
                return null;
            }
    
            // Substitute original event with prior event, evaluate inner expression
            var eventsPerStream = evaluateParams.EventsPerStream;
            var originalEvent = eventsPerStream[_streamNumber];
            eventsPerStream[_streamNumber] = substituteEvent;
            var evalResult = _evaluator.Evaluate(evaluateParams);
            eventsPerStream[_streamNumber] = originalEvent;
    
            return evalResult;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("prev(");
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
            return node is ExprPreviousMatchRecognizeNode;
        }

        /// <summary>Sets the index to use when accessing via getter </summary>
        /// <value>index</value>
        public int AssignedIndex
        {
            get { return _assignedIndex; }
            set { _assignedIndex = value; }
        }

        public RegexExprPreviousEvalStrategy Strategy
        {
            get { return _strategy; }
            set { _strategy = value; }
        }
    }
}
