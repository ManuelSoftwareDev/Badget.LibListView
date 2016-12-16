namespace Badget.LibListview.Controls
{

    /// <summary>
    /// Interface you must include for a control to be activated embedded useable
    /// </summary>
    public interface GLEmbeddedControl
	{

		/// <summary>
		/// item this control is embedded in
		/// </summary>
		GLItem				Item { get; set; }

		/// <summary>
		/// Sub item this control is embedded in
		/// </summary>
		GLSubItem		SubItem { get; set; }

		/// <summary>
		/// Parent control
		/// </summary>
		BadgetListView			ListControl { get; set; }

		/// <summary>
		/// This returns the current text output as entered into the control right now
		/// </summary>
		/// <returns></returns>
		string GLReturnText();


		/// <summary>
		/// Called when the control is loaded
		/// </summary>
		/// <param name="item"></param>
		/// <param name="subItem"></param>
		/// <param name="listctrl"></param>
		/// <returns></returns>
		bool GLLoad( GLItem item, GLSubItem subItem, BadgetListView listctrl );		// populate this control however you wish with item


		/// <summary>
		/// Called when control is being destructed
		/// </summary>
		void GLUnload();																			// take information from control and return it to the item
	}


}
