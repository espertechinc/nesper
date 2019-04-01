///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Reflection;

using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.logging;

namespace com.espertech.esper.common.@internal.@event.bean.core
{
	/// <summary>
	/// Copies an event for modification.
	/// </summary>
	public class BeanEventBeanConfiguredCopyMethod : EventBeanCopyMethod {
	    private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly BeanEventType beanEventType;
	    private readonly EventBeanTypedEventFactory eventAdapterService;
	    private readonly MethodInfo copyMethod;

	    /// <summary>
	    /// Ctor.
	    /// </summary>
	    /// <param name="beanEventType">type of bean to copy</param>
	    /// <param name="eventAdapterService">for creating events</param>
	    /// <param name="copyMethod">method to copy the event</param>
	    public BeanEventBeanConfiguredCopyMethod(BeanEventType beanEventType, EventBeanTypedEventFactory eventAdapterService, MethodInfo copyMethod) {
	        this.beanEventType = beanEventType;
	        this.eventAdapterService = eventAdapterService;
	        this.copyMethod = copyMethod;
	    }

	    public EventBean Copy(EventBean theEvent) {
	        object underlying = theEvent.Underlying;
	        object copied;
	        try {
	            copied = copyMethod.Invoke(underlying, null);
	        }
	        catch (EPException) {
	            throw;
    	    } catch (MemberAccessException e) {
	            Log.Error("MemberAccessException copying event object for update: " + e.Message, e);
	            return null;
	        } catch (TargetException e) {
	            Log.Error("TargetException copying event object for update: " + e.Message, e);
	            return null;
	        } catch (Exception e) {
	            Log.Error("RuntimeException copying event object for update: " + e.Message, e);
	            return null;
	        }

	        return eventAdapterService.AdapterForTypedBean(copied, beanEventType);
	    }
	}
} // end of namespace