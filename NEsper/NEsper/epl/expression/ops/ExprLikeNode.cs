///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.ops
{
    /// <summary>
    /// Represents the like-clause in an expression tree.
    /// </summary>
    [Serializable]
    public class ExprLikeNode : ExprNodeBase, ExprEvaluator
    {
        private readonly bool _isNot;

        private bool _isNumericValue;
        private bool _isConstantPattern;

        [NonSerialized]
        private LikeUtil _likeUtil;
        [NonSerialized]
        private ExprEvaluator[] _evaluators;

        /// <summary>Ctor. </summary>
        /// <param name="not">is true if this is a "not like", or false if just a like</param>
        public ExprLikeNode(bool not)
        {
            _isNot = not;
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if ((ChildNodes.Count != 2) && (ChildNodes.Count != 3))
            {
                throw new ExprValidationException("The 'like' operator requires 2 (no escape) or 3 (with escape) child expressions");
            }
            _evaluators = ExprNodeUtility.GetEvaluators(ChildNodes);

            // check eval child node - can be String or numeric
            Type evalChildType = _evaluators[0].ReturnType;
            _isNumericValue = evalChildType.IsNumeric();
            if ((evalChildType != typeof(String)) && (!_isNumericValue))
            {
                throw new ExprValidationException("The 'like' operator requires a String or numeric type left-hand expression");
            }

            // check pattern child node
            ExprEvaluator patternChildNode = _evaluators[1];
            Type patternChildType = patternChildNode.ReturnType;
            if (patternChildType != typeof(String))
            {
                throw new ExprValidationException("The 'like' operator requires a String-type pattern expression");
            }
            if (ChildNodes[1].IsConstantResult)
            {
                _isConstantPattern = true;
            }

            // check escape character node
            if (ChildNodes.Count == 3)
            {
                ExprEvaluator escapeChildNode = _evaluators[2];
                if (escapeChildNode.ReturnType != typeof(String))
                {
                    throw new ExprValidationException("The 'like' operator escape parameter requires a character-type value");
                }
            }

            return null;
        }

        public Type ReturnType
        {
            get { return typeof(bool?); }
        }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        public object Evaluate(EvaluateParams evaluateParams)
        {
            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().QExprLike(this); }
            if (_likeUtil == null)
            {
                var patternVal = (string)_evaluators[1].Evaluate(evaluateParams);
                if (patternVal == null)
                {
                    if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprLike(null); }
                    return null;
                }
                string escape = "\\";
                char? escapeCharacter = null;
                if (ChildNodes.Count == 3)
                {
                    escape = (String)_evaluators[2].Evaluate(evaluateParams);
                }
                if (escape.Length > 0)
                {
                    escapeCharacter = escape[0];
                }
                _likeUtil = new LikeUtil(patternVal, escapeCharacter, false);
            }
            else
            {
                if (!_isConstantPattern)
                {
                    var patternVal = (string)_evaluators[1].Evaluate(evaluateParams);
                    if (patternVal == null)
                    {
                        if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprLike(null); }
                        return null;
                    }
                    _likeUtil.ResetPattern(patternVal);
                }
            }

            var evalValue = _evaluators[0].Evaluate(evaluateParams);
            if (evalValue == null)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprLike(null); }
                return null;
            }

            if (_isNumericValue)
            {
                evalValue = evalValue.ToString();
            }

            var result = _likeUtil.Compare((String)evalValue);

            if (_isNot)
            {
                if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprLike(!result); }
                return !result;
            }

            if (InstrumentationHelper.ENABLED) { InstrumentationHelper.Get().AExprLike(result); }
            return result;
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            if (!(node is ExprLikeNode))
            {
                return false;
            }

            var other = (ExprLikeNode)node;
            return _isNot == other._isNot;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            var childNodes = ChildNodes;
            childNodes[0].ToEPL(writer, Precedence);

            if (_isNot)
            {
                writer.Write(" not");
            }

            writer.Write(" like ");
            childNodes[1].ToEPL(writer, Precedence);

            if (childNodes.Count == 3)
            {
                writer.Write(" escape ");
                childNodes[2].ToEPL(writer, Precedence);
            }
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.RELATIONAL_BETWEEN_IN; }
        }

        /// <summary>Returns true if this is a "not like", or false if just a like </summary>
        /// <value>indicator whether negated or not</value>
        public bool IsNot
        {
            get { return _isNot; }
        }
    }
}
