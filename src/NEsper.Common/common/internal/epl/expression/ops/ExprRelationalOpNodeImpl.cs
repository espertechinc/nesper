///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Text.Json.Serialization;

using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.type;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;


namespace com.espertech.esper.common.@internal.epl.expression.ops
{
    /// <summary>
    /// Represents a lesser or greater then (&lt;/&lt;=/&gt;/&gt;=) expression in a filter expression tree.
    /// </summary>
    public class ExprRelationalOpNodeImpl : ExprNodeBase,
        ExprRelationalOpNode
    {
        private readonly RelationalOpEnum relationalOpEnum;
        [JsonIgnore]
        [NonSerialized]
        private ExprRelationalOpNodeForge forge;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name = "relationalOpEnum">type of compare, ie. lt, gt, le, ge</param>
        public ExprRelationalOpNodeImpl(RelationalOpEnum relationalOpEnum)
        {
            this.relationalOpEnum = relationalOpEnum;
        }

        public bool IsConstantResult => false;

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            // Must have 2 child nodes
            if (ChildNodes.Length != 2) {
                throw new IllegalStateException("Relational op node does not have exactly 2 parameters");
            }

            // Must be either numeric or string
            var lhsForge = ChildNodes[0].Forge;
            var rhsForge = ChildNodes[1].Forge;
            var lhsClass = ValidateStringOrNumeric(lhsForge);
            var rhsClass = ValidateStringOrNumeric(rhsForge);
            if (lhsClass != typeof(string) || rhsClass != typeof(string)) {
                if (!lhsClass.IsTypeNumeric()) {
                    throw new ExprValidationException(
                        "Implicit conversion from datatype '" + lhsClass + "' to numeric is not allowed");
                }

                if (!rhsClass.IsTypeNumeric()) {
                    throw new ExprValidationException(
                        "Implicit conversion from datatype '" + rhsClass + "' to numeric is not allowed");
                }
            }

            var coercionType = lhsClass.GetCompareToCoercionType(rhsClass);
            var computer = relationalOpEnum.GetComputer(coercionType, lhsClass, rhsClass);
            forge = new ExprRelationalOpNodeForge(this, computer, coercionType);
            return null;
        }

        public override void ToPrecedenceFreeEPL(
            TextWriter writer,
            ExprNodeRenderableFlags flags)
        {
            ChildNodes[0].ToEPL(writer, Precedence, flags);
            writer.Write(relationalOpEnum.GetExpressionText());
            ChildNodes[1].ToEPL(writer, Precedence, flags);
        }

        public override ExprPrecedenceEnum Precedence => ExprPrecedenceEnum.RELATIONAL_BETWEEN_IN;

        public override bool EqualsNode(
            ExprNode node,
            bool ignoreStreamPrefix)
        {
            if (!(node is ExprRelationalOpNodeImpl other)) {
                return false;
            }

            if (other.relationalOpEnum != relationalOpEnum) {
                return false;
            }

            return true;
        }

        private Type ValidateStringOrNumeric(ExprForge forge)
        {
            var type = forge.EvaluationType;
            if (type == null) {
                throw new ExprValidationException("Null-type value is not allow for relational operator");
            }

            var typeClass = type;
            if (typeClass == typeof(string)) {
                return typeClass;
            }

            return ExprNodeUtilityValidate.ValidateReturnsNumeric(forge).GetBoxedType();
        }

        public ExprEvaluator ExprEvaluator {
            get {
                CheckValidated(forge);
                return forge.ExprEvaluator;
            }
        }

        public override ExprForge Forge {
            get {
                CheckValidated(forge);
                return forge;
            }
        }

        public RelationalOpEnum RelationalOpEnum => relationalOpEnum;
    }
} // end of namespace