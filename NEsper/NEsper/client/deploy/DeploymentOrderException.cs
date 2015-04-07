///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.client.deploy
{
    /// <summary>
    /// Exception indicates a problem when determining delpoyment order and uses-dependency checking.
    /// </summary>
    [Serializable]
    public class DeploymentOrderException : DeploymentException
    {
        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="message">error message</param>
        public DeploymentOrderException(String message)
            : base(message)
        {
        }
    }
}
