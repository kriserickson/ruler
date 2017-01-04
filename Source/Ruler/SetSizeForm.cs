using System;
using System.Drawing;
using System.Windows.Forms;

namespace Ruler
{
	public partial class SetSizeForm : Form
	{
		private readonly int _originalWidth;
		private readonly int _originalHeight;

		public SetSizeForm(int initWidth, int initHeight)
		{
			InitializeComponent();

			_originalWidth = initWidth;
			_originalHeight = initHeight;

			txtWidth.Text = initWidth.ToString();
			txtHeight.Text = initHeight.ToString();
		}

		private void BtnCancelClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		private void BtnOkClick(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
		}

		public Size GetNewSize()
		{
			int width;
			int height;

			Size size = new Size();

			size.Width = int.TryParse(txtWidth.Text, out width) ? width : _originalWidth;
			size.Height = int.TryParse(txtHeight.Text, out height) ? height : _originalHeight;

			return size;
		}
	}
}