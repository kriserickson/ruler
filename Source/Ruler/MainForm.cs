using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;

namespace Ruler
{
	sealed public class MainForm : Form, IRulerInfo
	{
		#region ResizeRegion enum

		private enum ResizeRegion
		{
			None, N, NE, E, SE, S, SW, W, NW
		}

		#endregion ResizeRegion enum

	    private const double TOLERANCE = 0.001;

	    private readonly ToolTip _toolTip = new ToolTip();
		private Point _offset;
		private Rectangle _mouseDownRect;
		private int _resizeBorderWidth = 5;
		private Point _mouseDownPoint;
		private ResizeRegion _resizeRegion = ResizeRegion.None;
		private readonly ContextMenu _menu = new ContextMenu();
		private MenuItem _verticalMenuItem;
		private MenuItem _lockedMenuItem;
	    private int _position;

	    private static void Main(params string[] args)
	    {
	        Application.EnableVisualStyles();
	        Application.SetCompatibleTextRenderingDefault(false);

	        // ToDo save rulerInfo to a config and restore it...
	        var mainForm = new MainForm(args.Length == 0  ? RulerInfo.GetDefaultRulerInfo() :  RulerInfo.CovertToRulerInfo(args));
	        Application.Run(mainForm);
	    }


	    public MainForm(RulerInfo rulerInfo)
		{
			SetStyle(ControlStyles.ResizeRedraw, true);
			UpdateStyles();

			ResourceManager resources = new ResourceManager(typeof(MainForm));
			Icon = (Icon)(resources.GetObject("$this.Icon"));

		    // We have to setup the menu before copying rulerInfo so we pass opacity in otherwise it starts off as 1...
			SetUpMenu(rulerInfo.Opacity);

			Text = "Ruler";

		    // TODO: Make color default...
			BackColor = Color.White;

			rulerInfo.CopyInto(this);

			FormBorderStyle = FormBorderStyle.None;

			ContextMenu = _menu;
			Font = new Font("Tahoma", 10);

		    SetToolTip();




			SetStyle(ControlStyles.DoubleBuffer | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
		}

	    public bool IsVertical
	    {
	        get { return _verticalMenuItem.Checked; }
	        set { _verticalMenuItem.Checked = value; }
	    }

	    public bool IsLocked
	    {
	        get;
	        set;
	    }

	    private RulerInfo GetRulerInfo()
		{
			RulerInfo rulerInfo = new RulerInfo();

			this.CopyInto(rulerInfo);

			return rulerInfo;
		}

		private void SetUpMenu(double opacity)
		{
			AddMenuItem("Stay On Top");
			_verticalMenuItem = AddMenuItem("Vertical");
			var opacityMenuItem = AddMenuItem("Opacity");
			_lockedMenuItem = AddMenuItem("Lock resizing", Shortcut.None, LockHandler);
			AddMenuItem("Set size...", Shortcut.None, SetWidthHeightHandler);
			AddMenuItem("Duplicate", Shortcut.None, DuplicateHandler);
			AddMenuItem("-");
			AddMenuItem("About...");
			AddMenuItem("-");
			AddMenuItem("Exit");

			for (int i = 10; i <= 100; i += 10)
			{
				var subMenu = new MenuItem(i + "%");
				subMenu.Checked = Math.Abs(i - (opacity * 100)) < TOLERANCE;
				subMenu.Click += OpacityMenuHandler;
				opacityMenuItem.MenuItems.Add(subMenu);
			}
		}

		private void SetWidthHeightHandler(object sender, EventArgs e)
		{
			SetSizeForm form = new SetSizeForm(Width, Height);

			if (TopMost)
			{
				form.TopMost = true;
			}

			if (form.ShowDialog() == DialogResult.OK)
			{
				Size size = form.GetNewSize();

				Width = size.Width;
				Height = size.Height;
			}
		}

		private void LockHandler(object sender, EventArgs e)
		{
			IsLocked = !IsLocked;
			_lockedMenuItem.Checked = IsLocked;
		}

		private void DuplicateHandler(object sender, EventArgs e)
		{
			string exe = Assembly.GetExecutingAssembly().Location;

			RulerInfo rulerInfo = GetRulerInfo();

			ProcessStartInfo startInfo = new ProcessStartInfo(exe, rulerInfo.ConvertToParameters());

			Process process = new Process();
			process.StartInfo = startInfo;
			process.Start();
		}

		private MenuItem AddMenuItem(string text)
		{
			return AddMenuItem(text, Shortcut.None, MenuHandler);
		}

		private MenuItem AddMenuItem(string text, Shortcut shortcut, EventHandler handler)
		{
			var mi = new MenuItem(text);
			mi.Click += handler;
			mi.Shortcut = shortcut;
			_menu.MenuItems.Add(mi);

			return mi;
		}

	    protected override void OnDoubleClick(EventArgs e)
	    {
	        ChangeOrientation();
	        base.OnDoubleClick(e);
	    }

	    protected override void OnMouseDown(MouseEventArgs e)
		{
			_offset = new Point(MousePosition.X - Location.X, MousePosition.Y - Location.Y);
			_mouseDownPoint = MousePosition;
			_mouseDownRect = ClientRectangle;

			base.OnMouseDown(e);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			_resizeRegion = ResizeRegion.None;
			base.OnMouseUp(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
		    if (IsVertical)
		    {
		        if (_position != e.Y)
		        {
		            _position = e.Y;
		            SetToolTip();
		        }
		    }
		    else
		    {
		        if (_position != e.X)
		        {
		            _position = e.X;
		            SetToolTip();
		        }
		    }


			if (_resizeRegion != ResizeRegion.None)
			{
				HandleResize();
				return;
			}

			Point clientCursorPos = PointToClient(MousePosition);
			Rectangle resizeInnerRect = ClientRectangle;
			resizeInnerRect.Inflate(-_resizeBorderWidth, -_resizeBorderWidth);

			bool inResizableArea = ClientRectangle.Contains(clientCursorPos) && !resizeInnerRect.Contains(clientCursorPos);



			if (inResizableArea)
			{
				ResizeRegion resizeRegion = GetResizeRegion(clientCursorPos);
				SetResizeCursor(resizeRegion);

				if (e.Button == MouseButtons.Left)
				{
					_resizeRegion = resizeRegion;
					HandleResize();
				}
			}
			else
			{
				Cursor = Cursors.Default;

				if (e.Button == MouseButtons.Left)
				{
					Location = new Point(MousePosition.X - _offset.X, MousePosition.Y - _offset.Y);
				}
			}

			base.OnMouseMove(e);
		}

		private void SetToolTip()
		{
			_toolTip.SetToolTip(this, _position.ToString());
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Right:
				case Keys.Left:
				case Keys.Up:
				case Keys.Down:
					HandleMoveResizeKeystroke(e);
					break;

				case Keys.Space:
					ChangeOrientation();
					break;
			}

			base.OnKeyDown(e);
		}

		private void HandleMoveResizeKeystroke(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Right)
			{
				if (e.Control)
				{
					if (e.Shift)
					{
						Width += 1;
					}
					else
					{
						Left += 1;
					}
				}
				else
				{
					Left += 5;
				}
			}
			else if (e.KeyCode == Keys.Left)
			{
				if (e.Control)
				{
					if (e.Shift)
					{
						Width -= 1;
					}
					else
					{
						Left -= 1;
					}
				}
				else
				{
					Left -= 5;
				}
			}
			else if (e.KeyCode == Keys.Up)
			{
				if (e.Control)
				{
					if (e.Shift)
					{
						Height -= 1;
					}
					else
					{
						Top -= 1;
					}
				}
				else
				{
					Top -= 5;
				}
			}
			else if (e.KeyCode == Keys.Down)
			{
				if (e.Control)
				{
					if (e.Shift)
					{
						Height += 1;
					}
					else
					{
						Top += 1;
					}
				}
				else
				{
					Top += 5;
				}
			}
		}

