using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace dotnetpaint
{
	[Serializable]
	class TextArgs : ISerializable
	{
		private string text;
		private Font font;
		private FontStyle style;
		//[NonSerialized]
		private FontFamily family;
		private float size;
		private SolidBrush brush;
		private Point location;

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
		public string Text { get => text; set => text = value; }
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
		public Color Color { get => brush.Color; set => brush.Color = value; }
		public SolidBrush Brush { get => brush; set => brush = value; }
		public Point Location { get => location; set => location = value; }

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
