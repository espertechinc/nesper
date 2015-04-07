///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;

namespace com.espertech.esper.epl.expression.ops
{
    /// <summary>
    /// Represents a simple Math (+/-/divide/*) in a filter expression tree.
    /// </summary>
    [Serializable]
    public class ExprConcatNode : ExprNodeBase, ExprEvaluator
    {
        private readonly StringBuilder _buffer;
        [NonSerialized] private ExprEvaluator[] _evaluators;
        /// <summary>Ctor. </summary>
        public ExprConcatNode()
        {
            _buffer = new StringBuilder();
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Length < 2)
            {
                throw new ExprValidationException("Concat node must have at least 2 parameters");
            }
            _evaluators = ExprNodeUtility.GetEvaluators(ChildNodes);
    
            for (var i = 0; i < _evaluators.Length; i++)
            {
                var childType = _evaluators[i].ReturnType;
                var childTypeName = childType == null ? "null" : childType.FullName;
                if (childType != typeof(String))
                {
                    throw new ExprValidationException(string.Format("Implicit conversion from datatype '{0}' to string is not allowed", childTypeName));
                }
            }

            return null;
        }

        public Type ReturnType
        {
            get { return typeof (string); }
        }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            string[] result = { null };

            using (Instrument.With(
                i => i.QExprConcat(this),
                i => i.AExprConcat(result[0])))
            {
                _buffer.Length = 0;

                foreach (var child in _evaluators)
                {
                    result[0] = (String) child.Evaluate(evaluateParams);
                    if (result[0] == null)
                    {
                        return null;
                    }
                    _buffer.Append(result[0]);
                }

                result[0] = _buffer.ToString();
                return result[0];
            }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            String delimiter = "";
            foreach (ExprNode child in ChildNodes)
            {
                writer.Write(delimiter);
                child.ToEPL(writer, Precedence);
                delimiter = "||";
            }
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.CONCAT; }
        }

        public override bool EqualsNode(ExprNode node)
        {
            return node is ExprConcatNode;
        }
    }
}
