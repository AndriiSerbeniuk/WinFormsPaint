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
    class RectangleTool : ToolBase, ISerializable
    {
        public RectangleTool() : base()
        {
            FillColor = Color.Transparent;
        }

        public RectangleTool(Color color, int width) : base(color, width)
        {
            FillColor = Color.Transparent;
        }

        public RectangleTool(SerializationInfo info, StreamingContext context)
        {
            ToolWidth = (int)info.GetValue("width", typeof(int));
            ToolColor = (Color)info.GetValue("primaryColor", typeof(Color));
            FillColor = (Color)info.GetValue("fillColor", typeof(Color));
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("primaryColor", ToolColor);
            info.AddValue("fillColor", FillColor);
            info.AddValue("width", ToolWidth);
        }

        public override void InitTool(Point InitPoint, Layer workLayer)
        {
            InitialPoint = InitPoint;
            ToolLayer = workLayer;
            if (ToolLayer.Shape == null)
                ToolLayer.Shape = new TShape();
            ToolLayer.Shape.Tool = this;
        }

        public override void UseTool(Point UsePoint)
        {
            ToolLayer.Graphics.Clear(Color.Transparent);
            int p1x = Math.Min(UsePoint.X, InitialPoint.X),
                p1y = Math.Min(UsePoint.Y, InitialPoint.Y),
                p2x = Math.Max(UsePoint.X, InitialPoint.X),
                p2y = Math.Max(UsePoint.Y, InitialPoint.Y);

            ToolLayer.Shape.Point1 = new Point(p1x, p1y);
            ToolLayer.Shape.Point2 = new Point(p2x, p2y);
            if (DrawingBrush.Color.A != 0)
                ToolLayer.Graphics.FillRectangle(DrawingBrush, p1x, p1y, p2x - p1x, p2y - p1y);
            ToolLayer.Graphics.DrawRectangle(DrawingPen, p1x, p1y, p2x - p1x, p2y - p1y);
        }

        public override ToolBase Clone()
        {
            RectangleTool clone = new RectangleTool(ToolColor, ToolWidth);
            clone.ToolLayer = ToolLayer;
            clone.InitialPoint = InitialPoint;
            //clone.UsedPoint = UsedPoint;
            clone.FillColor = FillColor;
            return clone;
        }
    }
}
