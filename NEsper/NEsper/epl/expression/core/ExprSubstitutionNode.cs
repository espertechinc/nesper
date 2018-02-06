///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.client;
using com.espertech.esper.compat;

namespace com.espertech.esper.epl.expression.core
{
    /// <summary>
    /// Represents a substitution value to be substituted in an expression tree, not valid for any purpose of use as an expression, however can take a place in an expression tree.
    /// </summary>
    [Serializable]
    public class ExprSubstitutionNode : ExprNodeBase
    {
        private const String ERROR_MSG = "Invalid use of substitution parameters marked by '?' in statement, use the prepare method to prepare statements with substitution parameters";

        /// <summary>
        /// Initializes a new instance of the <see cref="ExprSubstitutionNode"/> class.
        /// </summary>
        /// <param name="index">the index of the substitution parameter</param>
        public ExprSubstitutionNode(int index)
        {
            Index = index;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExprSubstitutionNode"/> class.
        /// </summary>
        /// <param name="name">the name of the substitution parameter</param>
        public ExprSubstitutionNode(string name)
        {
            Name = name;
        }

        public override ExprNode Validate(ExprValidationContext validationContext)
        {
            throw new ExprValidationException(ERROR_MSG);
        }

        /// <summary>
        /// Returns the substitution parameter index (or null if by-name).
        /// </summary>
        public int? Index { get; set; }

        /// <summary>
        /// Returns the substitution parameter name (or null if by-index).
        /// </summary>
        public String Name { get; set; }

        public override bool IsConstantResult
        {
            get { return false; }
        }

        public Type ReturnType
        {
            get { throw new IllegalStateException(ERROR_MSG); }
        }

        public Object Evaluate(EventBean[] eventsPerStream, bool isNewData, ExprEvaluatorContext exprEvaluatorContext)
        {
            throw new EPException(ERROR_MSG);
        }

        public override ExprEvaluator ExprEvaluator
        {
            get { throw new EPException(ERROR_MSG); }
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write('?');
        }

        public override ExprPrecedenceEnum Precedence
        {
            get { return ExprPrecedenceEnum.UNARY; }
        }

        public override bool EqualsNode(ExprNode node, bool ignoreStreamPrefix)
        {
            return (node is ExprSubstitutionNode);
        }
    }
}
