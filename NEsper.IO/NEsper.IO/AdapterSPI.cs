using com.espertech.esper.runtime.client;

namespace com.espertech.esperio
{
	/// <summary>
	/// An Adapter takes some external data, converts it into events, and sends it
	/// into the runtime engine.
	/// </summary>
	public interface AdapterSPI
	{
	    /// <summary>
	    /// Gets or sets the engine instance.
		/// </summary>

	    EPRuntime Runtime
		{
			get ;
			set ;
		}
	}
}
