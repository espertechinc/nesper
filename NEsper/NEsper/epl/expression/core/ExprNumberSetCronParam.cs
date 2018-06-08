///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Reflection;

using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.type;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.core
{
    /// <summary>
    ///     Expression for a parameter within a crontab.
    ///     <para />
    ///     May have one subnode depending on the cron parameter type.
    /// </summary>
    [Serializable]
    public class ExprNumberSetCronParam
        : ExprNodeBase
        , ExprEvaluator
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly CronOperatorEnum _cronOperator;
        [NonSerialized]
        private ExprEvaluator _evaluator;

        /// <summary>Ctor. </summary>
        /// <param name="cronOperator">type of cron parameter</param>
        public ExprNumberSetCronParam(CronOperatorEnum cronOperator)
        {
            _cronOperator = cronOperator;
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        /// <summary>Returns the cron parameter type. </summary>
        /// <value>type of cron parameter</value>
        public CronOperatorEnum CronOperator
        {
            get { return _cronOperator; }
        }

        public override bool IsConstantResult
        {
            get
            {
                if (ChildNodes.Count == 0)
                {
                    return true;
                }
                return ChildNodes[0].IsConstantResult;
            }
        }

        public Type ReturnType
        {
            get { return typeof(CronParameter); }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            if (ChildNodes.Count == 0)
            {
                return new CronParameter(_cronOperator, null);
            }
            object value = _evaluator.Evaluate(evaluateParams);
            if (value == null)
            {
                Log.Warn("Null value returned for cron parameter");
                return new CronParameter(_cronOperator, null);
            }
            else
            {
                int intValue = value.AsInt();
                return new CronParameter(_cronOperator, intValue);
            }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            if (ChildNodes.Count != 0)
            {
                ChildNodes[0].ToEPL(writer, Precedence);
                writer.Write(" ");
            }
            writer.Write(_cronOperator.GetSyntax());
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            if (!(node is ExprNumberSetCronParam))
            {
                return false;
            }
            var other = (ExprNumberSetCronParam)node;
            return other._cronOperator.Equals(_cronOperator);
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Count == 0)
            {
                return null;
            }
            _evaluator = ChildNodes[0].ExprEvaluator;
            Type type = _evaluator.ReturnType;
            if (!(type.IsNumericNonFP()))
            {
                throw new ExprValidationException("Frequency operator requires an integer-type parameter");
            }

            return null;
        }
    }
}