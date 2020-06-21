///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.@internal.rettype;

namespace com.espertech.esper.common.client.hook.enummethod
{
	/// <summary>
	///     Provides information about the public static method that implements the logic for the enumeration method.
	/// </summary>
	public class EnumMethodModeStaticMethod : EnumMethodMode
    {
	    /// <summary>
	    ///     Ctor.
	    /// </summary>
	    public EnumMethodModeStaticMethod()
        {
        }

	    /// <summary>
	    ///     Ctor.
	    /// </summary>
	    /// <param name="stateClass">class</param>
	    /// <param name="serviceClass">class</param>
	    /// <param name="methodName">method</param>
	    /// <param name="returnType">return type</param>
	    /// <param name="earlyExit">early-exit indicator, when the compiler should generate code to check for early-exit by calling the "completed" method of the state</param>
	    public EnumMethodModeStaticMethod(
            Type stateClass,
            Type serviceClass,
            string methodName,
            EPType returnType,
            bool earlyExit)
        {
            if (stateClass == null) {
                throw new InvalidParameterException("Required parameter state-class is not provided");
            }

            if (serviceClass == null) {
                throw new InvalidParameterException("Required parameter service-class is not provided");
            }

            if (methodName == null) {
                throw new InvalidParameterException("Required parameter method-name is not provided");
            }

            if (returnType == null) {
                throw new InvalidParameterException("Required parameter return-type is not provided");
            }

            StateClass = stateClass;
            ServiceClass = serviceClass;
            MethodName = methodName;
            ReturnType = returnType;
            IsEarlyExit = earlyExit;
        }

	    /// <summary>
	    ///     Returns the method name of the public static processing method provided by the service class
	    /// </summary>
	    /// <returns>method</returns>
	    public string MethodName { get; set; }

	    /// <summary>
	    ///     Returns the class providing state
	    /// </summary>
	    /// <returns>state class</returns>
	    public Type StateClass { get; set; }

	    /// <summary>
	    ///     Returns the class providing the processing method
	    /// </summary>
	    /// <returns>class providing the public static processing method</returns>
	    public Type ServiceClass { get; set; }

	    /// <summary>
	    ///     Returns the return type of the enumeration method.
	    /// </summary>
	    /// <returns>type</returns>
	    public EPType ReturnType { get; set; }

	    /// <summary>
	    ///     Returns indicator whether the compiler should consider the
	    ///     enumeration method as doing early-exit checking
	    /// </summary>
	    /// <returns>early-exit indicator</returns>
	    public bool IsEarlyExit { get; set; }

	    /// <summary>
	    ///     Returns the function that determines, for each lambda parameter, the lambda parameter type.
	    ///     <para>
	    ///         This function defaults to a function that assumes a value-type for all lambda parameters.
	    ///     </para>
	    /// </summary>
	    /// <value>function</value>
	    public Func<EnumMethodLambdaParameterDescriptor, EnumMethodLambdaParameterType> LambdaParameters { get; set; } =
            enumMethodLambdaParameterDescriptor => EnumMethodLambdaParameterTypeValue.INSTANCE;
    }
} // end of namespace