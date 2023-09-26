using com.espertech.esper.common.client;
using com.espertech.esper.common.@internal.rettype;

namespace com.espertech.esper.common.@internal.epl.resultset.@select.core
{
    public partial class SelectExprProcessorHelper
    {
        internal class TypesAndPropertyDescPair
        {
            private readonly EPChainableType[] insertIntoTargetsPerCol;
            private readonly EventPropertyDescriptor[] propertyDescriptors;
            private readonly bool[] canInsertEventBean;

            public TypesAndPropertyDescPair(
                EPChainableType[] insertIntoTargetsPerCol,
                EventPropertyDescriptor[] propertyDescriptors,
                bool[] canInsertEventBean)
            {
                this.insertIntoTargetsPerCol = insertIntoTargetsPerCol;
                this.propertyDescriptors = propertyDescriptors;
                this.canInsertEventBean = canInsertEventBean;
            }

            public EPChainableType[] InsertIntoTargetsPerCol => insertIntoTargetsPerCol;

            public EventPropertyDescriptor[] PropertyDescriptors => propertyDescriptors;

            public bool[] CanInsertEventBean => canInsertEventBean;
        }
    }
}