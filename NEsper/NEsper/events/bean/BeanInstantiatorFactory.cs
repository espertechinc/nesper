///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using XLR8.CGLib;

using com.espertech.esper.compat.logging;
using com.espertech.esper.epl.core;

namespace com.espertech.esper.events.bean
{
    public class BeanInstantiatorFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static BeanInstantiator MakeInstantiator(BeanEventType beanEventType, EngineImportService engineImportService)
        {
            // see if we use a factory method
            if (beanEventType.FactoryMethodName != null)
            {
                return ResolveFactoryMethod(beanEventType, engineImportService);
            }

            // find public ctor
            EngineImportException ctorNotFoundEx;
            try
            {
                engineImportService.ResolveCtor(beanEventType.UnderlyingType, new Type[0]);
                if (beanEventType.FastClass != null)
                {
                    return new BeanInstantiatorByNewInstanceFastClass(beanEventType.FastClass);
                }
                else
                {
                    return new BeanInstantiatorByNewInstanceReflection(beanEventType.UnderlyingType);
                }
            }
            catch (EngineImportException ex)
            {
                ctorNotFoundEx = ex;
            }

            // not found ctor, see if FastClass can handle
            if (beanEventType.FastClass != null)
            {
                var fastClass = beanEventType.FastClass;
                try
                {
                    fastClass.NewInstance();
                    return new BeanInstantiatorByNewInstanceFastClass(beanEventType.FastClass);
                }
                catch (TargetInvocationException e)
                {
                    string message = string.Format(
                        "Failed to instantiate class '{0}', define a factory method if the class has no suitable constructors: {1}",
                        fastClass.TargetType.FullName,
                        (e.InnerException ?? e).Message);
                    Log.Debug(message, e);
                }
                catch (ArgumentException e)
                {
                    string message =
                        string.Format(
                            "Failed to instantiate class '{0}', define a factory method if the class has no suitable constructors",
                            fastClass.TargetType.FullName);
                    Log.Debug(message, e);
                }
            }

            throw new EventBeanManufactureException(
                "Failed to find no-arg constructor and no factory method has been configured and cannot use reflection to instantiate object of type " +
                beanEventType.UnderlyingType.Name, ctorNotFoundEx);
        }

        private static BeanInstantiator ResolveFactoryMethod(BeanEventType beanEventType,
                                                             EngineImportService engineImportService)
        {
            string factoryMethodName = beanEventType.FactoryMethodName;

            int lastDotIndex = factoryMethodName.LastIndexOf('.');
            if (lastDotIndex == -1)
            {
                try
                {
                    MethodInfo method = engineImportService.ResolveMethod(
                        beanEventType.UnderlyingType, factoryMethodName, new Type[0], new bool[0], new bool[0]);
                    if (beanEventType.FastClass != null)
                    {
                        return new BeanInstantiatorByFactoryFastClass(beanEventType.FastClass.GetMethod(method));
                    }
                    else
                    {
                        return new BeanInstantiatorByFactoryReflection(method);
                    }
                }
                catch (EngineImportException e)
                {
                    string message =
                        string.Format(
                            "Failed to resolve configured factory method '{0}' expected to exist for class '{1}'",
                            factoryMethodName, beanEventType.UnderlyingType);
                    Log.Info(message, e);
                    throw new EventBeanManufactureException(message, e);
                }
            }

            string className = factoryMethodName.Substring(0, lastDotIndex);
            string methodName = factoryMethodName.Substring(lastDotIndex + 1);
            try
            {
                MethodInfo method = engineImportService.ResolveMethod(className, methodName, new Type[0], new bool[0], new bool[0]);
                if (beanEventType.FastClass != null)
                {
                    FastClass fastClassFactory = FastClass.Create(method.DeclaringType);
                    return new BeanInstantiatorByFactoryFastClass(fastClassFactory.GetMethod(method));
                }
                else
                {
                    return new BeanInstantiatorByFactoryReflection(method);
                }
            }
            catch (EngineImportException e)
            {
                String message = "Failed to resolve configured factory method '" + methodName +
                                 "' expected to exist for class '" + className + "'";
                Log.Info(message, e);
                throw new EventBeanManufactureException(message, e);
            }
        }
    }
}