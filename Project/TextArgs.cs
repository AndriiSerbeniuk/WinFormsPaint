using System;
using System.Drawing;
using System.Runtime.Serialization;

namespace dotnetpaint
{
	/// <summary>
	/// Клас, яккий містить інформацію про текст, який міститься на шарі.
	/// </summary>
	[Serializable]
	class TextArgs : ISerializable
	{
		/// <summary>
		/// Текст шару.
		/// </summary>
		private string text;
		/// <summary>
		/// Шрифт цього тексту.
		/// </summary>
		private Font font;
		/// <summary>
		/// Штиль шрифту цього тексту
		/// </summary>
		private FontStyle style;
		/// <summary>
		/// Сімейство шрифтів цього шрифту.
		/// </summary>
		private FontFamily family;
		/// <summary>
		/// Розмір шрифту.
		/// </summary>
		private float size;
		/// <summary>
		/// Пензлик, який пишеться текст.
		/// </summary>
		private SolidBrush brush;
		/// <summary>
		/// Розташування тексту на шарі.
		/// </summary>
		private Point location;
		/// <summary>
		/// Властивість для доступу до сімейства шрифтів.
		/// </summary>
		public FontFamily FontFamily
		{
			get => font.FontFamily;
			set
			{
				family = value;
				font.Dispose();
				font = new Font(family, size, style);
			}
		}
		/// <summary>
		/// Властивість для доступу до тексту.
		/// </summary>
		public string Text { get => text; set => text = value; }
		/// <summary>
		/// Властивість для доступу до шрифту.
		/// </summary>
		public Font Font 
		{ 
			get => font; 
			set
			{
				font.Dispose();
				font = value;
				style = value.Style;
				family = value.FontFamily;
				size = value.Size;
			}
		}
		/// <summary>
		/// Властивість для доступу до розміру шрифту.
		/// </summary>
		public float Size 
		{ 
			get => size; 
			set
			{
				size = value;
				font.Dispose();
				font = new Font(family, size, style);
			}
		}
		/// <summary>
		/// Властивість для доступу до кольору тексту.
		/// </summary>
		public Color Color { get => brush.Color; set => brush.Color = value; }
		/// <summary>
		/// Властивість для доступу до пензлика, яким пишеться текст.
		/// </summary>
		public SolidBrush Brush { get => brush; set => brush = value; }
		/// <summary>
		/// Властивість для доступу до локації тексту.
		/// </summary>
		public Point Location { get => location; set => location = value; }
		/// <summary>
		/// Конструктор, який ініціалізує об'єкт значеннями за замовчуванням.
		/// </summary>
		public TextArgs()
		{
			text = "";
			size = 12;
			family = FontFamily.GenericSansSerif;
			style = FontStyle.Regular;
			font = new Font(family, size);
			brush = new SolidBrush(Color.Black);
			location = Point.Empty;
		}
		/// <summary>
		/// Конструктор, який ініціалізує об'єкт переданими значеннями.
		/// </summary>
		/// <param name="font">Шрифт тексту.</param>
		/// <param name="fontSize">Розмір шрифту.</param>
		/// <param name="color">Колір шрифту.</param>
		/// <param name="location">Розташування тексту.</param>
		public TextArgs(Font font, int fontSize, Color color, Point location)
		{
			text = "";
			size = fontSize;
			family = font.FontFamily;
			this.font = new Font(font.FontFamily, size);
			style = font.Style;
			brush = new SolidBrush(color);
			this.location = location;
		}
		/// <summary>
		/// Конструктор десеріалізації.
		/// </summary>
		public TextArgs(SerializationInfo info, StreamingContext context)
		{
			font = (Font)info.GetValue("font", typeof(Font));
			style = font.Style;
			family = font.FontFamily;
			size = font.Size;
			brush = new SolidBrush((Color)info.GetValue("brush", typeof(Color)));
			location = (Point)info.GetValue("location", typeof(Point));
			text = (string)info.GetValue("text", typeof(string));
		}
		/// <summary>
		/// Метод серіалізації.
		/// </summary>
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("text", text, typeof(string));
			info.AddValue("font", font, typeof(Font));
			info.AddValue("brush", brush.Color, typeof(Color));
			info.AddValue("location", location, typeof(Point));
		}

		~TextArgs()
		{
			font.Dispose();
			brush.Dispose();
		}
	}
}
