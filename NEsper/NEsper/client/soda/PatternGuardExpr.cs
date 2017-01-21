///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.pattern.guard;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// Guard is the where timer-within pattern object for use in pattern expressions.
    /// </summary>
    [Serializable]
    public class PatternGuardExpr
        : EPBaseNamedObject
        , PatternExpr
    {
        /// <summary>Ctor - for use to create a pattern expression tree, without pattern child expression. </summary>
        /// <param name="namespace">is the guard object namespace</param>
        /// <param name="name">is the guard object name</param>
        /// <param name="parameters">is guard object parameters</param>
        public PatternGuardExpr(String @namespace, String name, IList<Expression> parameters)
            : base(@namespace, name, parameters)
        {
            Children = new List<PatternExpr>();
        }

        /// <summary>Ctor - for use to create a pattern expression tree, without pattern child expression. </summary>
        /// <param name="namespace">is the guard object namespace</param>
        /// <param name="name">is the guard object name</param>
        /// <param name="parameters">is guard object parameters</param>
        /// <param name="guarded">is the guarded pattern expression</param>
        public PatternGuardExpr(String @namespace, String name, Expression[] parameters, PatternExpr guarded)
            : base(@namespace, name, parameters)
        {
            Children = new List<PatternExpr>();
            Children.Add(guarded);
        }

        /// <summary>Ctor - for use to create a pattern expression tree, without pattern child expression. </summary>
        /// <param name="namespace">is the guard object namespace</param>
        /// <param name="name">is the guard object name</param>
        /// <param name="parameters">is guard object parameters</param>
        /// <param name="guardedPattern">is the guarded pattern expression</param>
        public PatternGuardExpr(String @namespace, String name, IList<Expression> parameters, PatternExpr guardedPattern)
            : base(@namespace, name, parameters)
        {
            Children = new List<PatternExpr>();
            Children.Add(guardedPattern);
        }

        public List<PatternExpr> Children { get; set; }

        /// <summary>Get sub expression </summary>
        /// <value>sub pattern</value>
        public List<PatternExpr> Guarded
        {
            get { return Children; }
            set { Children = value; }
        }

        public string TreeObjectName { get; set; }

        public PatternExprPrecedenceEnum Precedence
        {
            get { return PatternExprPrecedenceEnum.GUARD; }
        }

        public void ToEPL(TextWriter writer, PatternExprPrecedenceEnum parentPrecedence, EPStatementFormatter formatter)
        {
            if (Precedence.GetLevel() < parentPrecedence.GetLevel())
            {
                writer.Write("(");
                ToPrecedenceFreeEPL(writer, formatter);
                writer.Write(")");
            }
            else
            {
                ToPrecedenceFreeEPL(writer, formatter);
            }
        }

        /// <summary>
        /// Renders the expressions and all it's child expression, in full tree depth, as a string in language syntax.
        /// </summary>
        /// <param name="writer">is the output to use</param>
        /// <param name="formatter">for NewLine-whitespace formatting</param>
        public void ToPrecedenceFreeEPL(TextWriter writer, EPStatementFormatter formatter)
        {
            Children[0].ToEPL(writer, Precedence, formatter);
            if (GuardEnumExtensions.IsWhile(Namespace, Name))
            {
                writer.Write(" while (");
                Parameters[0].ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                writer.Write(")");
            }
            else
            {
                writer.Write(" where ");
                base.ToEPL(writer);
            }
        }
    }
}
