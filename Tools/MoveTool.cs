using System;
using System.Drawing;

namespace dotnetpaint
{
	/// <summary>
	/// Клас, який представляє інструмент для переміщення шару.
	/// </summary>
	class MoveTool : ToolBase
	{
		/// <summary>
		/// Конструктор, який ініціалізує інструмент значеннями за замовчуванням.
		/// </summary>
		public MoveTool() : base()
		{ }
		/// <summary>
		/// Готує інструмент до роботи: встановлює початкову точку, яка представляє початкове розташування шару, та робочий шар..
		/// </summary>
		/// <param name="InitPoint">Початкова точка.</param>
		/// <param name="workLayer">Шар, з яким працює інструмент.</param>
		public override void InitTool(Point InitPoint, Layer workLayer)
		{
			InitialPoint = InitPoint;
			ToolLayer = workLayer;
		}
		/// <summary>
		/// Використовує інструмент: визначає горизонтальну та вертикальну відстань між початковою точкою та точкою використання, та переміщає на неї шар.
		/// </summary>
		/// <param name="UsePoint">Точка використання.</param>
		public override void UseTool(Point UsePoint)
		{
			if (ToolLayer.Visible)
			{
				Point newLoc;
				if (ToolLayer.Text != null)
					newLoc = ToolLayer.Text.Location;
				else
					newLoc = ToolLayer.Location;

				if (ToolLayer.Shape == null)
				{
					newLoc.X += UsePoint.X - InitialPoint.X;
					newLoc.Y += UsePoint.Y - InitialPoint.Y;
				}

				if (ToolLayer.Text != null)
				{
					ToolLayer.Text.Location = newLoc;
					ToolLayer.RefreshContents();
				}
				else if (ToolLayer.Shape != null)
				{
					int xOffset = UsePoint.X - InitialPoint.X;
					int yOffset = UsePoint.Y - InitialPoint.Y;
					newLoc = ToolLayer.Shape.Point1;
					newLoc.X += xOffset;
					newLoc.Y += yOffset;
					ToolLayer.Shape.Point1 = newLoc;
					newLoc = ToolLayer.Shape.Point2;
					newLoc.X += xOffset;
					newLoc.Y += yOffset;
					ToolLayer.Shape.Point2 = newLoc;
					ToolLayer.RefreshContents();
				}
				else
					ToolLayer.Location = newLoc;
				InitialPoint = UsePoint;
			}
		}
		/// <summary>
		/// Повертає копію цього інструменту.
		/// </summary>
		/// <returns>Копія інструменту.</returns>
		public override ToolBase Clone()
		{
			MoveTool clone = new MoveTool();
			return clone;
		}
	}
}
