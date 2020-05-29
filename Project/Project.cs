using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;

namespace dotnetpaint
{
	/// <summary>
	/// Клас проекту, який містить шари із зображеннями, які накладаються одне на одного.
	/// </summary>
	[Serializable()]
	class Project : ISerializable
	{
		// Атрибути
		/// <summary>
		/// Всі шари проекту.
		/// </summary>
		private List<Layer> layers;
		/// <summary>
		/// Поточний обраний шар.
		/// </summary>
		private Layer selectedLayer;
		/// <summary>
		/// Індекс обраного шару.
		/// </summary>
		private int selectedLayerIndex;
		/// <summary>
		/// Шари, шо знаходять під обраним, у вигляді об'єднаного зображення.
		/// </summary>
		private Bitmap lowerLayers;
		/// <summary>
		/// Шари, шо знаходять над обраним, у вигляді об'єднаного зображення.
		/// </summary>
		private Bitmap upperLayers;
		/// <summary>
		/// Розмір проекту
		/// </summary>
		private Size size;
		/// <summary>
		/// Режим композиції усіх шарів проекту.
		/// </summary>
		private CompositingMode compositingMode;

		// Властивості
		/// <summary>
		/// Властивість для роботи із усіма шарами проекту.
		/// </summary>
		public List<Layer> Layers { get => layers; set => layers = value; }
		/// <summary>
		/// Властивість для отримання із вибраним шаром проекту.
		/// </summary>
		public Layer SelectedLayer { get => selectedLayer; }
		/// <summary>
		/// Властивість для отримання із зображенням шарів над обраним.
		/// </summary>
		public Bitmap UpperLayers { get => upperLayers; }
		/// <summary>
		/// Властивість для отримання із зображенням шарів під обраним.
		/// </summary>
		public Bitmap LowerLayers { get => lowerLayers; }
		/// <summary>
		/// Властивість для роботи із зображенням шарів над обраним.
		/// </summary>
		public CompositingMode Compositing 
		{ 
			get => compositingMode; 
			set
			{
				if (value != compositingMode)
				{
					compositingMode = value;
					foreach (Layer l in layers)
						l.Compositing = value;
				}
			}
		}
		/// <summary>
		/// Властивість для роботи із індексом обраного шару.
		/// </summary>
		public int SelectedLayerIndex 
		{ 
			get => selectedLayerIndex; 
			set => SelectLayer(value);   
		}
		/// <summary>
		/// Властивість для роботи із розміром проекту.
		/// </summary>
		public Size Size 
		{ 
			get => size;
			set
			{
				size = value;
				foreach(Layer l in layers)
				{
					l.LayerSize = value;
				}
			}
		}
		/// <summary>
		/// Властивість для роботи із шириною проекту.
		/// </summary>
		public int Width { get => size.Width; }
		/// <summary>
		/// Властивість для роботи із висотою проекту.
		/// </summary>
		public int Height { get => size.Height; }

