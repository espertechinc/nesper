///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text.RegularExpressions;

using com.espertech.esper.client;
using com.espertech.esper.compat;
using com.espertech.esper.epl.expression.core;
using com.espertech.esper.metrics.instrumentation;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.expression.ops
{
    /// <summary>
    /// Represents the regexp-clause in an expression tree.
    /// </summary>
    [Serializable]
    public class ExprRegexpNode : ExprNodeBase, ExprEvaluator
    {
        private readonly bool _isNot;

        private Regex _pattern;
        private bool _isNumericValue;
        private bool _isConstantPattern;
        [NonSerialized]
        private ExprEvaluator[] _evaluators;

        /// <summary>Ctor. </summary>
        /// <param name="not">is true if the it's a "not regexp" expression, of false for regular regexp</param>
        public ExprRegexpNode(bool not)
        {
            _isNot = not;
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { return this; }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Count != 2)
            {
                throw new ExprValidationException("The regexp operator requires 2 child expressions");
            }
            _evaluators = ExprNodeUtility.GetEvaluators(ChildNodes);

            // check pattern child node
            var patternChildType = _evaluators[1].ReturnType;
            if (patternChildType != typeof(String))
            {
                throw new ExprValidationException("The regexp operator requires a String-type pattern expression");
            }
            if (ChildNodes[1].IsConstantResult)
            {
                _isConstantPattern = true;
            }

            // check eval child node - can be String or numeric
            Type evalChildType = _evaluators[0].ReturnType;
            _isNumericValue = evalChildType.IsNumeric();
            if ((evalChildType != typeof(String)) && (!_isNumericValue))
            {
                throw new ExprValidationException("The regexp operator requires a String or numeric type left-hand expression");
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
            var result = new Mutable<bool?>(null);

            using (Instrument.With(
                i => i.QExprRegexp(this),
                i => i.AExprRegexp(result.Value)))
            {
                if (_pattern == null)
                {
                    var patternText = (String)_evaluators[1].Evaluate(evaluateParams);
                    if (patternText == null)
                    {
                        return null;
                    }
                    try
                    {
                        _pattern = new Regex(String.Format("^{0}$", patternText));
                    }
                    catch (ArgumentException ex)
                    {
                        throw new EPException("Error compiling regex pattern '" + patternText + '\'', ex);
                    }
                }
                else
                {
                    if (!_isConstantPattern)
                    {
                        var patternText = (String)_evaluators[1].Evaluate(evaluateParams);
                        if (patternText == null)
                        {
                            return null;
                        }
                        try
                        {
                            _pattern = new Regex(String.Format("^{0}$", patternText));

                        }
                        catch (ArgumentException ex)
                        {
                            throw new EPException("Error compiling regex pattern '" + patternText + '\'', ex);
                        }
                    }
                }

                var evalValue = _evaluators[0].Evaluate(evaluateParams);
                if (evalValue == null)
                {
                    return null;
                }

                if (_isNumericValue)
                {
                    if (evalValue is double)
                    {
                        var tempValue = (double)evalValue;
                        evalValue = tempValue.ToString("F");
                    }
                    else if (evalValue is float)
                    {
                        var tempValue = (float)evalValue;
                        evalValue = tempValue.ToString("F");
                    }
                    else if (evalValue is decimal)
                    {
                        var tempValue = (decimal)evalValue;
                        evalValue = tempValue.ToString("F");
                    }
                    else
                    {
                        evalValue = evalValue.ToString();
                    }
                }

                result.Value = _pattern.IsMatch((String)evalValue);

                if (_isNot)
                {
                    result.Value = !result.Value;
                }

                return result.Value;
            }
        }

        public override bool EqualsNode(ExprNode node)
        {
            if (!(node is ExprRegexpNode))
            {
                return false;
            }

            var other = (ExprRegexpNode)node;
            if (_isNot != other._isNot)
            {
                return false;
            }
            return true;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            ChildNodes[0].ToEPL(writer, Precedence);
            if (_isNot)
            {
                writer.Write(" not");
            }
            writer.Write(" regexp ");
            ChildNodes[1].ToEPL(writer, Precedence);
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.RELATIONAL_BETWEEN_IN; }
        }

        /// <summary>Returns true if this is a "not regexp", or false if just a regexp </summary>
        /// <value>indicator whether negated or not</value>
        public bool IsNot
        {
            get { return _isNot; }
        }
    }
}
