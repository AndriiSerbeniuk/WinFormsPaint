using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Windows.Forms;
using System.Runtime.Serialization;
using System.Drawing.Imaging;

namespace dotnetpaint
{
	[Serializable()]
	class Layer : ISerializable
	{
		private Bitmap layerImage;
		//[NonSerialized]
		private Graphics layerGraphics;
		private string layerName;
		//[NonSerialized]
		private Size layerSize;
		private bool visible;
		private Point location;
		public Point Location { get => location; set => location = value; }

		public Bitmap Image 
		{ 
			get => layerImage; 
			set
			{
				layerImage.Dispose();
				layerImage = value;
				if (layerGraphics != null)
					layerGraphics.Dispose();
				layerGraphics = Graphics.FromImage(layerImage);
				layerSize = layerImage.Size;
			}
		}

		public Graphics Graphics { get => layerGraphics; }
		public string Name { get => layerName; set => layerName = value; }
		public bool Visible { get => visible; set => visible = value; }

		public CompositingMode Compositing { get => layerGraphics.CompositingMode; set => layerGraphics.CompositingMode = value; }
		public Size LayerSize 
		{ 
			get => layerSize;
			set
			{
				layerSize = value;
				Bitmap newbmp = new Bitmap(layerSize.Width, layerSize.Height);
				Graphics newgrphcs = Graphics.FromImage(newbmp);
				newgrphcs.DrawImage(layerImage, 0, 0, newbmp.Width, newbmp.Height);
				layerImage.Dispose();
				layerGraphics.Dispose();
				layerImage = newbmp;
				layerGraphics = newgrphcs;
			}
		}
		public int Width 
		{ 
			get => layerSize.Width; 
			set
			{
				layerSize.Width = value;
				Bitmap newbmp = new Bitmap(layerSize.Width, layerSize.Height);
				Graphics newgrphcs = Graphics.FromImage(newbmp);
				newgrphcs.DrawImage(layerImage, 0, 0, newbmp.Width, newbmp.Height);
				layerImage.Dispose();
				layerGraphics.Dispose();
				layerImage = newbmp;
				layerGraphics = newgrphcs;
			}
		}
		public int Height 
		{ 
			get => layerSize.Height;
			set
			{
				layerSize.Height = value;
				Bitmap newbmp = new Bitmap(layerSize.Width, layerSize.Height);
				Graphics newgrphcs = Graphics.FromImage(newbmp);
				newgrphcs.DrawImage(layerImage, 0, 0, newbmp.Width, newbmp.Height);
				layerImage.Dispose();
				layerGraphics.Dispose();
				layerImage = newbmp;
				layerGraphics = newgrphcs;
			}
		}
		[NonSerialized]
		private TShape shape;
		public TShape Shape { get => shape; set => shape = value; }
		[NonSerialized]
		private TextArgs text;
		public TextArgs Text { get => text; set => text = value; }

		// ===================================
		public Layer(Size _layerSize, string name)
		{
			layerImage = new Bitmap(_layerSize.Width, _layerSize.Height);
			layerGraphics = Graphics.FromImage(layerImage);

			layerGraphics.SmoothingMode = SmoothingMode.AntiAlias;
			layerGraphics.Clear(Color.Transparent);

			layerSize = _layerSize;
			layerName = name;
			visible = true;
			shape = null;
			text = null;

			location = Point.Empty;
		}

		public Layer(Size _layerSize)
		{
			layerImage = new Bitmap(_layerSize.Width, _layerSize.Height);
			layerGraphics = Graphics.FromImage(layerImage);
			layerGraphics.SmoothingMode = SmoothingMode.AntiAlias;
			layerGraphics.Clear(Color.Transparent);

			layerSize = _layerSize;
			layerName = "";
			visible = true;
			shape = null;
			text = null;

			location = Point.Empty;
		}
		public Layer(SerializationInfo info, StreamingContext context)
		{
			Bitmap temp = (Bitmap)info.GetValue("image", typeof(Bitmap));
			layerImage = new Bitmap(temp.Width, temp.Height, (PixelFormat)info.GetValue("pixelFormat", typeof(PixelFormat)));

			layerGraphics = Graphics.FromImage(layerImage);
			layerGraphics.SmoothingMode = SmoothingMode.AntiAlias;
			layerGraphics.CompositingMode = (CompositingMode)info.GetValue("compositing", typeof(CompositingMode));
			layerGraphics.DrawImage(temp, Point.Empty);
			layerSize = layerImage.Size;
			layerName = (string)info.GetValue("name", typeof(string));
			visible = (bool)info.GetValue("visible", typeof(bool));
			location = (Point)info.GetValue("location", typeof(Point));

			//for now
			shape = null;
			text = (TextArgs)info.GetValue("textArgs", typeof(TextArgs));
			shape = (TShape)info.GetValue("shape", typeof(TShape));
			if (shape != null)
				shape.Tool.ToolLayer = this;
		}
		//[OnDeserialized]
		//private void SetValuesOnDeserialized(StreamingContext context)
		//{
		//	//allLayers = new List<Layer>();


		//}
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("image", layerImage, typeof(Bitmap));
			info.AddValue("pixelFormat", layerImage.PixelFormat, typeof(PixelFormat));

			info.AddValue("name", layerName, typeof(string));
			info.AddValue("visible", visible, typeof(bool));
			info.AddValue("location", location, typeof(Point));
			info.AddValue("compositing", layerGraphics.CompositingMode, typeof(CompositingMode));

			info.AddValue("textArgs", text, typeof(TextArgs));
			info.AddValue("shape", shape, typeof(TShape));
		}

		~Layer()
		{
			if (layerGraphics != null)
				layerGraphics.Dispose();
			if (layerImage != null)
				layerImage.Dispose();
		}
		public void RefreshContents()
		{
			layerGraphics.Clear(Color.Transparent);
			if (shape != null)
			{
				shape.Tool.InitTool(shape.Point1, shape.Tool.ToolLayer);
				shape.Tool.UseTool(shape.Point2);
			}
			else if (text != null)
			{
				Graphics.DrawString(text.Text, text.Font, text.Brush, text.Location);
			}
		}


	}
}