		// ============ Конструктори ==============
		public Project(Size projSize)
		{
			layers = new List<Layer>(0);
			lowerLayers = null;
			upperLayers = null;
			selectedLayer = null;
			selectedLayerIndex = -1;
			size = projSize;         
		}
		/// <summary>
		/// Конструктор десеріалізації.
		/// </summary>
		public Project(SerializationInfo info, StreamingContext context)
		{
			layers = (List<Layer>)info.GetValue("layers", typeof(List<Layer>));
			//Layer[] layers = (Layer[])info.GetValue("layers", typeof(Layer[]));
			//allLayers = new List<Layer>(layers);
			size = (Size)info.GetValue("size", typeof(Size));
			//SelectLayer(allLayers.Count - 1);
			Compositing = CompositingMode.SourceOver;
		}
		/// <summary>
		/// Метод для серіалізації проекту.
		/// </summary>
		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("layers", layers, typeof(List<Layer>));
			//Layer[] layers = allLayers.ToArray();
			//info.AddValue("layers", layers, typeof(Layer[]));
			info.AddValue("size", size, typeof(Size));
		}
		~Project()
		{
			if (layers != null)
				layers.Clear();
			if (lowerLayers != null)
				lowerLayers.Dispose();
			if (upperLayers != null)
				upperLayers.Dispose();
		}
		// ============= Методи =======================
		/// <summary>
		/// Поєднання усіх шарів проекту в одне хображення.
		/// </summary>
		/// <param name="DrawTo">Шар, в який буде поміщено результат.</param>
		public void GetFinalImage(Layer DrawTo)
		{
			// Незалежно від кількості шарів завжди буде малюватись тільки 3 зображення для поєднання їх всіх.
			// Зображення шарів під вибраним робочим шаром.
			if (lowerLayers != null)
				DrawTo.Graphics.DrawImage(lowerLayers, 0, 0);
			// Сам робочий шар.
			if (selectedLayer != null && selectedLayer.Image != null && selectedLayer.Visible)
				DrawTo.Graphics.DrawImage(selectedLayer.Image, selectedLayer.Location.X, selectedLayer.Location.Y, selectedLayer.Width, selectedLayer.Height);
			// Та зображення шарів над робочим шаром.
			if (upperLayers != null)
				DrawTo.Graphics.DrawImage(upperLayers, 0, 0);
		}
		/// <summary>
		/// Обрати робочий шар із шарів проекту.
		/// Спричиняє IndexOutOfRangeException при неправильному індексі шару.
		/// </summary>
		/// <param name="index">Індекс шару для вибору.</param>
		/// <returns>Повертає новообраний шар.</returns>
		public Layer SelectLayer(int index) 
		{
			if (index < 0 || index >= layers.Count)
				throw new IndexOutOfRangeException();
			else
			{
				selectedLayerIndex = index;
				selectedLayer = layers[index];
				// При виборі нового шару порібно перевірити заново, які шари над ним, а які під.
				SetLowerLayers();
				SetUpperLayers();
				return layers[selectedLayerIndex];
			}
		}
		/// <summary>
		/// Об'єднує усі шари під вибраним в одне зображення.
		/// </summary>
		private void SetLowerLayers()
		{
			if (selectedLayerIndex > 0)
			{
				lowerLayers = new Bitmap(size.Width, size.Height);
				Graphics g = Graphics.FromImage(lowerLayers);
				g.Clear(Color.Transparent);
				for (int i = 0; i < selectedLayerIndex; i++)
					if (layers[i].Visible)
						g.DrawImage(layers[i].Image, layers[i].Location.X, layers[i].Location.Y, layers[i].LayerSize.Width, layers[i].LayerSize.Height);

			}
			else if (lowerLayers != null)
			{
				lowerLayers.Dispose();
				lowerLayers = null;
			}
		}
		/// <summary>
		/// Об'єднує усі шари над вибраним в одне зображення.
		/// </summary>
		private void SetUpperLayers()
		{
			if (selectedLayerIndex < layers.Count - 1)
			{
				upperLayers = new Bitmap(size.Width, size.Height);
				Graphics g = Graphics.FromImage(upperLayers);
				g.Clear(Color.Transparent);
				for (int i = selectedLayerIndex + 1; i < layers.Count; i++)
					if (layers[i].Visible)
						g.DrawImage(layers[i].Image, layers[i].Location.X, layers[i].Location.Y, layers[i].LayerSize.Width, layers[i].LayerSize.Height);
			}
			else if(upperLayers != null)
			{
				upperLayers.Dispose();
				upperLayers = null;
			}
		}
		/// <summary>
		/// Додає новий шар із заданим ім'ям в проект.
		/// </summary>
		/// <param name="name">Ім'я шару.</param>
		public void AddLayer(string name)
		{
			layers.Add(new Layer(Size, name));
			if (layers.Count == 1)
				SelectLayer(0);
		}
		/// <summary>
		/// Додає новий шар в проект.
		/// </summary>
		/// <param name="layer">Шар, який потрібно додати.</param>
		public void AddLayer(Layer layer)
		{
			layers.Add(layer);
			if (layers.Count == 1)
				SelectLayer(0);
		}
		/// <summary>
		/// Видаляє шар із заданим індексом із проекту.
		/// </summary>
		/// <param name="index">Індекс шару для видалення.</param>
		public void RemoveLayer(int index)
		{
			if (index >= 0 && index < layers.Count)
				layers.RemoveAt(index); 
		}
		/// <summary>
		/// Міняє два шари проекту місцями.
		/// </summary>
		/// <param name="ind1">Індекс першого шару.</param>
		/// <param name="ind2">Індекс другого шару.</param>
		public void SwapLayers(int ind1, int ind2)
		{
			if (ind1 >= 0 && ind1 < layers.Count && ind2 >= 0 && ind2 < layers.Count)
			{
				Layer temp = layers[ind1];
				layers[ind1] = layers[ind2];
				layers[ind2] = temp;
			}
		}
	}
}