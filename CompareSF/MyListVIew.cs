using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CompareSFns
{
	/// <summary>
	/// Solution for ListView unnecessary flickering from
	///		https://stackoverflow.com/questions/21867801/cannot-get-listview-to-stop-flickering
	/// </summary>
	public partial class MyListView : ListView
	{
		public MyListView()
		{
			SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
		}
	}
}
