///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2017 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.events.bean
{
    public class BeanInstantiatorByNewInstanceReflection : BeanInstantiator {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    
        private readonly Type clazz;
    
        public BeanInstantiatorByNewInstanceReflection(Type clazz) {
            this.clazz = clazz;
        }
    
        public Object Instantiate() {
            try {
                return Clazz.NewInstance();
            } catch (IllegalAccessException e) {
                return Handle(e);
            } catch (InstantiationException e) {
                return Handle(e);
            }
        }
    
        private Object Handle(Exception e) {
            string message = "Unexpected exception encountered invoking newInstance on class '" + clazz.Name + "': " + e.Message;
            Log.Error(message, e);
            return null;
        }
    }
} // end of namespace
