///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text.RegularExpressions;

using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    /// Represents the regexp-clause in an expression tree.
    /// </summary>
    [Serializable]
    public class ExprRegexpNode : ExprNodeBase
    {
        private readonly bool _isNot;

        [NonSerialized] private ExprRegexpNodeForge _forge;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="not">is true if the it's a "not regexp" expression, of false for regular regexp</param>
        public ExprRegexpNode(bool not)
        {
            _isNot = not;
        }

        public ExprEvaluator ExprEvaluator {
            get {
                CheckValidated(_forge);
                return _forge.ExprEvaluator;
            }
        }

        public override ExprForge Forge {
            get {
                CheckValidated(_forge);
                return _forge;
            }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (ChildNodes.Length != 2) {
                throw new ExprValidationException("The regexp operator requires 2 child expressions");
            }

            // check pattern child node
            var patternType = ChildNodes[1].Forge.EvaluationType;
            if (patternType != typeof(string)) {
                throw new ExprValidationException("The regexp operator requires a String-type pattern expression");
            }

            var constantPattern = ChildNodes[1].Forge.ForgeConstantType.IsCompileTimeConstant;

            // check eval child node - can be String or numeric
            var evalType = ChildNodes[0].Forge.EvaluationType;
            var isNumericValue = TypeHelper.IsNumeric(evalType);
            if ((evalType != typeof(string)) && (!isNumericValue)) {
                throw new ExprValidationException(
                    "The regexp operator requires a String or numeric type left-hand expression");
            }

            if (constantPattern) {
                var patternText = (string) ChildNodes[1].Forge.ExprEvaluator.Evaluate(null, true, null);

                Regex pattern;
                try {
                    pattern = RegexExtensions.Compile(patternText, out patternText);
                }
                catch (ArgumentException ex) {
                    throw new ExprValidationException(
                        "Failed to compile regex pattern '" + patternText + "': " + ex.Message,
                        ex);
                }

                var patternInit = NewInstance<Regex>(Constant(patternText));
                _forge = new ExprRegexpNodeForgeConst(this, isNumericValue, pattern, patternInit);
            }
            else {
                _forge = new ExprRegexpNodeForgeNonconst(this, isNumericValue);
            }

            return null;
        }

        public Type Type {
            get => typeof(bool?);
        }

        public bool IsConstantResult {
            get => false;
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprRegexpNode)) {
                return false;
            }

            var other = (ExprRegexpNode) node;
            if (_isNot != other._isNot) {
                return false;
            }

            return true;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            ChildNodes[0].ToEPL(writer, Precedence, flags);
            if (_isNot) {
                writer.Write(" not");
            }

            writer.Write(" regexp ");
            ChildNodes[1].ToEPL(writer, Precedence, flags);
        }

        public override ExprPrecedenceEnum Precedence {
            get => ExprPrecedenceEnum.RELATIONAL_BETWEEN_IN;
        }

        /// <summary>
        /// Returns true if this is a "not regexp", or false if just a regexp
        /// </summary>
        /// <returns>indicator whether negated or not</returns>
        public bool IsNot {
            get => _isNot;
        }
    }
} // end of namespace