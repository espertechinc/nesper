///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.annotation
{
    /// <summary>Annotation for defining an external data window name and open/close functon. </summary>
    public class ExternalDWAttribute : Attribute
    {
        public ExternalDWAttribute()
        {
            FunctionOpen = string.Empty;
            FunctionClose = string.Empty;
            IsUnique = false;
        }

        /// <summary>
        /// Name
        /// </summary>
        /// <value>The name.</value>
        /// <returns>name</returns>
        public String Name { get; set; }

        /// <summary>
        /// Open function.
        /// </summary>
        /// <value>The function open.</value>
        /// <returns>open function.</returns>
        public String FunctionOpen { get; set; }

        /// <summary>
        /// Close function.
        /// </summary>
        /// <value>The function close.</value>
        /// <returns>close function</returns>
        public String FunctionClose { get; set; }

        /// <summary>
        /// Indicator whether unique-key semantics should apply.
        /// <para/>
        /// This indicator is false by default meaning that the implementation should not 
        /// assume unique-data-window semantics, and would not need to post the previous value 
        /// of the key as a remove stream event. 
        /// <para />
        /// Setting this indicator is interpreted by an implementation to assume unique-data-window 
        /// semantics, thereby instructing to post the previous value for the currently-updated key 
        /// as a remove stream event.
        /// </summary>
        /// <value><c>true</c> if this instance is unique; otherwise, <c>false</c>.</value>
        /// <returns>unique-key semantics</returns>
        public bool IsUnique { get; set; }
    }
}