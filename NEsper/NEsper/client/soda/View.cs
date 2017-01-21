///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

namespace com.espertech.esper.client.soda
{
    /// <summary>
    /// A view provides a projection upon a stream, such as a data window, grouping or unique.
    /// </summary>
    [Serializable]
    public class View : EPBaseNamedObject
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="View"/> class.
        /// </summary>
        public View()
        {
        }

        /// <summary>
        /// Creates a view.
        /// </summary>
        /// <param name="namespace">the view namespace, i.e. "win" for data windows</param>
        /// <param name="name">the view name, i.e. "length" for length window</param>
        /// <returns>view</returns>
	    public static View Create(String @namespace, String name)
	    {
            return new View(@namespace, name, new List<Expression>());
	    }

        /// <summary>
        /// Creates a view.
        /// </summary>
        /// <param name="namespace">the view namespace, i.e. "win" for data windows</param>
        /// <param name="name">the view name, i.e. "length" for length window</param>
        /// <param name="parameters">a list of view parameters, or empty if there are no parameters for the view</param>
        /// <returns>view</returns>
        public static View Create(String @namespace, String name, IList<Expression> parameters)
	    {
	        return new View(@namespace, name, parameters);
	    }

        /// <summary>
        /// Creates a view.
        /// </summary>
        /// <param name="namespace">the view namespace, i.e. "win" for data windows</param>
        /// <param name="name">the view name, i.e. "length" for length window</param>
        /// <param name="parameters">a list of view parameters, or empty if there are no parameters for the view</param>
        /// <returns>view</returns>
        public static View Create(String @namespace, String name, params Expression[] parameters)
        {
            return parameters != null 
                ? new View(@namespace, name, parameters)
                : new View(@namespace, name, new Expression[] { });
        }

	    /// <summary>
        /// Creates a view.
        /// </summary>
        /// <param name="namespace">the view namespace, i.e. "win" for data windows</param>
        /// <param name="name">the view name, i.e. "length" for length window</param>
        /// <param name="parameters">a list of view parameters, or empty if there are no parameters for the view</param>
        public View(String @namespace, String name, IList<Expression> parameters)
	    	: base(@namespace, name, parameters)
	    {
	    }
	}
} // End of namespace
