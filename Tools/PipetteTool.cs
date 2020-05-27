using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace dotnetpaint
{
	class PipetteTool : ToolBase
	{
		public PipetteTool() : base()
		{
			FillColor = Color.Transparent;
			ToolWidth = 12;
		}

		public override void InitTool(Point InitPoint, Layer workLayer)
		{
			Color color = workLayer.Image.GetPixel(InitPoint.X, InitPoint.Y);
			ToolColor = color;
		}

		public override void UseTool(Point UsePoint)
		{ }

		public override ToolBase Clone()
		{
			PipetteTool clone = new PipetteTool();
			clone.ToolLayer = ToolLayer;
			clone.InitialPoint = InitialPoint;
			return clone;
		}
	}
}
