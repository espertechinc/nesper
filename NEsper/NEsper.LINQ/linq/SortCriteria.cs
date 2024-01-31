///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client.soda;

namespace com.espertech.esper.runtime.client.linq
{
    public class SortCriteria
    {
        /// <summary>
        /// Gets or sets the property.
        /// </summary>
        /// <value>
        /// The property.
        /// </value>
        public string Property { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="SortCriteria"/> is ascending.
        /// </summary>
        /// <value>
        ///   <c>true</c> if ascending; otherwise, <c>false</c>.
        /// </value>
        public bool Ascending { get; set; }

        /// <summary>
        /// Returns the SODA variant of the object.
        /// </summary>
        /// <returns></returns>
        public OrderedObjectParamExpression ToSodaExpression()
        {
            var expression = new OrderedObjectParamExpression(!Ascending);
            expression.AddChild(new PropertyValueExpression(Property));
            return expression;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SortCriteria"/> class.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="ascending">if set to <c>true</c> [ascending].</param>
        public SortCriteria(string property, bool @ascending)
        {
            Property = property;
            Ascending = @ascending;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SortCriteria"/> class.
        /// </summary>
        /// <param name="property">The property.</param>
        public SortCriteria(string property)
        {
            Property = property;
            Ascending = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SortCriteria"/> class.
        /// </summary>
        public SortCriteria()
        {
            Ascending = true;
        }
    }
}