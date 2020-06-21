///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2015 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;

using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.client.util;
using com.espertech.esper.common.@internal.@event.bean.core;
using com.espertech.esper.common.@internal.@event.bean.service;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.common.@internal.@event.eventtypefactory;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;
using com.espertech.esper.container;

namespace com.espertech.esper.runtime.@internal.kernel.service
{
	public class EPRuntimeBeanAnonymousTypeService
	{
		private IContainer _container;
		private readonly BeanEventTypeStemService _stemSvc;
		private readonly BeanEventTypeFactoryPrivate _factoryPrivate;

		public EPRuntimeBeanAnonymousTypeService(IContainer container)
		{
			_container = container;
			_stemSvc = new BeanEventTypeStemService(
				EmptyDictionary<Type, IList<string>>.Instance,
				null,
				PropertyResolutionStyle.CASE_SENSITIVE,
				AccessorStyle.NATIVE);

			_factoryPrivate = new BeanEventTypeFactoryPrivate(
				new EventBeanTypedEventFactoryRuntime(null),
				EventTypeFactoryImpl.GetInstance(container),
				_stemSvc);
		}

		public BeanEventType MakeBeanEventTypeAnonymous(Type beanType)
		{
			var metadata = new EventTypeMetadata(
				beanType.Name,
				null,
				EventTypeTypeClass.STREAM,
				EventTypeApplicationType.CLASS,
				NameAccessModifier.TRANSIENT,
				EventTypeBusModifier.NONBUS,
				false,
				EventTypeIdPair.Unassigned());
			var stem = _stemSvc.GetCreateStem(beanType, null);
			return new BeanEventType(_container, stem, metadata, _factoryPrivate, null, null, null, null);
		}
	}
} // end of namespace
