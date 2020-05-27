using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace dotnetpaint
{
	[Serializable]
	abstract class ToolBase : ISerializable
	{
		private Layer toolLayer;
		public Layer ToolLayer { get => toolLayer; set => toolLayer = value; }

		private Color toolColor;
		public virtual Color ToolColor 
		{ 
			get => toolColor; 
			set 
			{ 
				toolColor = value; 
				drawingPen.Color = value; 
			} 
		}
		private Color fillColor;

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

		private int toolWidth;
		public int ToolWidth { get => toolWidth; set { toolWidth = value; drawingPen.Width = value; } }

		private Pen drawingPen;
		public Pen DrawingPen { get => drawingPen; /*set => drawingPen = value;*/ }

		private SolidBrush drawingBrush;
		public SolidBrush DrawingBrush { get => drawingBrush; set => drawingBrush = value; }

		protected Point InitialPoint;
		//protected Point UsedPoint;

		public ToolBase()
		{
			toolWidth = 5;
			toolColor = Color.Black;
			drawingPen = new Pen(Color.Black, 5);
			drawingPen.StartCap = drawingPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
			drawingBrush = new SolidBrush(Color.Black);
		}

		public ToolBase(Color color, int width)
		{
			toolWidth = width;
			toolColor = color;
			drawingPen = new Pen(color, width);
			drawingPen.StartCap = drawingPen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
			drawingBrush = new SolidBrush(color);
		}

		public ToolBase(SerializationInfo info, StreamingContext context)
		{
			toolWidth = (int)info.GetValue("width", typeof(int));
			toolColor = (Color)info.GetValue("primaryColor", typeof(Color));
			fillColor = (Color)info.GetValue("fillColor", typeof(Color));
		}

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("primaryColor", toolColor);
			info.AddValue("fillColor", fillColor);
			info.AddValue("width", toolWidth);
		}

		public abstract void InitTool(Point InitPoint, Layer workLayer);
		public abstract void UseTool(Point UsePoint);

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
