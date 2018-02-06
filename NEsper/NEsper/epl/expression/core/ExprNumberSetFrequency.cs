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
    /// <summary>Expression for use within crontab to specify a frequency.</summary>
    [Serializable]
    public class ExprNumberSetFrequency
        : ExprNodeBase
        , ExprEvaluator
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [NonSerialized] private ExprEvaluator _evaluator;

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write("*/");
            this.ChildNodes[0].ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.MINIMUM; }
        }

        public override bool IsConstantResult
        {
            get { return this.ChildNodes[0].IsConstantResult; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            return node is ExprNumberSetFrequency;
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            _evaluator = ChildNodes[0].ExprEvaluator;
            var type = _evaluator.ReturnType;
            if (!type.IsNumericNonFP())
            {
                throw new ExprValidationException("Frequency operator requires an integer-type parameter");
            }
            return null;
        }

        public Type ReturnType
        {
            get { return typeof (FrequencyParameter); }
        }

        public Object Evaluate(EvaluateParams evaluateParams)
        {
            var value = _evaluator.Evaluate(evaluateParams);
            if (value == null)
            {
                Log.Warn("Null value returned for frequency parameter");
                return new FrequencyParameter(Int32.MaxValue);
            }
            else
            {
                var intValue = value.AsInt();
                return new FrequencyParameter(intValue);
            }
        }
    }
} // end of namespace
