using System.Text;

using com.espertech.esper.common.client;
using com.espertech.esper.compat;
using com.espertech.esper.compat.logging;
using com.espertech.esper.runtime.client;

namespace NEsper.Examples.OHLC
{
    public class OHLCUpdateListener : UpdateListener
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(OHLCUpdateListener));

        public void Update(
            object sender,
            UpdateEventArgs eventArgs)
        {
            var newData = eventArgs.NewEvents;
            var statement = eventArgs.Statement;
            var isInfoEnabled = Log.IsInfoEnabled;

            for (var i = 0; i < newData.Length; i++) {
                if (isInfoEnabled) {
                    Log.Info("Statement " + statement.Name + " produced: " + GetProperties(newData[i]));
                }
            }
        }

        private string GetProperties(EventBean theEvent)
        {
            var buf = new StringBuilder();

            foreach (var name in theEvent.EventType.PropertyNames) {
                var value = theEvent.Get(name);
                buf.Append(name);
                buf.Append("=");

                if (name.Contains("timestamp")) {
                    var timestamp = value.AsInt64();
                    var dateTime = DateTimeEx.UtcInstance(timestamp);
                    buf.Append(dateTime);
                }
                else {
                    buf.Append(value);
                }

                buf.Append(" ");
            }

            return buf.ToString();
        }
    }
}