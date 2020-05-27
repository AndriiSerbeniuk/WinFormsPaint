using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace dotnetpaint
{
    class MoveTool : ToolBase
    {
        public MoveTool() : base()
        { }

        public MoveTool(int width) : base(Color.Transparent, width)
        { }

        public override void InitTool(Point InitPoint, Layer workLayer)
        {

            InitialPoint = InitPoint;
            ToolLayer = workLayer;

        }

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

        public override ToolBase Clone()
        {
            throw new NotImplementedException();
        }
    }
}
