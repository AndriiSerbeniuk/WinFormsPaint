using System;
using System.Drawing;
using System.Runtime.Serialization;

namespace dotnetpaint
{
	/// <summary>
	/// Клас, який представляє інструмент для створення прямокутників.
	/// </summary>
	[Serializable]
	class RectangleTool : ToolBase, ISerializable
	{
		/// <summary>
		/// Конструктор, який ініціалізує інструмент значеннями за замовчуванням.
		/// </summary>
		public RectangleTool() : base()
		{
			FillColor = Color.Transparent;
		}
		// <summary>
		/// Конструктор, який ініціалізує інструмент значеннями із параметрів.
		/// </summary>
		/// <param name="color">Основний колір інструменту.</param>
		/// <param name="width">Ширина інструменту.</param>
		public RectangleTool(Color color, int width) : base(color, width)
		{
			FillColor = Color.Transparent;
		}
		/// <summary>
		/// Конструктор десеріалізації.
		/// </summary>
		public RectangleTool(SerializationInfo info, StreamingContext context)
		{
			ToolWidth = (int)info.GetValue("width", typeof(int));
			ToolColor = (Color)info.GetValue("primaryColor", typeof(Color));
			FillColor = (Color)info.GetValue("fillColor", typeof(Color));
		}
		/// <summary>
		/// Метод серіалізації.
		/// </summary>
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("primaryColor", ToolColor);
			info.AddValue("fillColor", FillColor);
			info.AddValue("width", ToolWidth);
		}
		/// <summary>
		/// Готує інструмент до роботи: встановлює першу точку прямокутника.
		/// </summary>
		/// <param name="InitPoint">Початкова точка.</param>
		/// <param name="workLayer">Шар, з яким працює інструмент.</param>
		public override void InitTool(Point InitPoint, Layer workLayer)
		{
			InitialPoint = InitPoint;
			ToolLayer = workLayer;
			if (ToolLayer.Shape == null)
				ToolLayer.Shape = new TShape();
			ToolLayer.Shape.Tool = this;
		}
		/// <summary>
		/// Використовує інструмент: малює прямокутник, сформований початкою точкою та точкою використання.
		/// </summary>
		/// <param name="UsePoint">Точка використання.</param>
		public override void UseTool(Point UsePoint)
		{
			ToolLayer.Graphics.Clear(Color.Transparent);
			int p1x = Math.Min(UsePoint.X, InitialPoint.X),
				p1y = Math.Min(UsePoint.Y, InitialPoint.Y),
				p2x = Math.Max(UsePoint.X, InitialPoint.X),
				p2y = Math.Max(UsePoint.Y, InitialPoint.Y);

			ToolLayer.Shape.Point1 = new Point(p1x, p1y);
			ToolLayer.Shape.Point2 = new Point(p2x, p2y);
			if (DrawingBrush.Color.A != 0)
				ToolLayer.Graphics.FillRectangle(DrawingBrush, p1x, p1y, p2x - p1x, p2y - p1y);
			ToolLayer.Graphics.DrawRectangle(DrawingPen, p1x, p1y, p2x - p1x, p2y - p1y);
		}
		/// <summary>
		/// Повертає копію цього інструменту.
		/// </summary>
		/// <returns>Копія інструменту.</returns>
		public override ToolBase Clone()
		{
			RectangleTool clone = new RectangleTool(ToolColor, ToolWidth);
			clone.ToolLayer = ToolLayer;
			clone.InitialPoint = InitialPoint;
			//clone.UsedPoint = UsedPoint;
			clone.FillColor = FillColor;
			return clone;
		}
	}
}
