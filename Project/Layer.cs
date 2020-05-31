using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;
using System.Drawing.Imaging;

namespace dotnetpaint
{
	/// <summary>
	/// Клас, який представляє шар для малювання.
	/// </summary>
	[Serializable()]
	class Layer : ISerializable
	{
		/// <summary>
		/// Зображення шару.
		/// </summary>
		private Bitmap layerImage;
		/// <summary>
		/// Об'єкт Graphics для роботи із зображенням.
		/// </summary>
		private Graphics layerGraphics;
		/// <summary>
		/// Ім'я шару.
		/// </summary>
		private string layerName;
		/// <summary>
		/// Розмір шару.
		/// </summary>
		private Size layerSize;
		/// <summary>
		/// Мітка, що позначає видимість шару.
		/// </summary>
		private bool visible;
		/// <summary>
		/// Позиція шару на робочій поверхні.
		/// </summary>
		private Point location;
		/// <summary>
		/// Дані про фігуру, яка міститься в шарі.
		/// </summary>
		private TShape shape;
		/// <summary>
		/// Дані про текст, який міститься в шарі.
		/// </summary>
		private TextArgs text;
		/// <summary>
		/// Властивість для роботи із зображенням шару.
		/// </summary>
		public Bitmap Image 
		{ 
			/// Повертає зображення шару.
			get => layerImage; 
			/// Присвоює значення шару, при цьому, можливо, змінюючи його розмір та перестворюючи об'єкт Graphics.
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
		/// <summary>
		/// Дозволяє отримати об'єкт Graphics для роботи із шаром.
		/// </summary>
		public Graphics Graphics { get => layerGraphics; }
		/// <summary>
		/// Властивість для присвоювання або отримання імені шару.
		/// </summary>
		public string Name { get => layerName; set => layerName = value; }
		/// <summary>
		/// Властивість для присвоювання або отримання мітки видимості шару.
		/// </summary>
		public bool Visible { get => visible; set => visible = value; }
		/// <summary>
		/// Властивість, що дозволяє змінювати режим композиції об'єкту Graphics цього шару.
		/// </summary>
		public CompositingMode Compositing { get => layerGraphics.CompositingMode; set => layerGraphics.CompositingMode = value; }
		/// <summary>
		/// Властивість для роботи із розміром шару.
		/// </summary>
		public Size LayerSize 
		{ 
			/// Повертає розмір шару.
			get => layerSize;
			/// Змінює розмір шару, із зміною розміру зображення.
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
		/// <summary>
		/// Властивість для роботи із шириною шару.
		/// </summary>
		public int Width 
		{ 
			/// Повертає ширину шару.
			get => layerSize.Width; 
			/// Присвоює нову ширину шару, із зміною розміру зображення.
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
		/// <summary>
		/// Властивість для роботи із висотою шару.
		/// </summary>
		public int Height 
		{
			/// Повертає висоту шару.
			get => layerSize.Height;
			/// Присвоює нову висоту шару, із зміною розміру зображення.
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
		/// <summary>
		/// Властивість для роботи із позицією шару на робочій поверхні.
		/// </summary>
		public Point Location { get => location; set => location = value; }
		/// <summary>
		/// Властивість для роботи із фігурою шару.
		/// </summary>
		public TShape Shape { get => shape; set => shape = value; }
		/// <summary>
		/// Властивість для роботи із текстом шару.
		/// </summary>
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
		/// <summary>
		/// Конструкрот десеріалізації об'єкту шару.
		/// </summary>
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

			text = (TextArgs)info.GetValue("textArgs", typeof(TextArgs));
			shape = (TShape)info.GetValue("shape", typeof(TShape));
			if (shape != null)
				shape.Tool.ToolLayer = this;
		}
		/// <summary>
		/// Серіалізація шару.
		/// </summary>
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
		/// <summary>
		/// Метод, який обновлює зображення шару при наявності даних фігури або тексту.
		/// </summary>
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