		private void HandleResize()
		{
			if (IsLocked)
			{
				return;
			}

		    int newWidth = Width;
		    int newHeight = Height;

		    switch (_resizeRegion)
		    {

		        case ResizeRegion.E:
		        {
		            int diff = MousePosition.X - _mouseDownPoint.X;
		            newWidth = _mouseDownRect.Width + diff;
		            break;
		        }
		        case ResizeRegion.S:
		        {
		            int diff = MousePosition.Y - _mouseDownPoint.Y;
		            newHeight = _mouseDownRect.Height + diff;
		            break;
		        }
		        case ResizeRegion.SE:
		        {
		            newWidth = _mouseDownRect.Width + MousePosition.X - _mouseDownPoint.X;
		            newHeight = _mouseDownRect.Height + MousePosition.Y - _mouseDownPoint.Y;
		            break;
		        }
		    }

		    if (newWidth > 50 && newWidth < 200)
		    {
		        Width = newWidth;
		    }
		    if (newHeight > 50)
		    {
		        Height = newHeight;
		    }
		}

		private void SetResizeCursor(ResizeRegion region)
		{
			switch (region)
			{
				case ResizeRegion.N:
				case ResizeRegion.S:
					Cursor = Cursors.SizeNS;
					break;

				case ResizeRegion.E:
				case ResizeRegion.W:
					Cursor = Cursors.SizeWE;
					break;

				case ResizeRegion.NW:
				case ResizeRegion.SE:
					Cursor = Cursors.SizeNWSE;
					break;

				default:
					Cursor = Cursors.SizeNESW;
					break;
			}
		}

