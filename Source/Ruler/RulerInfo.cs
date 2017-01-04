﻿namespace Ruler
{
	public class RulerInfo : IRulerInfo
	{
		public int Width
		{
			get;
			set;
		}

		public int Height
		{
			get;
			set;
		}

		/// <summary>
		/// TODO
		/// </summary>
		public bool IsVertical
		{
			get;
			set;
		}

		public double Opacity
		{
			get;
			set;
		}


		/// <summary>
		/// TODO
		/// </summary>
		public bool IsLocked
		{
			get;
			set;
		}

		public bool TopMost
		{
			get;
			set;
		}

		public string ConvertToParameters()
		{
			return string.Format("{0} {1} {2} {3} {5} {6}", this.Width, this.Height, this.IsVertical, this.Opacity, this.IsLocked, this.TopMost);
		}

		public static RulerInfo CovertToRulerInfo(string[] args)
		{
			string width = args[0];
			string height = args[1];
			string isVertical = args[2];
			string opacity = args[3];
			string isLocked = args[4];
			string topMost = args[5];

			RulerInfo rulerInfo = new RulerInfo();

			rulerInfo.Width = int.Parse(width);
			rulerInfo.Height = int.Parse(height);
			rulerInfo.IsVertical = bool.Parse(isVertical);
			rulerInfo.Opacity = double.Parse(opacity);
			rulerInfo.IsLocked = bool.Parse(isLocked);
			rulerInfo.TopMost = bool.Parse(topMost);

			return rulerInfo;
		}

		public static RulerInfo GetDefaultRulerInfo()
		{
			RulerInfo rulerInfo = new RulerInfo();

			rulerInfo.Width = 400;
			rulerInfo.Height = 75;
			rulerInfo.Opacity = 0.65;
			rulerInfo.IsLocked = false;
			rulerInfo.IsVertical = false;
			rulerInfo.TopMost = false;

			return rulerInfo;
		}
	}

	public static class IRulerInfoExtentension
	{
		public static void CopyInto<T>(this IRulerInfo ruler, T targetInstance)
			where T : IRulerInfo
		{
			targetInstance.Width = ruler.Width;
			targetInstance.Height = ruler.Height;
			targetInstance.IsVertical = ruler.IsVertical;
			targetInstance.Opacity = ruler.Opacity;
			targetInstance.IsLocked = ruler.IsLocked;
			targetInstance.TopMost = ruler.TopMost;
		}
	}
}