///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    ///     Guard is the where timer-within pattern object for use in pattern expressions.
    /// </summary>
    [Serializable]
    public class PatternGuardExpr : EPBaseNamedObject,
        PatternExpr
    {
        private IList<PatternExpr> guarded;
        private string treeObjectName;

        /// <summary>
        ///     Ctor - for use to create a pattern expression tree, without pattern child expression.
        /// </summary>
        /// <param name="namespace">is the guard object namespace</param>
        /// <param name="name">is the guard object name</param>
        /// <param name="parameters">is guard object parameters</param>
        public PatternGuardExpr(
            string @namespace,
            string name,
            IList<Expression> parameters)
            : base(@namespace, name, parameters)
        {
            guarded = new List<PatternExpr>();
        }

        /// <summary>
        ///     Ctor - for use to create a pattern expression tree, without pattern child expression.
        /// </summary>
        /// <param name="namespace">is the guard object namespace</param>
        /// <param name="name">is the guard object name</param>
        /// <param name="parameters">is guard object parameters</param>
        /// <param name="guarded">is the guarded pattern expression</param>
        public PatternGuardExpr(
            string @namespace,
            string name,
            Expression[] parameters,
            PatternExpr guarded)
            : this(@namespace, name, (IList<Expression>)parameters, guarded)
        {
        }

        /// <summary>
        ///     Ctor - for use to create a pattern expression tree, without pattern child expression.
        /// </summary>
        /// <param name="namespace">is the guard object namespace</param>
        /// <param name="name">is the guard object name</param>
        /// <param name="parameters">is guard object parameters</param>
        /// <param name="guardedPattern">is the guarded pattern expression</param>
        public PatternGuardExpr(
            string @namespace,
            string name,
            IList<Expression> parameters,
            PatternExpr guardedPattern)
            : base(@namespace, name, parameters)
        {
            guarded = new List<PatternExpr>();
            guarded.Add(guardedPattern);
        }

        /// <summary>
        ///     Get sub expression
        /// </summary>
        /// <returns>sub pattern</returns>
        public IList<PatternExpr> Guarded {
            get => guarded;
            set => guarded = value;
        }

        public IList<PatternExpr> Children {
            get => guarded;
            set => guarded = value;
        }

        public string TreeObjectName {
            get => treeObjectName;
            set => treeObjectName = value;
        }

        public PatternExprPrecedenceEnum Precedence => PatternExprPrecedenceEnum.GUARD;

        public void ToEPL(
            TextWriter writer,
            PatternExprPrecedenceEnum parentPrecedence,
            EPStatementFormatter formatter)
        {
            if (Precedence.GetLevel() < parentPrecedence.GetLevel()) {
                writer.Write("(");
                ToPrecedenceFreeEPL(writer, formatter);
                writer.Write(")");
            }
            else {
                ToPrecedenceFreeEPL(writer, formatter);
            }
        }

        /// <summary>
        ///     Renders the expressions and all it's child expression, in full tree depth, as a string in
        ///     language syntax.
        /// </summary>
        /// <param name="writer">is the output to use</param>
        /// <param name="formatter">for newline-whitespace formatting</param>
        public void ToPrecedenceFreeEPL(
            TextWriter writer,
            EPStatementFormatter formatter)
        {
            guarded[0].ToEPL(writer, Precedence, formatter);
            if (GuardEnumExtensions.IsWhile(Namespace, Name)) {
                writer.Write(" while (");
                Parameters[0].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                writer.Write(")");
            }
            else {
                writer.Write(" where ");
                base.ToEPL(writer);
            }
        }
    }
} // end of namespace