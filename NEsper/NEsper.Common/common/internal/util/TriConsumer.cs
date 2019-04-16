///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2019 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

using System;

namespace com.espertech.esper.common.@internal.util
{
    public interface TriConsumer<T, U, V>
    {
        void Accept(
            T t,
            U u,
            V v);
    }

    public class ProxyTriConsumer<T, U, V> : TriConsumer<T, U, V>
    {
        public Action<T, U, V> ProcAccept;

        public ProxyTriConsumer()
        {
        }

        public ProxyTriConsumer(Action<T, U, V> procAccept)
        {
            ProcAccept = procAccept;
        }

        public void Accept(
            T t,
            U u,
            V v)
        {
            ProcAccept(t, u, v);
        }
    }

#if MIXING_DEFAULT
    public default TriConsumer<T, U, V> andThen(TriConsumer<? super T, ? super U, ? super V> after) {
            Objects.requireNonNull(after);
            return (a, b, c)-> {
                accept(a, b, c);
                after.accept(a, b, c);
            }
            ;
        }
#endif
}