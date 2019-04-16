///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using com.espertech.esper.common.client;
using com.espertech.esper.common.client.meta;
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

        public EventBean[] GetPrototypeArray()
        {
            return prototypeArray;
        }

        public override bool Equals(object o)
        {
            if (this == o) return true;
            if (o == null || GetType() != o.GetType()) return false;
            if (!base.Equals(o)) return false;

            ExprNodeAdapterMSBase that = (ExprNodeAdapterMSBase) o;

            // Array-of-events comparison must consider array-tag holders
            for (int i = 0; i < prototypeArray.Length; i++) {
                EventBean mine = prototypeArray[i];
                EventBean other = that.prototypeArray[i];
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
                MappedEventBean mineMapped = (MappedEventBean) mine;
                MappedEventBean otherMapped = (MappedEventBean) other;
                string propName = mineMapped.EventType.PropertyNames[0];
                EventBean[] mineEvents = (EventBean[]) mineMapped.Properties.Get(propName);
                EventBean[] otherEvents = (EventBean[]) otherMapped.Properties.Get(propName);
                if (!CompatExtensions.AreEqual(mineEvents, otherEvents)) {
                    return false;
                }
            }

            return true;
        }
    }
} // end of namespace