///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.util;

namespace com.espertech.esper.runtime.client
{
    /// <summary>
    ///     The statement is the means to attach callbacks to receive statement results (push, observer)
    ///     and to object current results using pull.
    /// </summary>
    public interface EPStatement : EPListenable,
        EPIterable
    {
        /// <summary>
        ///     Returns the type of events the statement pushes to listeners or returns for iterator.
        /// </summary>
        /// <returns>event type of events the iterator returns and that listeners receive</returns>
        EventType EventType { get; }

        /// <summary>
        ///     Returns statement annotations.
        ///     <para>
        ///         See the annotation <seealso cref="com.espertech.esper.common.client.annotation" /> package
        ///         for available annotations. Application can define their own annotations.
        ///     </para>
        /// </summary>
        /// <returns>annotations or a zero-length array if no annotations have been specified.</returns>
        Attribute[] Annotations { get; }

        /// <summary>
        ///     Returns the statement name.
        /// </summary>
        /// <returns>statement name</returns>
        string Name { get; }

        /// <summary>
        ///     Returns the deployment id.
        /// </summary>
        /// <returns>deployment id</returns>
        string DeploymentId { get; }

        /// <summary>
        ///     Returns true if the statement has been undeployed.
        /// </summary>
        /// <value>true for undeployed statements, false for deployed statements.</value>
        bool IsDestroyed { get; }

        /// <summary>
        ///     Returns the current subscriber instance that receives statement results.
        /// </summary>
        /// <returns>subscriber object, or null to indicate that no subscriber is attached</returns>
        object Subscriber { get; set; }

        /// <summary>
        ///     Returns the application defined user data object associated with the statement at
        ///     compile time, or null if none was supplied at time of statement compilation.
        ///     <para>
        ///         The <em>user object</em> is a single, unnamed field that is stored with every statement.
        ///         Applications may put arbitrary objects in this field or a null value.
        ///     </para>
        ///     <para>User objects are passed at time of statement compilation via options.</para>
        /// </summary>
        /// <returns>user object or null if none defined</returns>
        object UserObjectCompileTime { get; }

        /// <summary>
        ///     Returns the application defined user data object associated with the statement at
        ///     deployment time, or null if none was supplied at time of deployment.
        ///     <para>
        ///         The <em>user object</em> is a single, unnamed field that is stored with every statement.
        ///         Applications may put arbitrary objects in this field or a null value.
        ///     </para>
        ///     <para>User objects are passed at time of deployment via options.</para>
        /// </summary>
        /// <returns>user object or null if none defined</returns>
        object UserObjectRuntime { get; }

        /// <summary>
        ///     Returns a statement property value.
        /// </summary>
        /// <param name="field">statement property value</param>
        /// <returns>property or null if not set</returns>
        object GetProperty(StatementProperty field);

        /// <summary>
        ///     Attaches a subscriber to receive statement results,
        ///     or removes a previously set subscriber (by providing a null value).
        ///     <para>
        ///         Note: Requires the allow-subscriber compiler options.
        ///         Only a single subscriber may be set for a statement. If this method is invoked twice
        ///         any previously-set subscriber is no longer used.
        ///     </para>
        /// </summary>
        /// <param name="subscriber">to attach, or null to remove the previously set subscriber</param>
        /// <throws>
        ///     EPSubscriberException if the subscriber does not provide the methods needed to receive statement results
        /// </throws>
        void SetSubscriber(object subscriber);

        /// <summary>
        ///     Attaches a subscriber to receive statement results by calling the method with the provided method name,
        ///     or removes a previously set subscriber (by providing a null value).
        ///     <para>
        ///         Note: Requires the allow-subscriber compiler options.
        ///         Only a single subscriber may be set for a statement. If this method is invoked twice
        ///         any previously-set subscriber is no longer used.
        ///     </para>
        /// </summary>
        /// <param name="subscriber">to attach, or null to remove the previously set subscriber</param>
        /// <param name="methodName">the name of the method to invoke, or null for the "update" method</param>
        /// <throws>
        ///     EPSubscriberException if the subscriber does not provide the methods needed to receive statement results
        /// </throws>
        void SetSubscriber(
            object subscriber,
            string methodName);
    }
} // end of namespace