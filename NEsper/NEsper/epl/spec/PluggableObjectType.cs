///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.epl.spec
{
    /// <summary>Enumeration for types of plug-in objects. </summary>
    public enum PluggableObjectType
    {
        /// <summary>Pattern guard object type. </summary>
        PATTERN_GUARD,
    
        /// <summary>Pattern observer object type. </summary>
        PATTERN_OBSERVER,
    
        /// <summary>View object type. </summary>
        VIEW,
    
        /// <summary>Virtual data window object type. </summary>
        VIRTUALDW
    }
}
