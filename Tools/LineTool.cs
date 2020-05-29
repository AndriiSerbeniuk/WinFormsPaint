using System;
using System.Drawing;
using System.Runtime.Serialization;

namespace dotnetpaint
{
    /// <summary>
    /// Клас, який представляє інструмент для створення ліній.
    /// </summary>
    [Serializable]
    class LineTool : ToolBase, ISerializable
    {
        /// <summary>
		/// Конструктор, який ініціалізує інструмент значеннями за замовчуванням.
		/// </summary>
        public LineTool() : base()
        { }
        /// <summary>
		/// Конструктор, який ініціалізує інструмент значеннями із параметрів.
		/// </summary>
		/// <param name="color">Основний колір інструменту.</param>
		/// <param name="width">Ширина інструменту.</param>
        public LineTool(Color color, int width) : base(color, width)
        { }
        /// <summary>
		/// Конструктор десеріалізації.
		/// </summary>
        public LineTool(SerializationInfo info, StreamingContext context)
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
		/// Готує інструмент до роботи: встановлює першу точку лінії.
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
            ToolLayer.Shape.Point1 = InitPoint;
        }
        /// <summary>
		/// Використовує інструмент: малює лінію між початковою точкою та точкою використання.
		/// </summary>
		/// <param name="UsePoint">Точка використання.</param>
        public override void UseTool(Point UsePoint)
        {
            ToolLayer.Graphics.Clear(Color.Transparent);
            ToolLayer.Graphics.DrawLine(DrawingPen, InitialPoint, UsePoint);
            ToolLayer.Shape.Point2 = UsePoint;
        }
        /// <summary>
		/// Повертає копію цього інструменту.
		/// </summary>
		/// <returns>Копія інструменту.</returns>
        public override ToolBase Clone()
        {
            LineTool clone = new LineTool(ToolColor, ToolWidth);
            clone.ToolLayer = ToolLayer;
            clone.InitialPoint = InitialPoint;
            return clone;
        }
    }
}
