///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.epl.expression.core;
using com.espertech.esper.util;

namespace com.espertech.esper.epl.spec
{
    /// <summary>
    /// Filter definition in an un-validated and un-resolved form.
    /// <para/>
    /// Event type and expression nodes in this filter specification are not yet
    /// validated, optimized for resolved against actual streams.
    /// </summary>
    [Serializable]
    public class FilterSpecRaw : MetaDefItem
    {
        private readonly String _eventTypeName;
        private readonly IList<ExprNode> _filterExpressions;
        private readonly PropertyEvalSpec _optionalPropertyEvalSpec;
    
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="eventTypeName">is the name of the event type</param>
        /// <param name="filterExpressions">is a list of expression nodes representing individual filter expressions</param>
        /// <param name="optionalPropertyEvalSpec">specification for a property select</param>
        public FilterSpecRaw(String eventTypeName, IList<ExprNode> filterExpressions, PropertyEvalSpec optionalPropertyEvalSpec)
        {
            this._eventTypeName = eventTypeName;
            this._filterExpressions = filterExpressions;
            this._optionalPropertyEvalSpec = optionalPropertyEvalSpec;
        }
    
        /// <summary>
        /// Default ctor.
        /// </summary>
        public FilterSpecRaw()
        {
        }

        /// <summary>
        /// Returns the event type name of the events we are looking for.
        /// </summary>
        /// <returns>
        /// event name
        /// </returns>
        public string EventTypeName
        {
            get { return _eventTypeName; }
        }

        /// <summary>
        /// Returns the list of filter expressions.
        /// </summary>
        /// <returns>
        /// filter expression list
        /// </returns>
        public IList<ExprNode> FilterExpressions
        {
            get { return _filterExpressions; }
        }

        /// <summary>
        /// Returns the property evaluation specification, if any, or null if no properties
        /// evaluated.
        /// </summary>
        /// <returns>
        /// property eval spec
        /// </returns>
        public PropertyEvalSpec OptionalPropertyEvalSpec
        {
            get { return _optionalPropertyEvalSpec; }
        }
    }
}
