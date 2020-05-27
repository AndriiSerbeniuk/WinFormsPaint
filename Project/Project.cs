using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;

namespace dotnetpaint
{
	[Serializable()]
	class Project : ISerializable
	{
		// Attributes
		private List<Layer> allLayers;
		private Layer selectedLayer;
		private int selectedLayerIndex;
		private Bitmap lowerLayers;
		private Bitmap upperLayers;
		private Size projectSize;
		private CompositingMode compositingMode;

		// Properties
		public List<Layer> AllLayers { get => allLayers; set => allLayers = value; }
		public Layer SelectedLayer { get => selectedLayer; }
		public Bitmap UpperLayers { get => upperLayers; }
		public Bitmap LowerLayers { get => lowerLayers;  }
		public CompositingMode Compositing 
		{ 
			get => compositingMode; 
			set
			{
				if (value != compositingMode)
				{
					compositingMode = value;
					foreach (Layer l in allLayers)
						l.Compositing = value;
				}
			}
		}
		public int SelectedLayerIndex 
		{ 
			get => selectedLayerIndex; 
			set => SelectLayer(value);   
		}
		public Size Size 
		{ 
			get => projectSize;
			set
			{
				projectSize = value;
				foreach(Layer l in allLayers)
				{
					l.LayerSize = value;
				}
			}
		}
		public int Width { get => projectSize.Width; }
		public int Height { get => projectSize.Height; }

		// ============ Constructors ==============
		public Project(Size projSize)
		{
			allLayers = new List<Layer>(0);
			lowerLayers = null;
			upperLayers = null;
			selectedLayer = null;
			selectedLayerIndex = -1;
			projectSize = projSize;         
		}

		public Project(SerializationInfo info, StreamingContext context)
		{
			allLayers = (List<Layer>)info.GetValue("layers", typeof(List<Layer>));
			//Layer[] layers = (Layer[])info.GetValue("layers", typeof(Layer[]));
			//allLayers = new List<Layer>(layers);
			projectSize = (Size)info.GetValue("size", typeof(Size));
			//SelectLayer(allLayers.Count - 1);
			Compositing = CompositingMode.SourceOver;
		}

		//[OnDeserialized]
		//private void SetValuesOnDeserialized(StreamingContext context)
		//{
		//	//allLayers = new List<Layer>();


		//}

		void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("layers", allLayers, typeof(List<Layer>));
			//Layer[] layers = allLayers.ToArray();
			//info.AddValue("layers", layers, typeof(Layer[]));
			info.AddValue("size", projectSize, typeof(Size));
		}

		~Project()
		{
			allLayers.Clear();
			if (lowerLayers != null)
				lowerLayers.Dispose();
			if (upperLayers != null)
				upperLayers.Dispose();
		}

		// ============= Methods =======================
		public void GetFinalImage(Layer DrawTo)
		{
			if (lowerLayers != null)
				DrawTo.Graphics.DrawImage(lowerLayers, 0, 0);
			if (selectedLayer != null && selectedLayer.Image != null && selectedLayer.Visible)
				DrawTo.Graphics.DrawImage(selectedLayer.Image, selectedLayer.Location.X, selectedLayer.Location.Y, selectedLayer.Width, selectedLayer.Height);
			if (upperLayers != null)
				DrawTo.Graphics.DrawImage(upperLayers, 0, 0);
		}


		public Layer SelectLayer(int index) 
		{
			if (index < 0 || index >= allLayers.Count)
				throw new IndexOutOfRangeException();
			else
			{
				selectedLayerIndex = index;
				selectedLayer = allLayers[index];
				
				SetLowerLayers();
				SetUpperLayers();
				return allLayers[selectedLayerIndex];
			}
		}

		private void SetLowerLayers()
		{
			if (selectedLayerIndex > 0)
			{
				if (lowerLayers == null)
					lowerLayers = new Bitmap(projectSize.Width, projectSize.Height);
				Graphics g = Graphics.FromImage(lowerLayers);
				g.Clear(Color.Transparent);
				for (int i = 0; i < selectedLayerIndex; i++)
					if (allLayers[i].Visible)
						g.DrawImage(allLayers[i].Image, allLayers[i].Location.X, allLayers[i].Location.Y, allLayers[i].LayerSize.Width, allLayers[i].LayerSize.Height);
						//g.DrawImage(allLayers[i].Image, allLayers[i].Location.X, allLayers[i].Location.Y);

			}
			else if (lowerLayers != null)
			{
				lowerLayers.Dispose();
				lowerLayers = null;
			}
		}

		private void SetUpperLayers()
		{
			if (selectedLayerIndex < allLayers.Count - 1)
			{
				if (upperLayers == null)
					upperLayers = new Bitmap(projectSize.Width, projectSize.Height);
				Graphics g = Graphics.FromImage(upperLayers);
				g.Clear(Color.Transparent);
				for (int i = selectedLayerIndex + 1; i < allLayers.Count; i++)
					if (allLayers[i].Visible)
						g.DrawImage(allLayers[i].Image, allLayers[i].Location.X, allLayers[i].Location.Y, allLayers[i].LayerSize.Width, allLayers[i].LayerSize.Height);
						//g.DrawImage(allLayers[i].Image, allLayers[i].Location.X, allLayers[i].Location.Y);
			}
			else if(upperLayers != null)
			{
				upperLayers.Dispose();
				upperLayers = null;
			}
		}

		public void AddLayer(string name)
		{
			allLayers.Add(new Layer(Size, name));
			if (allLayers.Count == 1)
				SelectLayer(0);
		}
		public void AddLayer(Layer layer, string name)
		{
			allLayers.Add(layer);
			if (allLayers.Count == 1)
				SelectLayer(0);
		}

		public void RemoveLayer(int index)
		{
			allLayers.RemoveAt(index); 
		}

		public void SwapLayers(int ind1, int ind2)
		{
			Layer temp = allLayers[ind1];
			allLayers[ind1] = allLayers[ind2];
			allLayers[ind2] = temp;
		}

		
	}
}
