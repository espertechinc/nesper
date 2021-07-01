///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Named parameter expression of the form "name:expression" or "name:(expression, expression...)"
    /// </summary>
    [Serializable]
    public class NamedParameterExpression : ExpressionBase
    {
        private string name;

        /// <summary>
        /// Ctor.
        /// </summary>
        public NamedParameterExpression()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="name">substitution parameter name</param>
        public NamedParameterExpression(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Returns the parameter name.
        /// </summary>
        /// <returns>name</returns>
        public string Name {
            get => name;
            set => name = value;
        }

        /// <summary>
        /// Sets the parameter name.
        /// </summary>
        /// <param name="name">name to set</param>
        public NamedParameterExpression SetName(string name)
        {
            this.name = name;
            return this;
        }

        public override ExpressionPrecedenceEnum Precedence
        {
            get => ExpressionPrecedenceEnum.UNARY;
        }

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            writer.Write(name);
            writer.Write(':');
            if (Children.Count > 1 || Children.IsEmpty())
            {
                writer.Write('(');
            }

            string delimiter = "";
            foreach (Expression expr in Children)
            {
                writer.Write(delimiter);
                expr.ToEPL(writer, ExpressionPrecedenceEnum.MINIMUM);
                delimiter = ",";
            }

            if (Children.Count > 1 || Children.IsEmpty())
            {
                writer.Write(')');
            }
        }
    }
} // end of namespace