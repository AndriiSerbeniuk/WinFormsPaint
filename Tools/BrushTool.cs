using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace dotnetpaint
{
	class BrushTool : ToolBase
	{
		public BrushTool() : base()
		{ }

		public BrushTool(Color color, int width) : base(color, width)
		{
			FillColor = color;
		}

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

		public override void UseTool(Point UsePoint)
		{
			if (ToolLayer.Visible)
			{
				//ToolLayer.Graphics.DrawLine(DrawingPen, InitialPoint.X - ToolLayer.Location.X, InitialPoint.Y - ToolLayer.Location.Y, UsePoint.X - ToolLayer.Location.X, UsePoint.Y - ToolLayer.Location.Y);
				if (ToolLayer.Visible)
				{
					ToolLayer.Graphics.DrawLine(DrawingPen, InitialPoint, UsePoint);
					InitialPoint = UsePoint;
				}
			}
		}

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
