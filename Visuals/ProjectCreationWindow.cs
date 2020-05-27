using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dotnetpaint
{
	public partial class ProjectCreationWindow : Form
	{
		private Size newSize;
		public  Size NewProjectSize { get => newSize; }
		public ProjectCreationWindow()
		{
			InitializeComponent();
		}

		private void OkButton_Click(object sender, EventArgs e)
		{
			try
			{
				newSize.Width = Convert.ToInt32(WidthBox.Text);
				newSize.Height = Convert.ToInt32(HeightBox.Text);
			}
			catch (FormatException)
			{
				MessageBox.Show("Wrong number format.", "Input rejected");
			}
		}
	}
}
