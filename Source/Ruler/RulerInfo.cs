namespace Ruler
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
			return string.Format("{0} {1} {2} {3} {5} {6}", Width, Height, IsVertical, Opacity, IsLocked, TopMost);
		}

		public static RulerInfo CovertToRulerInfo(string[] args)
		{
			string width = args[0];
			string height = args[1];
			string isVertical = args[2];
			string opacity = args[3];
			string isLocked = args[4];
			string topMost = args[5];


		    return new RulerInfo
		    {
		        Width = int.Parse(width),
		        Height = int.Parse(height),
		        IsVertical = bool.Parse(isVertical),
		        Opacity = double.Parse(opacity),
		        IsLocked = bool.Parse(isLocked),
		        TopMost = bool.Parse(topMost)
		    };
		}

		public static RulerInfo GetDefaultRulerInfo()
		{
		    return new RulerInfo
		    {
		        Width = 400,
		        Height = 75,
		        Opacity = 0.70,
		        IsLocked = false,
		        IsVertical = false,
		        TopMost = false
		    };
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