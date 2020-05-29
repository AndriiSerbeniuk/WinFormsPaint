using System.Drawing;

namespace dotnetpaint
{
    /// <summary>
    /// Клас, який представляє інструмент для створення об'єкту TextArgs у робочому шарі.
    /// </summary>
    class TextInitTool : ToolBase
    {
        /// <summary>
		/// Конструктор, який ініціалізує інструмент значеннями за замовчуванням.
		/// </summary>
        public TextInitTool() : base()
        {
            FillColor = Color.Transparent;
            ToolWidth = 12;
        }
        /// <summary>
		/// Конструктор, який ініціалізує інструмент значеннями із параметрів.
		/// </summary>
		/// <param name="color">Основний колір інструменту.</param>
		/// <param name="width">Ширина інструменту.</param>
        public TextInitTool(Color color, int width) : base(color, width)
        {
            FillColor = Color.Transparent;
        }
        /// <summary>
		/// Використовує інструмент: створює в робочому шарі об'єкт TextArgs в позиції початкової точки.
		/// </summary>
		/// <param name="InitPoint">Початкова точка.</param>
		/// <param name="workLayer">Шар, з яким працює інструмент.</param>
        public override void InitTool(Point InitPoint, Layer workLayer)
        {
            workLayer.Text = new TextArgs();
            workLayer.Text.Color = ToolColor;
            workLayer.Text.Size = ToolWidth;
            workLayer.Text.Location = InitPoint;
        }

        /// <summary>
        /// Не використовується.
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
            TextInitTool clone = new TextInitTool(ToolColor, ToolWidth);
            clone.ToolLayer = ToolLayer;
            clone.InitialPoint = InitialPoint;
            clone.FillColor = FillColor;
            return clone;
        }
    }
}