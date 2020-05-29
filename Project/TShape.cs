using System;
using System.Drawing;
using System.Runtime.Serialization;

namespace dotnetpaint
{
	/// <summary>
	/// Зберігає дані про фігуру, яка міститься в шарі.
	/// </summary>
	[Serializable]
	class TShape : ISerializable
	{
		/// <summary>
		/// Крайні точки фігури.
		/// </summary>
		private Point point1, point2;
		/// <summary>
		/// Інструмент малювання фігури.
		/// </summary>
		private ToolBase tool;
		/// <summary>
		/// Висота і шиотна фігури.
		/// </summary>
		private int width, height;

		/// <summary>
		/// Властивість для доступу до першої точки фігури.
		/// </summary>
		public Point Point1 
		{ 
			/// Повертає знначення точки.
			get => point1;
			/// Встановлює значення точки, змінює ширину та висоту фігури.
			set
			{
				point1 = value;
				Width = Math.Abs(point1.X - Point2.X);
				Height = Math.Abs(point1.Y - point2.Y);
			}
		}
		/// <summary>
		/// Властивість для доступу до другої точки фігури.
		/// </summary>
		public Point Point2
		{
			/// Повертає знначення точки.
			get => point2;
			/// Встановлює значення точки, змінює ширину та висоту фігури.
			set
			{
				point2 = value;
				Width = Math.Abs(point1.X - point2.X);
				Height = Math.Abs(point1.Y - point2.Y);
			}
		}
		/// <summary>
		/// Властивість для доступу до інструменту фігури.
		/// </summary>
		public ToolBase Tool 
		{ 
			get => tool; 
			set => tool = value.Clone(); 
		}
		/// <summary>
		/// Властивість для доступу до ширини інструменту фігури.
		/// </summary>
		public int ToolWidth
		{
			get => tool.ToolWidth;
			set => tool.ToolWidth = value;
		}
		/// <summary>
		/// Властивість для доступу до ширини фігури.
		/// </summary>
		public int Width
		{
			/// Повертає ширину фігури.
			get => width;
			/// Змінює ширину фігури, змінює відстань між точками, щоб вона відповідала ширині.
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
				}
			}
		}
		/// <summary>
		/// Властивість для доступу до висоти фігури.
		/// </summary>
		public int Height
		{
			/// Повертає висоту фігури.
			get => height;
			/// Змінює вистоу фігури, змінює відстань між точками, щоб вона відповідала висоті.
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
				}
			}
		}
		/// <summary>
		/// Властивість для доступу до основного кольору фігури.
		/// </summary>
		public Color Color 
		{
			get => tool.ToolColor;
			set => tool.ToolColor = value;
		}
		/// <summary>
		/// Властивість для доступу до кольору заливки фігури.
		/// </summary>
		public Color FillColor
		{
			get => tool.FillColor;
			set => tool.FillColor = value;
		}
		/// <summary>
		/// Кнструктор, який ініціалізує об'єкт значеннями за замовчуванням.
		/// </summary>
		public TShape()
		{
			point1 = point2 = Point.Empty;
			tool = null;
		}
		/// <summary>
		/// Кнструктор, який ініціалізує об'єкт переданими значеннями.
		/// </summary>
		/// <param name="p1">Перша точка фігури.</param>
		/// <param name="p2">Друга точка фігури.</param>
		/// <param name="_tool">Інструмент фігури.</param>
		public TShape(Point p1, Point p2, ToolBase _tool)
		{
			point1 = p1;
			point2 = p2;
			Width = Math.Abs(point1.X - Point2.X);
			Height = Math.Abs(point1.Y - point2.Y);
			tool = _tool.Clone();
		}
		/// <summary>
		/// Конструктор десеріалізації.
		/// </summary>
		public TShape(SerializationInfo info, StreamingContext context)
		{
			point1 = (Point)info.GetValue("p1", typeof(Point));
			Point2 = (Point)info.GetValue("p2", typeof(Point));
			tool = (ToolBase)info.GetValue("tool", typeof(ToolBase));
		}
		/// <summary>
		/// Метод серіалізації.
		/// </summary>
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("p1", point1, typeof(Point));
			info.AddValue("p2", point2, typeof(Point));
			info.AddValue("tool", tool, typeof(ToolBase));
		}
	}
}
