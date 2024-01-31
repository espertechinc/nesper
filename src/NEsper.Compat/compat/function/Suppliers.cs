///////////////////////////////////////////////////////////////////////////////////////
// Copyright (C) 2006-2024 Esper Team. All rights reserved.                           /
// http://esper.codehaus.org                                                          /
// ---------------------------------------------------------------------------------- /
// The software in this package is published under the terms of the GPL license       /
// a copy of which has been included with this distribution in the license.txt file.  /
///////////////////////////////////////////////////////////////////////////////////////

namespace com.espertech.esper.compat.function
{
    public class Suppliers
    {
        public static Supplier<T> Memoize<T>(Supplier<T> supplier) where T : class
        {
            return new MemoizedSupplier<T>(supplier).GetValue;
        }

        public class MemoizedSupplier<T>
        {
            private Supplier<T> supplier;
            private T value;
            private bool valueIsSet;

            public MemoizedSupplier(Supplier<T> supplier)
            {
                this.supplier = supplier;
                this.value = default(T);
                this.valueIsSet = false;
            }

            public T GetValue()
            {
                if (!valueIsSet)
                {
                    lock (this)
                    {
                        if (!valueIsSet)
                        {
                            value = supplier.Invoke();
                            valueIsSet = true;
                        }
                    }
                }

                return value;
            }
        }
    }
}