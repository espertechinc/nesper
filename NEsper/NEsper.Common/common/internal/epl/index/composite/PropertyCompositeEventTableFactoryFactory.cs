///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;
using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.context.util;
using com.espertech.esper.common.@internal.epl.index.@base;

namespace com.espertech.esper.common.@internal.epl.index.composite
{
    public class PropertyCompositeEventTableFactoryFactory : EventTableFactoryFactory
    {
        private readonly int _indexedStreamNum;
        private readonly bool _isFireAndForget;
        private readonly EventPropertyValueGetter _keyGetter;
        private readonly string[] _keyProps;
        private readonly Type[] _keyTypes;
        private readonly EventPropertyValueGetter[] _rangeGetters;
        private readonly string[] _rangeProps;
        private readonly Type[] _rangeTypes;
        private readonly int? _subqueryNum;

        public PropertyCompositeEventTableFactoryFactory(
            int indexedStreamNum, int? subqueryNum, bool isFireAndForget, string[] keyProps, Type[] keyTypes,
            EventPropertyValueGetter keyGetter, string[] rangeProps, Type[] rangeTypes,
            EventPropertyValueGetter[] rangeGetters)
        {
            this._indexedStreamNum = indexedStreamNum;
            this._subqueryNum = subqueryNum;
            this._isFireAndForget = isFireAndForget;
            this._keyProps = keyProps;
            this._keyTypes = keyTypes;
            this._keyGetter = keyGetter;
            this._rangeProps = rangeProps;
            this._rangeTypes = rangeTypes;
            this._rangeGetters = rangeGetters;
        }

        public EventTableFactory Create(EventType eventType, StatementContext statementContext)
        {
            return statementContext.EventTableIndexService.CreateComposite(
                _indexedStreamNum, eventType,
                _keyProps, _keyTypes, _keyGetter,
                _rangeProps, _rangeTypes, _rangeGetters,
                null, _isFireAndForget);
        }
    }
} // end of namespace