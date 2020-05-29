using System.Drawing;

namespace dotnetpaint
{
	/// <summary>
	/// Клас, який представляє інструмент "піпетку" - інструмент бере колір пікселя в переданій позиції в зображенні робочого шару.
	/// </summary>
	class PipetteTool : ToolBase
	{
		/// <summary>
		/// Конструктор, який ініціалізує інструмент значеннями за замовчуванням.
		/// </summary>
		public PipetteTool() : base()
		{
			FillColor = Color.Transparent;
			ToolWidth = 12;
		}
		/// <summary>
		/// Використовує інструмент: встановлює колір інструменту значенням кольору пікселя в позиції початкової точки в зображенні робочого шару.
		/// </summary>
		/// <param name="InitPoint">Початкова точка.</param>
		/// <param name="workLayer">Шар, з яким працює інструмент.</param>
		public override void InitTool(Point InitPoint, Layer workLayer)
		{
			Color color = workLayer.Image.GetPixel(InitPoint.X, InitPoint.Y);
			ToolColor = color;
		}
		/// <summary>
		/// не використовується
		/// </summary>
		/// <param name="UsePoint">-</param>
		public override void UseTool(Point UsePoint)
		{}
		/// <summary>
		/// Повертає копію цього інструменту.
		/// </summary>
		/// <returns>Копія інструменту.</returns>
		public override ToolBase Clone()
		{
			PipetteTool clone = new PipetteTool();
			clone.ToolLayer = ToolLayer;
			clone.InitialPoint = InitialPoint;
			return clone;
		}
	}
}
