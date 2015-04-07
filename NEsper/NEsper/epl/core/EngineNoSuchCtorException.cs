///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

namespace com.espertech.esper.epl.core
{
    /// <summary>
    /// Exception for resolution of a method failed.
    /// </summary>
    [Serializable]
    public class EngineNoSuchCtorException : Exception
    {
        [NonSerialized]
        private readonly ConstructorInfo _nearestMissCtor;
    
        /// <summary>Ctor. </summary>
        /// <param name="message">message</param>
        /// <param name="nearestMissCtor">best-match method</param>
        public EngineNoSuchCtorException(String message, ConstructorInfo nearestMissCtor)
            : base(message)
        {
            _nearestMissCtor = nearestMissCtor;
        }

        /// <summary>Returns the best-match ctor. </summary>
        /// <value>ctor</value>
        public ConstructorInfo NearestMissCtor
        {
            get { return _nearestMissCtor; }
        }
    }
}
