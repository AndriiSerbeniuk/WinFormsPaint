using System.Drawing;

namespace dotnetpaint
{
	/// <summary>
	/// Клас, який представляє інструмент "пензлик".
	/// </summary>
	class BrushTool : ToolBase
	{
		/// <summary>
		/// Конструктор, який ініціалізує інструмент значеннями за замовчуванням.
		/// </summary>
		public BrushTool() : base()
		{ }
		/// <summary>
		/// Конструктор, який ініціалізує інструмент значеннями із параметрів.
		/// </summary>
		/// <param name="color">Основний колір інструменту.</param>
		/// <param name="width">Ширина інструменту.</param>
		public BrushTool(Color color, int width) : base(color, width)
		{
			FillColor = color;
		}
		/// <summary>
		/// Готує інструмент до роботи: встановлює початкову точку малювання.
		/// </summary>
		/// <param name="InitPoint">Початкова точка.</param>
		/// <param name="workLayer">Шар, з яким працює інструмент.</param>
		public override void InitTool(Point InitPoint, Layer workLayer)
		{
			ToolLayer = workLayer;
			if (ToolLayer.Visible)
			{
				InitialPoint = InitPoint;
				DrawingBrush.Color = DrawingPen.Color;
				ToolLayer.Graphics.FillEllipse(DrawingBrush, InitPoint.X - ToolWidth / 2, InitPoint.Y - ToolWidth / 2, ToolWidth, ToolWidth);
			}
		}
		/// <summary>
		/// Використовує інструмент: малює лінію від початкової точки до точки використання, після чого точка використання стає початковою.
		/// </summary>
		/// <param name="UsePoint">Точка використання.</param>
		public override void UseTool(Point UsePoint)
		{
			if (ToolLayer.Visible)
			{
				if (ToolLayer.Visible)
				{
					ToolLayer.Graphics.DrawLine(DrawingPen, InitialPoint, UsePoint);
					InitialPoint = UsePoint;
				}
			}
		}
		/// <summary>
		/// Повертає копію цього інструменту.
		/// </summary>
		/// <returns>Копія інструменту.</returns>
		public override ToolBase Clone()
		{
			BrushTool clone = new BrushTool(ToolColor, ToolWidth);
			clone.ToolLayer = ToolLayer;
			clone.InitialPoint = InitialPoint;
			//clone.UsedPoint = UsedPoint;
			return clone;
		}
	}
}
