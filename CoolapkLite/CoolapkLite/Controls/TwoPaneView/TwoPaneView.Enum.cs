using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolapkLite.Controls
{
	public enum ViewMode
	{
		Pane1Only,
		Pane2Only,
		LeftRight,
		RightLeft,
		TopBottom,
		BottomTop,
		None
	}

	public enum TwoPaneViewPriority
	{
		Pane1 = 0,
		Pane2 = 1
	}

	public enum TwoPaneViewMode
	{
		SinglePane = 0,
		Wide = 1,
		Tall = 2
	}

	public enum TwoPaneViewWideModeConfiguration
	{
		SinglePane = 0,
		LeftRight = 1,
		RightLeft = 2
	}

	public enum TwoPaneViewTallModeConfiguration
	{
		SinglePane = 0,
		TopBottom = 1,
		BottomTop = 2
	}
}
