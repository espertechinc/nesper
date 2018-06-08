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
using com.espertech.esper.compat.logging;
using com.espertech.esper.type;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.core
{
    /// <summary>
    /// Expression for use within crontab to specify a range.
    /// <para/>
    /// Differs from the between-expression since the value returned by evaluating is a cron-value object.
    /// </summary>
    [Serializable]
    public class ExprNumberSetRange
        : ExprNodeBase
        , ExprEvaluator
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [NonSerialized]
        private ExprEvaluator[] _evaluators;

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            ChildNodes[0].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
            writer.Write(":");
            ChildNodes[1].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public override bool IsConstantResult
        {
            get { return ChildNodes[0].IsConstantResult && ChildNodes[1].IsConstantResult; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            return node is ExprNumberSetRange;
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            _evaluators = ExprNodeUtility.GetEvaluators(ChildNodes);
            Type typeOne = _evaluators[0].ReturnType;
            Type typeTwo = _evaluators[1].ReturnType;
            if ((!(typeOne.IsNumericNonFP())) || (!(typeTwo.IsNumericNonFP())))
            {
                throw new ExprValidationException("Range operator requires integer-type parameters");
            }

            return null;
        }

        public Type ReturnType
        {
            get { return typeof(RangeParameter); }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            Object valueLower = _evaluators[0].Evaluate(evaluateParams);
            Object valueUpper = _evaluators[1].Evaluate(evaluateParams);
            if (valueLower == null)
            {
                Log.Warn("Null value returned for lower bounds value in range parameter, using zero as lower bounds");
                valueLower = 0;
            }
            if (valueUpper == null)
            {
                Log.Warn("Null value returned for upper bounds value in range parameter, using max as upper bounds");
                valueUpper = int.MaxValue;
            }
            int intValueLower = valueLower.AsInt();
            int intValueUpper = valueUpper.AsInt();
            return new RangeParameter(intValueLower, intValueUpper);
        }
    }
}
