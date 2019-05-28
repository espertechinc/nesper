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

using com.espertech.esper.common.@internal.bytecodemodel.model.expression;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.util;

using static com.espertech.esper.common.@internal.bytecodemodel.model.expression.CodegenExpressionBuilder;

namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    /// Represents the regexp-clause in an expression tree.
    /// </summary>
    [Serializable]
    public class ExprRegexpNode : ExprNodeBase
    {
        private readonly bool isNot;

        [NonSerialized] private ExprRegexpNodeForge forge;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="not">is true if the it's a "not regexp" expression, of false for regular regexp</param>
        public ExprRegexpNode(bool not)
        {
            this.isNot = not;
        }

        public ExprEvaluator ExprEvaluator
        {
            get {
                CheckValidated(forge);
                return forge.ExprEvaluator;
            }
        }

        public override ExprForge Forge
        {
            get {
                CheckValidated(forge);
                return forge;
            }
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            if (this.ChildNodes.Length != 2)
            {
                throw new ExprValidationException("The regexp operator requires 2 child expressions");
            }

            // check pattern child node
            var patternChildType = ChildNodes[1].Forge.EvaluationType;
            if (patternChildType != typeof(string))
            {
                throw new ExprValidationException("The regexp operator requires a String-type pattern expression");
            }

            var constantPattern = this.ChildNodes[1].Forge.ForgeConstantType.IsCompileTimeConstant;

            // check eval child node - can be String or numeric
            var evalChildType = ChildNodes[0].Forge.EvaluationType;
            var isNumericValue = TypeHelper.IsNumeric(evalChildType);
            if ((evalChildType != typeof(string)) && (!isNumericValue))
            {
                throw new ExprValidationException(
                    "The regexp operator requires a String or numeric type left-hand expression");
            }

            if (constantPattern)
            {
                var patternText = (string) ChildNodes[1].Forge.ExprEvaluator.Evaluate(null, true, null);
                Regex pattern;
                try
                {
                    pattern = new Regex(patternText);
                }
                catch (ArgumentException ex)
                {
                    throw new ExprValidationException(
                        "Error compiling regex pattern '" + patternText + "': " + ex.Message, ex);
                }

                var patternInit = NewInstance<Regex>(Constant(patternText));
                forge = new ExprRegexpNodeForgeConst(this, isNumericValue, pattern, patternInit);
            }
            else
            {
                forge = new ExprRegexpNodeForgeNonconst(this, isNumericValue);
            }

            return null;
        }

        public Type Type
        {
            get => typeof(bool?);
        }

        public bool IsConstantResult
        {
            get => false;
        }

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprRegexpNode))
            {
                return false;
            }

            var other = (ExprRegexpNode) node;
            if (this.isNot != other.isNot)
            {
                return false;
            }

            return true;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            this.ChildNodes[0].ToEPL(writer, Precedence);
            if (isNot)
            {
                writer.Write(" not");
            }

            writer.Write(" regexp ");
            this.ChildNodes[1].ToEPL(writer, Precedence);
        }

        public override ExprPrecedenceEnum Precedence
        {
            get => ExprPrecedenceEnum.RELATIONAL_BETWEEN_IN;
        }

        /// <summary>
        /// Returns true if this is a "not regexp", or false if just a regexp
        /// </summary>
        /// <returns>indicator whether negated or not</returns>
        public bool IsNot
        {
            get => isNot;
        }
    }
} // end of namespace