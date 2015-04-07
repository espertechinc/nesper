///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.compat.logging;

namespace com.espertech.esper.events.bean
{
    public class BeanInstantiatorByFactoryReflection : BeanInstantiator
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly MethodInfo _method;

        public BeanInstantiatorByFactoryReflection(MethodInfo method)
        {
            _method = method;
        }
    
        public Object Instantiate() {
            try
            {
                return _method.Invoke(null, null);
            }
            catch (TargetInvocationException e)
            {
                String message = string.Format("Unexpected exception encountered invoking factory method '{0}' on class '{1}': {2}",
                    _method.Name, 
                    _method.DeclaringType.FullName,
                    e.InnerException.Message);
                Log.Error(message, e);
                return null;
            }
            catch (MemberAccessException ex) {
                String message = string.Format("Unexpected exception encountered invoking factory method '{0}' on class '{1}': {2}", 
                    _method.Name,
                    _method.DeclaringType.FullName,
                    ex.Message);
                Log.Error(message, ex);
                return null;
            }
        }
    }
}
