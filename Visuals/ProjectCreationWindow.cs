using System;
using System.Drawing;
using System.Windows.Forms;

namespace dotnetpaint
{
	/// <summary>
	/// Діалог вибору розміру.
	/// </summary>
	public partial class ProjectCreationWindow : Form
	{
		/// <summary>
		/// Розмір, отриманий із діалогу.
		/// </summary>
		private Size newSize;
		/// <summary>
		/// ВластивістьЮ для роботи із newSize. Доступне тільки отримання значення.
		/// </summary>
		public Size NewProjectSize { get => newSize; }
		public ProjectCreationWindow()
		{
			InitializeComponent();
		}
		/// <summary>
		/// Подія кнопки, що відповідає за підтвердження вибору.
		/// </summary>
		private void OkButton_Click(object sender, EventArgs e)
		{
			try
			{
				newSize.Width = Convert.ToInt32(WidthBox.Text);
				newSize.Height = Convert.ToInt32(HeightBox.Text);
			}
			// Якщо введені значення не являють собою число.
			catch (FormatException)
			{
				MessageBox.Show("Wrong number format.", "Input rejected");
			}
		}
	}
}
