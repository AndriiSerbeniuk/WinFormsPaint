using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dotnetpaint
{
    class TextInitTool : ToolBase
    {
        public TextInitTool() : base()
        {
            FillColor = Color.Transparent;
            ToolWidth = 12;
        }

        public TextInitTool(Color color, int width) : base(color, width)
        {
            FillColor = Color.Transparent;
        }

        public override void InitTool(Point InitPoint, Layer workLayer)
        {
            workLayer.Text = new TextArgs();
            workLayer.Text.Color = ToolColor;
            workLayer.Text.Size = ToolWidth;
            workLayer.Text.Location = InitPoint;
        }

        public override void UseTool(Point UsePoint)
        {
        }

        public override ToolBase Clone()
        {
            TextInitTool clone = new TextInitTool(ToolColor, ToolWidth);
            clone.ToolLayer = ToolLayer;
            clone.InitialPoint = InitialPoint;
            //clone.UsedPoint = UsedPoint;
            clone.FillColor = FillColor;
            return clone;
        }
    }
}
