using System;
using System.Drawing;
using System.Runtime.Serialization;

namespace dotnetpaint
{
	/// <summary>
	/// Представляє основу інструменту графічного редактору.
	/// </summary>
	[Serializable]
	abstract class ToolBase : ISerializable
	{
		/// <summary>
		/// Шар, з яким працює інструмент.
		/// </summary>
		private Layer toolLayer;
		/// <summary>
		/// Основний колір інструменту.
		/// </summary>
		private Color toolColor;
		/// <summary>
		/// Колір заливки інструменту.
		/// </summary>
		private Color fillColor;
		/// <summary>
		/// Ширина інструменту.
		/// </summary>
		private int toolWidth;
		/// <summary>
		/// Ручка, якою малює інструмент.
		/// </summary>
		private Pen drawingPen;
		/// <summary>
		/// Пензлик, яким малює інструмент.
		/// </summary>
		private SolidBrush drawingBrush;
		/// <summary>
		/// Початкова точка роботи інструменту.
		/// </summary>
		protected Point InitialPoint;
		/// <summary>
		/// Властивість для доступу до шару інструменту.
		/// </summary>
		public Layer ToolLayer { get => toolLayer; set => toolLayer = value; }
		/// <summary>
		/// Властивість для доступу до основного кольору інструменту.
		/// </summary>
		public Color ToolColor 
		{ 
			get => toolColor; 
			set 
			{ 
				toolColor = value; 
				drawingPen.Color = value; 
			} 
		}
		/// <summary>
		/// Властивість для доступу до кольору заливки інструменту.
		/// </summary>
		public Color FillColor
		{
			get => fillColor;
			set
			{
				fillColor = value;
				if (drawingBrush == null)
					drawingBrush = new SolidBrush(value);
				else
					DrawingBrush.Color = value;
			}
		}
		/// <summary>
		/// Властивість для доступу до ширини інструменту.
		/// </summary>
		public int ToolWidth { get => toolWidth; set { toolWidth = value; drawingPen.Width = value; } }
		/// <summary>
		/// Властивість для доступу до ручки інструменту.
		/// </summary>
		public Pen DrawingPen 
		{ 
			get => drawingPen; 
			set
			{
				drawingPen = value;
				toolWidth = (int)drawingPen.Width;
			}
		}
		/// <summary>
		/// Властивість для доступу до пензлика інструменту.
		/// </summary>
		public SolidBrush DrawingBrush { get => drawingBrush; set => drawingBrush = value; }
		/// <summary>
		/// Конструктор, який ініціалізує інструмент значеннями за замовчуванням.
		/// </summary>
		public ToolBase()
		{
			toolWidth = 5;
			toolColor = Color.Black;
			drawingPen = new Pen(Color.Black, 5);
			drawingPen.StartCap = drawingPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
			drawingBrush = new SolidBrush(Color.Black);
		}
		/// <summary>
		/// Конструктор, який ініціалізує інструмент значеннями із параметрів.
		/// </summary>
		/// <param name="color">Основний колір інструменту.</param>
		/// <param name="width">Ширина інструменту.</param>
		public ToolBase(Color color, int width)
		{
			toolWidth = width;
			toolColor = color;
			drawingPen = new Pen(color, width);
			drawingPen.StartCap = drawingPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
			drawingBrush = new SolidBrush(color);
		}
		/// <summary>
		/// Конструктор десеріалізації.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		public ToolBase(SerializationInfo info, StreamingContext context)
		{
			toolWidth = (int)info.GetValue("width", typeof(int));
			toolColor = (Color)info.GetValue("primaryColor", typeof(Color));
			fillColor = (Color)info.GetValue("fillColor", typeof(Color));
		}
		/// <summary>
		/// Метод серіалізації.
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("primaryColor", toolColor);
			info.AddValue("fillColor", fillColor);
			info.AddValue("width", toolWidth);
		}
		/// <summary>
		/// Готує інструмент до роботи.
		/// </summary>
		/// <param name="InitPoint">Початкова точка.</param>
		/// <param name="workLayer">Шар, з яким працює інструмент.</param>
		public abstract void InitTool(Point InitPoint, Layer workLayer);
		/// <summary>
		/// Використовує інструмент.
		/// </summary>
		/// <param name="UsePoint">Точка використання.</param>
		public abstract void UseTool(Point UsePoint);
		/// <summary>
		/// Повертає копію цього інструменту.
		/// </summary>
		/// <returns>Копія інструменту.</returns>
		public abstract ToolBase Clone();
		~ToolBase()
		{
			if (drawingBrush != null)
				drawingBrush.Dispose();
			if (drawingPen != null)
				drawingPen.Dispose();
		}
	}
}