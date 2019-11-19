///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
using com.espertech.esper.common.@internal.collection;
using com.espertech.esper.common.@internal.epl.expression.core;
using com.espertech.esper.common.@internal.@event.core;
using com.espertech.esper.compat.collections;

namespace com.espertech.esper.common.@internal.filterspec
{
    public abstract class ExprNodeAdapterMSBase : ExprNodeAdapterBase
    {
        internal readonly EventBean[] prototypeArray;

        public ExprNodeAdapterMSBase(
            FilterSpecParamExprNode factory,
            ExprEvaluatorContext evaluatorContext,
            EventBean[] prototypeArray)
            : base(factory, evaluatorContext)

        {
            this.prototypeArray = prototypeArray;
        }

        public EventBean[] PrototypeArray => prototypeArray;

        public override bool Equals(object o)
        {
            if (this == o) {
                return true;
            }

            if (o == null || GetType() != o.GetType()) {
                return false;
            }

            if (!base.Equals(o)) {
                return false;
            }

            var that = (ExprNodeAdapterMSBase) o;

            // Array-of-events comparison must consider array-tag holders
            for (var i = 0; i < prototypeArray.Length; i++) {
                var mine = prototypeArray[i];
                var other = that.prototypeArray[i];
                if (mine == null) {
                    if (other != null) {
                        return false;
                    }

                    continue;
                }

                if (mine.Equals(other)) {
                    continue;
                }

                if (mine.EventType.Metadata.TypeClass != EventTypeTypeClass.PATTERNDERIVED) {
                    return false;
                }

                // these events holds array-matches
                var mineMapped = (MappedEventBean) mine;
                var otherMapped = (MappedEventBean) other;
                var propName = mineMapped.EventType.PropertyNames[0];
                var mineEvents = (EventBean[]) mineMapped.Properties.Get(propName);
                var otherEvents = (EventBean[]) otherMapped.Properties.Get(propName);
                if (!mineEvents.AreEqual(otherEvents)) {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
} // end of namespace