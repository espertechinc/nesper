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
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.type;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.ops
{
    using RelationalComputer = Func<object, object, bool>;

    [Serializable]
    public class ExprRelationalOpNodeImpl 
        : ExprNodeBase
        , ExprEvaluator
        , ExprRelationalOpNode
    {
        private readonly RelationalOpEnum _relationalOpEnum;
        [NonSerialized] private RelationalComputer _computer;
        [NonSerialized] private ExprEvaluator[] _evaluators;
        /// <summary>Ctor. </summary>
        /// <param name="relationalOpEnum">type of compare, ie. lt, gt, le, ge</param>
        public ExprRelationalOpNodeImpl(RelationalOpEnum relationalOpEnum)
        {
            _relationalOpEnum = relationalOpEnum;
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        /// <summary>Returns the type of relational op used. </summary>
        /// <value>enum with relational op type</value>
        public RelationalOpEnum RelationalOpEnum
        {
            get { return _relationalOpEnum; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            // Must have 2 child nodes
            if (ChildNodes.Count != 2)
            {
                throw new IllegalStateException("Relational op node does not have exactly 2 parameters");
            }
            _evaluators = ExprNodeUtility.GetEvaluators(ChildNodes);
    
            // Must be either numeric or string
            Type typeOne = _evaluators[0].ReturnType.GetBoxedType();
            Type typeTwo = _evaluators[1].ReturnType.GetBoxedType();
            Type compareType;

            if (typeOne == typeTwo)
            {
                compareType = typeOne;
            }
            else if ((typeOne == typeof (string)) && (typeTwo == typeof (string)))
            {
                compareType = typeof (string);
            }
            else
            {
                try
                {
                    compareType = typeOne.GetCompareToCoercionType(typeTwo);
                }
                catch (CoercionException)
                {
                    if (!typeOne.IsNumeric())
                    {
                        throw new ExprValidationException(
                            string.Format("Implicit conversion from datatype '{0}' to numeric is not allowed", Name.Clean(typeOne)));
                    }
                    else if (!typeTwo.IsNumeric())
                    {
                        throw new ExprValidationException(
                            string.Format("Implicit conversion from datatype '{0}' to numeric is not allowed", Name.Clean(typeTwo)));
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            _computer = _relationalOpEnum.GetComputer(compareType, typeOne, typeTwo);

            return null;
        }

        public Type ReturnType
        {
            get { return typeof (bool?); }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprRelOp(this, _relationalOpEnum.GetExpressionText());}
            Object valueLeft = _evaluators[0].Evaluate(evaluateParams);
            if (valueLeft == null)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprRelOp(null);}
                return null;
            }
    
            Object valueRight = _evaluators[1].Evaluate(evaluateParams);
            if (valueRight == null)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprRelOp(null);}
                return null;
            }
    
            if (InstrumentationHelper.ENABLED) {
                var result = _computer.Invoke(valueLeft, valueRight);
                InstrumentationHelper.Get().AExprRelOp(result);
                return result;
            }
            return _computer.Invoke(valueLeft, valueRight);
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            ChildNodes[0].ToEPL(writer, Precedence);
            writer.Write(_relationalOpEnum.GetExpressionText());
            ChildNodes[1].ToEPL(writer, Precedence);
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.RELATIONAL_BETWEEN_IN; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            var other = node as ExprRelationalOpNodeImpl;
            if (other == null)
            {
                return false;
            }
    
            return other._relationalOpEnum == _relationalOpEnum;
        }
    }
}