		private ResizeRegion GetResizeRegion(Point clientCursorPos)
		{
		    if (clientCursorPos.Y <= _resizeBorderWidth)
		    {
		        if (clientCursorPos.X <= _resizeBorderWidth) return ResizeRegion.NW;
		        if (clientCursorPos.X >= Width - _resizeBorderWidth) return ResizeRegion.NE;
		        return ResizeRegion.N;
		    }
		    if (clientCursorPos.Y >= Height - _resizeBorderWidth)
		    {
		        if (clientCursorPos.X <= _resizeBorderWidth) return ResizeRegion.SW;
		        if (clientCursorPos.X >= Width - _resizeBorderWidth) return ResizeRegion.SE;
		        return ResizeRegion.S;
		    }
		    if (clientCursorPos.X <= _resizeBorderWidth) return ResizeRegion.W;
		    return ResizeRegion.E;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			Graphics graphics = e.Graphics;

			int height = Height;
			int width = Width;

			if (IsVertical)
			{
				graphics.RotateTransform(90);
				graphics.TranslateTransform(0, -Width + 1);
				height = Width;
				width = Height;
			}

			DrawRuler(graphics, width, height);

			base.OnPaint(e);
		}

		private void DrawRuler(Graphics g, int formWidth, int formHeight)
		{
			// Border
			g.DrawRectangle(Pens.Black, 0, 0, formWidth - 1, formHeight - 1);

			// Width
			g.DrawString(formWidth + " pixels", Font, Brushes.Black, 10, (formHeight / 2) - (Font.Height / 2));

			// Ticks
			for (int i = 0; i < formWidth; i++)
			{
				if (i % 2 == 0)
				{
					int tickHeight;
					if (i % 100 == 0)
					{
						tickHeight = 15;
						DrawTickLabel(g, i.ToString(), i, formHeight, tickHeight);
					}
					else if (i % 10 == 0)
					{
						tickHeight = 10;
					}
					else
					{
						tickHeight = 5;
					}

					DrawTick(g, i, formHeight, tickHeight);
				}
			}
		}

		private static void DrawTick(Graphics g, int xPos, int formHeight, int tickHeight)
		{
			// Top
			g.DrawLine(Pens.Black, xPos, 0, xPos, tickHeight);

			// Bottom
			g.DrawLine(Pens.Black, xPos, formHeight, xPos, formHeight - tickHeight);
		}

		private void DrawTickLabel(Graphics g, string text, int xPos, int formHeight, int height)
		{
			// Top
			g.DrawString(text, Font, Brushes.Black, xPos, height);

			// Bottom
			g.DrawString(text, Font, Brushes.Black, xPos, formHeight - height - Font.Height);
		}


		private void OpacityMenuHandler(object sender, EventArgs e)
		{
			MenuItem mi = (MenuItem)sender;
			UncheckMenuItem(mi.Parent);
			mi.Checked = true;
			Opacity = double.Parse(mi.Text.Replace("%", "")) / 100;
		}

		private void UncheckMenuItem(Menu parent)
		{
			if (parent == null)
			{
				return;
			}

			for (int i = 0; i < parent.MenuItems.Count; i++)
			{
				if (parent.MenuItems[i].Checked)
				{
					parent.MenuItems[i].Checked = false;
				}
			}
		}

		private void MenuHandler(object sender, EventArgs e)
		{
			MenuItem mi = (MenuItem)sender;

			switch (mi.Text)
			{
				case "Exit":
					Close();
					break;

				case "Vertical":
					ChangeOrientation();
					break;

				case "Stay On Top":
					mi.Checked = !mi.Checked;
					TopMost = mi.Checked;
					break;

				case "About...":
					string message =
					    $"Ruler v{Application.ProductVersion} by Jeff Key\nwww.sliver.com\nIcon by Kristen Magee @ www.kbecca.com";
					MessageBox.Show(message, "About Ruler", MessageBoxButtons.OK, MessageBoxIcon.Information);
					break;

				default:
					MessageBox.Show("Unknown menu item.");
					break;
			}
		}

		private void ChangeOrientation()
		{
			IsVertical = !IsVertical;
			int width = Width;
			Width = Height;
			Height = width;
		}

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.SuspendLayout();
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.ResumeLayout(false);

        }
    }
}