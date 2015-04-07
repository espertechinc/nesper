///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.util
{
    /// <summary>
    /// Widerner that coerces to a widened boxed number.
    /// </summary>
    public class TypeWidenerBoxedNumeric
    {
        private readonly Coercer _coercer;
    
        /// <summary>Ctor. </summary>
        /// <param name="coercer">the coercer</param>
        public TypeWidenerBoxedNumeric(Coercer coercer)
        {
            _coercer = coercer;
        }
    
        public Object Widen(Object input)
        {
           return _coercer.Invoke(input);
        }
    }
}
