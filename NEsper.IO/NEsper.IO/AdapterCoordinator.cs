namespace com.espertech.esperio
{
	/// <summary>
	/// A AdapterCoordinator coordinates several Adapters so that the events they 
	/// send into the runtime engine arrive in some well-defined order, in
	/// effect making the several Adapters into one large sending Adapter.
	/// </summary>
	public interface AdapterCoordinator : InputAdapter
	{
		/// <summary>
		/// Coordinate an InputAdapter.
		/// <param name="adapter">the InputAdapter to coordinate</param>
		/// </summary>
		void Coordinate(InputAdapter adapter);
	}
}
