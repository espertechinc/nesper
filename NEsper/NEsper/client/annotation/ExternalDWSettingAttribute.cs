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
    /// <summary>Annotation for defining a external data window settings. </summary>
    public class ExternalDWSettingAttribute : Attribute
    {
        public ExternalDWSettingAttribute()
        {
            Iterable = true;
            FunctionLookupCompleted = string.Empty;
        }

        /// <summary>
        /// Indicator whether iterable or not.
        /// </summary>
        /// <value><c>true</c> if iterable; otherwise, <c>false</c>.</value>
        /// <returns>iterable flag</returns>
        public bool Iterable { get; set; }

        /// <summary>
        /// Function name to invoke when a lookup completed.
        /// </summary>
        /// <value>The function lookup completed.</value>
        /// <returns>function name</returns>
        public string FunctionLookupCompleted { get; set; }
    }
}