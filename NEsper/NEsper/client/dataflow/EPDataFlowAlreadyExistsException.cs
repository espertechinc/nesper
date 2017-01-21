///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.dataflow
{
    /// <summary>
    /// Thrown to indicate a data flow saved configuration already exists.
    /// </summary>
    public class EPDataFlowAlreadyExistsException : EPException
    {
        /// <summary>Ctor. </summary>
        /// <param name="message">error message</param>
        public EPDataFlowAlreadyExistsException(String message)
            : base(message)
        {
        }
    }
}