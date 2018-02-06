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
using System.Linq;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;
using com.espertech.esper.type;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.core
{
    /// <summary>
    /// Expression for use within crontab to specify a list of values.
    /// </summary>
    [Serializable]
    public class ExprNumberSetList
        : ExprNodeBase
        , ExprEvaluator
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [NonSerialized]
        private ExprEvaluator[] _evaluators;

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            String delimiter = "";

            writer.Write('[');
            for (int ii = 0; ii < ChildNodes.Count; ii++)
            {
                ExprNode expr = ChildNodes[ii];
                writer.Write(delimiter);
                expr.ToEPL(writer, ExprPrecedenceEnum.MINIMUM);
                delimiter = ",";
            }
            writer.Write(']');
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        public override bool IsConstantResult
        {
            get { return ChildNodes.All(child => child.IsConstantResult); }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            return (node is ExprNumberSetList);
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            // all nodes must either be int, frequency or range
            _evaluators = ExprNodeUtility.GetEvaluators(ChildNodes);
            foreach (ExprEvaluator child in _evaluators)
            {
                Type type = child.ReturnType;
                if ((type == typeof(FrequencyParameter)) || (type == typeof(RangeParameter)))
                {
                    continue;
                }
                if (!(type.IsNumericNonFP()))
                {
                    throw new ExprValidationException("Frequency operator requires an integer-type parameter");
                }
            }

            return null;
        }

        public Type ReturnType
        {
            get { return typeof(ListParameter); }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            IList<NumberSetParameter> parameters = new List<NumberSetParameter>();
            foreach (ExprEvaluator child in _evaluators)
            {
                Object value = child.Evaluate(evaluateParams);
                if (value == null)
                {
                    Log.Info("Null value returned for lower bounds value in list parameter, skipping parameter");
                    continue;
                }
                if ((value is FrequencyParameter) || (value is RangeParameter))
                {
                    parameters.Add((NumberSetParameter)value);
                    continue;
                }

                int intValue = value.AsInt();
                parameters.Add(new IntParameter(intValue));
            }
            if (parameters.IsEmpty())
            {
                Log.Warn("EmptyFalse list of values in list parameter, using upper bounds");
                parameters.Add(new IntParameter(int.MaxValue));
            }
            return new ListParameter(parameters);
        }
    }
}
