///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.settings;
using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.@event.bean.instantiator
{
    public class BeanInstantiatorFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static BeanInstantiatorForge MakeInstantiator(
            BeanEventType beanEventType,
            ImportService importService)
        {
            // see if we use a factory method
            if (beanEventType.FactoryMethodName != null) {
                return ResolveFactoryMethod(beanEventType, importService);
            }

            // find public ctor
            ImportException ctorNotFoundEx;
            try {
                importService.ResolveCtor(beanEventType.UnderlyingType, new Type[0]);
                return new BeanInstantiatorForgeByNewInstanceReflection(beanEventType.UnderlyingType);
            }
            catch (ImportException ex) {
                ctorNotFoundEx = ex;
            }

            throw new EventBeanManufactureException(
                "Failed to find no-arg constructor and no factory method has been configured to instantiate object of type " +
                beanEventType.UnderlyingType.CleanName(),
                ctorNotFoundEx);
        }

        private static BeanInstantiatorForge ResolveFactoryMethod(
            BeanEventType beanEventType,
            ImportService importService)
        {
            var factoryMethodName = beanEventType.FactoryMethodName;

            var lastDotIndex = factoryMethodName.LastIndexOf('.');
            if (lastDotIndex == -1) {
                try {
                    var method = importService.ResolveMethod(
                        beanEventType.UnderlyingType,
                        factoryMethodName,
                        new Type[0],
                        new bool[0],
                        new bool[0]);
                    return new BeanInstantiatorForgeByReflection(method);
                }
                catch (ImportException e) {
                    var message = "Failed to resolve configured factory method '" +
                                  factoryMethodName +
                                  "' expected to exist for class '" +
                                  beanEventType.UnderlyingType +
                                  "'";
                    Log.Info(message, e);
                    throw new EventBeanManufactureException(message, e);
                }
            }

            var className = factoryMethodName.Substring(0, lastDotIndex);
            var methodName = factoryMethodName.Substring(lastDotIndex + 1);
            try {
                var method = importService.ResolveMethodOverloadChecked(
                    className,
                    methodName,
                    new Type[0],
                    new bool[0],
                    new bool[0]);
                return new BeanInstantiatorForgeByReflection(method);
            }
            catch (ImportException e) {
                var message = "Failed to resolve configured factory method '" +
                              methodName +
                              "' expected to exist for class '" +
                              className +
                              "'";
                Log.Info(message, e);
                throw new EventBeanManufactureException(message, e);
            }
        }
    }
} // end of namespace