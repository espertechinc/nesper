using System;

using com.espertech.esper.common.@internal.util;
using com.espertech.esper.compat;
using com.espertech.esper.compat.collections;

using DataMap = System.Collections.Generic.IDictionary<string, object>;

namespace com.espertech.esperio.csv
{
	/// <summary>A spec for CSVAdapters.</summary>

	public class CSVInputAdapterSpec
	{
	    private DataMap _propertyTypes;
		
		/// <summary>Ctor.</summary>
		/// <param name="adapterInputSource">the source for the CSV data</param>
		/// <param name="eventTypeName">the name of the event type created from the CSV data</param>

		public CSVInputAdapterSpec(AdapterInputSource adapterInputSource, string eventTypeName)
		{
			AdapterInputSource = adapterInputSource;
			EventTypeName = eventTypeName;
		}

	    /// <summary>
	    /// Gets or sets the number of events per seconds.
	    /// </summary>
	    public int? EventsPerSec { get; set; }

	    /// <summary>
	    /// Gets or sets the property order of the properties in the CSV file
	    /// </summary>
	    public string[] PropertyOrder { get; set; }

	    /// <summary>
	    /// Gets or sets the flag that indicates if the adapter is looping
	    /// </summary>
	    public bool IsLooping { get; set; }

	    /// <summary>
		/// Gets or sets the propertyTypes value
		/// </summary>
		/// <returns>
		/// a mapping between the names and types of the properties in the
		/// CSV file; this will also be the form of the Map event created
		/// from the data
		/// </returns>

		public DataMap PropertyTypes
		{
			get { return _propertyTypes; }
            set
            {
                var tempDict = new NullableDictionary<string, object>();
                foreach( var entry in value ) {
	                tempDict[entry.Key] = ((Type) entry.Value).GetBoxedType();
                }

                _propertyTypes = tempDict;
            }
		}

	    /// <summary>
	    /// Gets or sets a flag indicating whether to use the engine timer thread for work or not.
	    /// Setting the value to true uses the engine timer thread for work.  Setting the value
	    /// to false, uses the current thread.
	    /// </summary>
	    public bool IsUsingEngineThread { get; set; }

	    /// <summary>
	    /// Gets or sets a value indicating whether this instance is using external timer.
	    /// </summary>
	    /// <value>
	    /// 	<c>true</c> if this instance is using external timer; otherwise, <c>false</c>.
	    /// </value>
	    public bool IsUsingExternalTimer { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is using time span events.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is using time span events; otherwise, <c>false</c>.
        /// </value>
        public bool IsUsingTimeSpanEvents { get; set; }

	    /// <summary>
	    /// Gets or sets the timestamp column name.
	    /// </summary>
	    /// <returns>the name of the column to use for timestamps</returns>
	    public string TimestampColumn { get; set; }

	    /// <summary>
	    /// Gets or sets the adapter input source.
	    /// </summary>
	    public AdapterInputSource AdapterInputSource { get; set; }

	    /// <summary>
	    /// Gets or sets the event type name.
	    /// </summary>
	    public string EventTypeName { get; set; }
	}
}
