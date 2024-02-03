///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.IO;

using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.client.soda
{
    /// <summary>
    /// Dot-expresson is for use in "(inner_expression).dot_expression".
    /// </summary>
    public class DotExpression : ExpressionBase
    {
        private IList<DotExpressionItem> _chain = new List<DotExpressionItem>();

        /// <summary>
        /// Ctor.
        /// </summary>
        public DotExpression()
        {
        }

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="innerExpression">the expression in parenthesis</param>
        public DotExpression(Expression innerExpression)
        {
            Children.Add(innerExpression);
        }

        /// <summary>
        /// Add a method to the chain of methods after the dot.
        /// </summary>
        /// <param name="item">item to add</param>
        public void Add(DotExpressionItem item)
        {
            _chain.Add(item);
        }

        /// <summary>
        /// Add a method to the chain of methods after the dot.
        /// </summary>
        /// <param name="name">to add</param>
        /// <param name="parameters">parameters to method</param>
        public void Add(
            string name,
            IList<Expression> parameters)
        {
            _chain.Add(new DotExpressionItemCall(name, parameters));
        }

        /// <summary>
        /// Add a method to the chain of methods after the dot, indicating the this segment is a property and does not need parenthesis and won't have paramaters.
        /// </summary>
        /// <param name="name">method name</param>
        /// <param name="parameters">parameter expressions</param>
        /// <param name="isProperty">property flag</param>
        public void Add(
            string name,
            IList<Expression> parameters,
            bool isProperty)
        {
            if (parameters.IsEmpty() && isProperty) {
                _chain.Add(new DotExpressionItemName(name));
            }
            else {
                _chain.Add(new DotExpressionItemCall(name, parameters));
            }
        }

        /// <summary>
        /// Returns the method chain of all methods after the dot.
        /// </summary>
        /// <returns>method name ane list of parameters</returns>
        public IList<DotExpressionItem> Chain {
            get => _chain;
            set => _chain = value;
        }

        public override ExpressionPrecedenceEnum Precedence => ExpressionPrecedenceEnum.MINIMUM;

        public override void ToPrecedenceFreeEPL(TextWriter writer)
        {
            if (!Children.IsEmpty()) {
                Children[0].ToEPL(writer, Precedence);
            }

            DotExpressionItem.Render(_chain, writer, !Children.IsEmpty());
        }
    }
} // end of namespace