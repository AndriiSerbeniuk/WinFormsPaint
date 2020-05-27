using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dotnetpaint
{
	[Serializable]
	class TShape : ISerializable
	{
		private Point point1, point2;
		private ToolBase tool;
		private int width, height;

		public Point Point1 
		{ 
			get => point1;
			set
			{
				point1 = value;
				Width = Math.Abs(point1.X - Point2.X);
				Height = Math.Abs(point1.Y - point2.Y);
			}
		}
		public Point Point2
		{
			get => point2;
			set
			{
				point2 = value;
				Width = Math.Abs(point1.X - point2.X);
				Height = Math.Abs(point1.Y - point2.Y);
			}
		}
		public ToolBase Tool 
		{ 
			get => tool; 
			set => tool = value.Clone(); 
		}

		public int ToolWidth
		{
			get => tool.ToolWidth;
			set => tool.ToolWidth = value;
		}

		public int Width
		{
			get => width;
			set
			{
				if (value != width)
				{
					int oldWidth = width;
					width = value;

					if (width != Math.Abs(point1.X - point2.X))
					{
						if (point2.X > point1.X)
							point2.X += value - oldWidth;
						else
							point1.X += value - oldWidth;
					}
					
					//if (point2.X > point1.X)
					//	point2.X += value - width;
					//else
					//	point1.X += value - width;

					//width = Math.Abs(point1.X - point2.X);
					//if (width == 0)
					//	width++;
				}
			}
		}

		public int Height
		{
			get => height;
			set
			{
				if (value != height)
				{
					int oldHeight = height;
					height = value;

					if (height != Math.Abs(point1.Y - point2.Y))
					{
						if (point2.Y > point1.Y)
							point2.Y += value - oldHeight;
						else
							point1.Y += value - oldHeight;
					}

					//if (point2.Y > point1.Y)
					//	point2.Y += value - height;
					//else
					//	point1.Y += value - height;

					//height = Math.Abs(point1.Y - point2.Y);
					//if (height == 0)
					//	height++;
				}
			}
		}

		public Color Color 
		{
			get => tool.ToolColor;
			set => tool.ToolColor = value;
		}

		public Color FillColor
		{
			get => tool.FillColor;
			set => tool.FillColor = value;
		}

		public TShape()
		{
			point1 = point2 = Point.Empty;
			tool = null;
		}

		public TShape(Point p1, Point p2, ToolBase _tool)
		{
			point1 = p1;
			point2 = p2;
			Width = Math.Abs(point1.X - Point2.X);
			Height = Math.Abs(point1.Y - point2.Y);
			tool = _tool.Clone();

		}

		public TShape(SerializationInfo info, StreamingContext context)
		{
			point1 = (Point)info.GetValue("p1", typeof(Point));
			Point2 = (Point)info.GetValue("p2", typeof(Point));
			tool = (ToolBase)info.GetValue("tool", typeof(ToolBase));
			//tool.ToolLayer 
		}

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("p1", point1, typeof(Point));
			info.AddValue("p2", point2, typeof(Point));
			info.AddValue("tool", tool, typeof(ToolBase));
		}
	}
}
