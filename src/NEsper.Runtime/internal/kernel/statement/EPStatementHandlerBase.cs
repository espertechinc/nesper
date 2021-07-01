namespace com.espertech.esper.runtime.@internal.kernel.statement
{
    public class EPStatementHandlerBase
    {
        /// <summary>
        ///     Sets a subscriber instance.
        /// </summary>
        /// <param name="subscriber">is the subscriber to set</param>
        /// <param name="methodName">method name</param>
        public void SetSubscriber(
            object subscriber,
            string methodName)
        {
            Subscriber = subscriber;
            SubscriberMethodName = methodName;
        }


        /// <summary>
        ///     Returns the subscriber instance.
        /// </summary>
        /// <returns>subscriber</returns>
        public object Subscriber { get; private set; }

        public string SubscriberMethodName { get; private set; }
    }
}
