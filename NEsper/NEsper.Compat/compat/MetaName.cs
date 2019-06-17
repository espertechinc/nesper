///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.compat
{
    public class MetaName
    {
        public String Name { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaName"/> class.
        /// </summary>
        public MetaName()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetaName"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public MetaName(string name)
        {
            Name = name;
        }
    }
}
