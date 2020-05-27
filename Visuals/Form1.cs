using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;


namespace dotnetpaint
{
	public enum ToolUse { Draw, Shape, Transform };

	public partial class Courseach : Form
	{
		private bool DBMouseDown;
		private bool ModifyLayers;
		private ToolUse CurUse;
		private ToolUse PrevUse;

		private Layer BufferLayer;

		private Project project;

		private ToolBase[] Tools;
		private ToolBase CurrentTool;
		private ToolBase LastShapeTool;
		private ToolBase PrevTool;

		private ToolStripItem LastUsedButton;

		private string SavePath;

		public Courseach()
		{
			InitializeComponent();
			DBMouseDown = false;
			ModifyLayers = true;

			Tools = new ToolBase[9];
			Tools[0] = new BrushTool();                     // Brush
			Tools[1] = new BrushTool(Color.Transparent, 5); // Eraser
			Tools[2] = new LineTool();                      // Line tool
			Tools[3] = new RectangleTool();                 // Rectangle tool
			Tools[4] = new EllipseTool();                   // Ellipse tool
			Tools[5] = new MoveTool();                      // Affine translation tool
			Tools[6] = new TextInitTool();                  // Text initialisation tool
			Tools[7] = new PipetteTool();                   // Color copy from image for primary color
			Tools[8] = new PipetteTool();                   // Color copy from image for fill color

			CurrentTool = Tools[0];
			CurUse = ToolUse.Draw;
			LastShapeTool = Tools[2];
			ShapeToolsButton.Image = ShapeToolsButton.DropDownItems[0].Image;

			SetToolInfo();

			LastUsedButton = BrushButton;
			BrushButton.BackColor = Color.LightBlue;
			SavePath = null;

			DrawingBox.Refresh();
		}

		// ========== Project creation and saving ========================
		private void BlankProjButton_Click(object sender, EventArgs e)
		{
			ProjectCreationWindow d = new ProjectCreationWindow();
			if (d.ShowDialog() == DialogResult.OK)
			{
				InitProject(d.NewProjectSize);
			}
			d.Dispose();
		}

		
		private void ImageProjButton_Click(object sender, EventArgs e)
		{
			OpenFileDialog d = new OpenFileDialog();
			d.Filter = "Images (*.bmp;*.jpg;*.PNG,*.TIFF)|*.BMP;*.JPG;*.PNG;*.TIFF|" + "All files (*.*)|*.*";
			if (d.ShowDialog() == DialogResult.OK)
			{
				try
				{ 
					Bitmap bmp = new Bitmap(d.FileName);
					InitProject(bmp);
				}
				catch (IOException)
				{
					MessageBox.Show("An error occured while loading the image.", "Error");
				}
			}

			d.Dispose();
		}
		private void FileProjButton_Click(object sender, EventArgs e)
		{
			OpenFileDialog d = new OpenFileDialog();
			d.Filter = " (*.dat)|*.dat|" + "All files (*.*)|*.*";
			if (d.ShowDialog() == DialogResult.OK)
			{
				try
				{
					BinaryFormatter formatter = new BinaryFormatter();
					using (FileStream fs = new FileStream(d.FileName, FileMode.Open))
					{
						// In case a project already exists we should save it first
						Project temp = (Project)formatter.Deserialize(fs);
						if (project != null)
							SaveIntoFile(SavePath);
						project = temp;
						project.SelectLayer(project.AllLayers.Count - 1);
						SavePath = d.FileName;

						// Clear visual representation of prevoius project
						LayersTreeView.Nodes.Clear();
						InitProject();
						DrawingBox.Refresh();
						MessageBox.Show("Project loaded successfully.", "Success!");
					}
				}
				catch (IOException)
				{
					MessageBox.Show("An error occured while loading the file.", "Error");
				}
				catch (SerializationException)
				{
					MessageBox.Show("An error occured while loading the file.", "Error");
				}
			}

			d.Dispose();
		}

		private void InitProject()
		{
			BufferLayer = new Layer(project.Size);
			DrawingBox.Location = Point.Empty;
			CountDrawingBoxSize();

			// Add all the new layers into LayersTreeView for the user to see
			ModifyLayers = false;
			foreach (Layer l in project.AllLayers)
			{
				LayersTreeView.Nodes.Add(l.Name);
				LayersTreeView.Nodes[LayersTreeView.Nodes.Count - 1].Checked = l.Visible;
				if (LayersTreeView.Nodes.Count > 1)
				{
					string temp;
					bool tempchecked;
					for (int i = LayersTreeView.Nodes.Count - 1; i > 0; i--)
					{
						temp = LayersTreeView.Nodes[i].Text;
						tempchecked = LayersTreeView.Nodes[i].Checked;
						LayersTreeView.Nodes[i].Text = LayersTreeView.Nodes[i - 1].Text;
						LayersTreeView.Nodes[i].Checked = LayersTreeView.Nodes[i - 1].Checked;
						LayersTreeView.Nodes[i - 1].Text = temp;
						LayersTreeView.Nodes[i - 1].Checked = tempchecked;
					}
				}
			}
			ModifyLayers = true;

			AddLayerButton.Enabled = true;
			ToolParametersPanel.Visible = true;
			LayerSizePanel.Visible = true;
		}

		private void CountDrawingBoxSize()
		{
			Point newLoc = Point.Empty;
			// Relation of project width / height
			float PR = (float)project.Width / project.Height;
			// Relation of drawing surface width / height
			float DR = (float)DrawSurfPanel.Width / DrawSurfPanel.Height;
			if (PR < DR)
			{
				DrawingBox.Height = DrawSurfPanel.Height;
				DrawingBox.Width = (int)(PR * (DrawSurfPanel.Height));
				newLoc.X = DrawSurfPanel.Width / 2 - DrawingBox.Width / 2;
			}
			else
			{
				DrawingBox.Width = DrawSurfPanel.Width;
				DrawingBox.Height = (int)((DrawSurfPanel.Width) / PR);
				newLoc.Y = DrawSurfPanel.Height / 2 - DrawingBox.Height / 2;
			}
			DrawingBox.Location = newLoc;
		}

		private void InitProject(Size projSize)
		{
			if (project != null)
			{
				SaveIntoFile(SavePath);
				LayersTreeView.Nodes.Clear();
			}

			project = new Project(projSize);
			BufferLayer = new Layer(projSize);
			CountDrawingBoxSize();

			AddLayer("Background");
			project.AllLayers[0].Graphics.FillRectangle(new SolidBrush(Color.White), 0, 0, project.Width, project.Height);

			LayersTreeView.SelectedNode = LayersTreeView.Nodes[0];
			AddLayerButton.Enabled = true;
			ToolParametersPanel.Visible = true;
			LayerSizePanel.Visible = true;
			DrawingBox.Refresh();
		}

		private void SaveIntoImage(string format)
		{
			SaveFileDialog d = new SaveFileDialog
			{
				Filter = "Images (*.bmp;*.jpg;*.png,*.tiff)|*.bmp;*.jpg;*.png;*.tiff|" + "All files (*.*)|*.*",
				DefaultExt = format
			};

			if (d.ShowDialog() == DialogResult.OK)
			{
				BufferLayer.Image.Save(d.FileName);

				MessageBox.Show("File created successfully!", "File created");
			}

			d.Dispose();
		}
		private void SavePngButton_Click(object sender, EventArgs e)
		{
			if (project != null)
				SaveIntoImage("png");
		}
		private void SaveJpgButton_Click(object sender, EventArgs e)
		{
			if (project != null)
				SaveIntoImage("jpg");
		}
		private void SaveBmpButton_Click(object sender, EventArgs e)
		{
			if (project != null)
				SaveIntoImage("bmp");
		}
		private void SaveTiffButton_Click(object sender, EventArgs e)
		{
			if (project != null)
				SaveIntoImage("tiff");
		}
		private void dATFileToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (project != null)
				SaveIntoFile(null);
		}
		private void InitProject(Bitmap img)
		{
			InitProject(img.Size);
			project.SelectedLayer.Image = img;
			DrawingBox.Refresh();
		}

		private void SaveIntoFile(string path)
		{
			// Binary formater is used to serialise the project
			BinaryFormatter formatter = new BinaryFormatter();
			// Dialog to get the path to save file
			SaveFileDialog d = new SaveFileDialog
			{
				DefaultExt = "dat"
			};

			if (path == null && d.ShowDialog() == DialogResult.OK)
			{
				path = d.FileName;
				SavePath = path;
			}
			d.Dispose();

			if (path != null)
			{
				try
				{
					using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
					{
						formatter.Serialize(fs, project);
						MessageBox.Show("File saved successfully", "Success!");
					}
				}
				catch (IOException)
				{
					MessageBox.Show("File extention not suitable.", "Error");
				}
				catch (SerializationException)
				{
					MessageBox.Show("An error occured while saving the project.", "Error");
				}
			}
			
		}
		private void SaveFileButton_Click(object sender, EventArgs e)
		{
			if (project != null)
				SaveIntoFile(SavePath);
		}
		private void Courseach_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (project != null && MessageBox.Show("Do you want to save current project?", "Project will be lost", MessageBoxButtons.YesNo) == DialogResult.Yes)
				SaveIntoFile(SavePath);
		}
		private void QuitButton_Click(object sender, EventArgs e)
		{
			Close();
		}
		// ============ Drawing on screen ===============
		private void DrawingBox_Paint(object sender, PaintEventArgs e)
		{
			if (project != null)
			{
				BufferLayer.Graphics.Clear(Color.Transparent);
				project.GetFinalImage(BufferLayer);
				if (BufferLayer.Image != null)
					e.Graphics.DrawImage(BufferLayer.Image, 0, 0, DrawingBox.Width, DrawingBox.Height);
			}
		}
		private void Courseach_Resize(object sender, EventArgs e)
		{
			if (project != null)
			{
				CountDrawingBoxSize();
				DrawingBox.Refresh();
			}
		}

		// ========== Mouse events ============================
		private void DrawingBox_MouseDown(object sender, MouseEventArgs e)
		{
			if (project != null && project.SelectedLayer != null)	// Only do something if there is a layer to work with
			{
				// Don't allow usage of tools while layer modification is in progress, witha an exception of the MoveTool
				if (ModifyCheckBox.Checked && CurUse != ToolUse.Transform)
					MessageBox.Show("You can't use tools while shape modification is in progress.", "Drawing not allowed");
				// Don't allow drawing on layers that contain a shape or text information. Using other tools is fine because they either 
				// don't change the image or create a new layer to work on
				else if (CurUse == ToolUse.Draw && (project.SelectedLayer.Shape != null || project.SelectedLayer.Text != null))
				{
					if (MessageBox.Show("You can't draw on shape layers. in order to do this you need to resterise the layer." +
						" Would you like to resterise this layer? You will no longer be able to change the layer shape after this action.",
						"Drawing not allowed", MessageBoxButtons.YesNo) == DialogResult.Yes)
					{
						// If user wishes to get rid of shape or text arguments and only leave the image
						project.SelectedLayer.Shape = null;
						project.SelectedLayer.Text = null;
						SetShapeBoxes();
					}
				}
				// Else the tool usage is allowed
				else
				{
					DBMouseDown = true;
					Point UsePoint = new Point();
					// Because the drawingBox and project sizes are different there is a need to calculate where the cursor is located
					// relative to the image
					UsePoint.X = e.X * project.Width / DrawingBox.Width;
					UsePoint.Y = e.Y * project.Height / DrawingBox.Height;
					if (CurUse == ToolUse.Draw)
					{
						// Take into account that image could have been moved with MoveTool
						UsePoint.X -= project.SelectedLayer.Location.X;
						UsePoint.Y -= project.SelectedLayer.Location.Y;
					}
					
					if (CurUse == ToolUse.Shape)   // If current tool is a shape
					{
						// Create a new layer to create the shape on and select it
						AddLayer("Layer " + (LayersTreeView.Nodes.Count + 1).ToString());
						LayersTreeView.SelectedNode = LayersTreeView.Nodes[0];
					}
					// Pipette and text tools work differently enought to treat them separately
					if (!(CurrentTool is PipetteTool))
						CurrentTool.InitTool(UsePoint, project.SelectedLayer);
					if (CurrentTool is TextInitTool)
					{
						// Clicking with the tool sets the position that text will be drawn from, there are no other usages
						DBMouseDown = false;
						ModifyCheckBox.Checked = true;
						ModifyCheckBox.Visible = true;
						LayerSizePanel.Visible = false;
						LayerTextBox.Text = "";
						LayerTextBox.Focus();
					}
					else if (CurrentTool is PipetteTool)
					{
						// Pipette is clicked, after what the color it picked up will be transfered to the tool
						// that was selected before the pipette, and that tool is selected
						CurrentTool.InitTool(UsePoint, BufferLayer);
						DBMouseDown = false;
						// There are two pipettes, one for primary color, and other for fill color
						if (CurrentTool == Tools[7])
						{
							SetPrimaryColor(CurrentTool.ToolColor, PrevTool);
							PipetteToolButton.BackColor = Color.Transparent;
						}
						else
						{
							SetFillColor(CurrentTool.ToolColor, PrevTool);
							FillPipetteButton.BackColor = Color.Transparent;
						}
						CurrentTool = PrevTool;
						CurUse = PrevUse;

						LastUsedButton.BackColor = Color.LightBlue;
					}
					DrawingBox.Refresh();
				}
			}
		}

		private void DrawingBox_MouseMove(object sender, MouseEventArgs e)
		{
			if (DBMouseDown)
			{
				Point UsePoint = new Point();
				UsePoint.X = e.X * project.Width / DrawingBox.Width;
				UsePoint.Y = e.Y * project.Height / DrawingBox.Height;
				if (CurUse == ToolUse.Draw)
				{
					UsePoint.X -= project.SelectedLayer.Location.X;
					UsePoint.Y -= project.SelectedLayer.Location.Y;
				}
				CurrentTool.UseTool(UsePoint);
				DrawingBox.Refresh();
			}
		}

		private void DrawingBox_MouseUp(object sender, MouseEventArgs e)
		{
			if (DBMouseDown)
			{
				DBMouseDown = false;
				SetShapeBoxes();
			}
		}
		// ================== Layers controls =========================
		private void AddLayerButton_Click(object sender, EventArgs e)
		{
			AddLayer("Layer " + (LayersTreeView.Nodes.Count + 1).ToString());
		}

		private void AddLayer(string layerName)
		{
			project.AddLayer(layerName);
			// Layers in the project are drawn like this: first index - drawn at the background, last - at the top.
			// Layers in the treeView go in another direction: index 0 in treeView is last index in the project and vice versa.
			// This is made solely for interface clarity, because this way project top layer is located on the top of the treeView.

			ModifyLayers = false;
			LayersTreeView.Nodes.Add(layerName);
			if (LayersTreeView.Nodes.Count > 1)
			{
				string temp;
				bool tempchecked;
				for (int i = LayersTreeView.Nodes.Count - 1; i > 0; i--)
				{
					temp = LayersTreeView.Nodes[i].Text;
					tempchecked = LayersTreeView.Nodes[i].Checked;
					LayersTreeView.Nodes[i].Text = LayersTreeView.Nodes[i - 1].Text;
					LayersTreeView.Nodes[i].Checked = LayersTreeView.Nodes[i - 1].Checked;
					LayersTreeView.Nodes[i - 1].Text = temp;
					LayersTreeView.Nodes[i - 1].Checked = tempchecked;
				}
			}
			ModifyLayers = true;
			LayersTreeView.Nodes[0].Checked = true;

			SelectNode(0);
		}

		private void SelectNode(int index)
		{
			LayersTreeView.SelectedNode = LayersTreeView.Nodes[index];
			project.SelectLayer(LayersTreeView.Nodes.Count - index - 1);

			if (!RemoveLayerButton.Enabled && LayersTreeView.Nodes.Count > 1)
			{
				RemoveLayerButton.Enabled = true;
			}
			else if (RemoveLayerButton.Enabled && LayersTreeView.SelectedNode == null)
				RemoveLayerButton.Enabled = false;

			if (LayersTreeView.SelectedNode.Index != LayersTreeView.Nodes.Count - 1)
				LayerDownButton.Enabled = true;
			else
				LayerDownButton.Enabled = false;
			if (LayersTreeView.SelectedNode.Index != 0)
				LayerUpButton.Enabled = true;
			else
				LayerUpButton.Enabled = false;

			SetShapeBoxes();
			ModifyCheckBox.Checked = false;
		}

		private void RemoveLayerButton_Click(object sender, EventArgs e)
		{
			if (LayersTreeView.Nodes.Count > 1)
			{
				project.RemoveLayer(LayersTreeView.Nodes.Count - LayersTreeView.SelectedNode.Index - 1);
				LayersTreeView.Nodes.Remove(LayersTreeView.SelectedNode);
				project.SelectLayer(LayersTreeView.Nodes.Count - LayersTreeView.SelectedNode.Index - 1);
				if (LayersTreeView.Nodes.Count == 1)
				{
					RemoveLayerButton.Enabled = false;
					LayerUpButton.Enabled = false;
					LayerDownButton.Enabled = false;
				}
				DrawingBox.Refresh();
			}
		}
		private void LayerDownButton_Click(object sender, EventArgs e)
		{
			int selectedIndex = LayersTreeView.SelectedNode.Index;
			string temp = LayersTreeView.Nodes[selectedIndex].Text;
			LayersTreeView.Nodes[selectedIndex].Text = LayersTreeView.Nodes[selectedIndex + 1].Text;
			LayersTreeView.Nodes[selectedIndex + 1].Text = temp;
			int projSelInd = LayersTreeView.Nodes.Count - selectedIndex - 1;
			project.SwapLayers(projSelInd, projSelInd - 1);
			LayersTreeView.SelectedNode = LayersTreeView.Nodes[selectedIndex + 1];

			DrawingBox.Refresh();
		}

		private void LayerUpButton_Click(object sender, EventArgs e)
		{
			int selectedIndex = LayersTreeView.SelectedNode.Index;
			string temp = LayersTreeView.Nodes[selectedIndex].Text;
			LayersTreeView.Nodes[selectedIndex].Text = LayersTreeView.Nodes[selectedIndex - 1].Text;
			LayersTreeView.Nodes[selectedIndex - 1].Text = temp;
			int projSelInd = LayersTreeView.Nodes.Count - selectedIndex - 1;
			project.SwapLayers(projSelInd, projSelInd + 1);
			LayersTreeView.SelectedNode = LayersTreeView.Nodes[selectedIndex - 1];

			DrawingBox.Refresh();
		}
		
		/// <summary>
		/// Selects a layer in the LayersTreeView.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void LayersTreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			SelectNode(e.Node.Index);
		}

		private void SetShapeBoxes()
		{
			if (project.SelectedLayer.Shape != null)    // Is a shape
			{
				LayerSizePanel.Visible = true;
				LayerWidth.Value = project.SelectedLayer.Shape.Width;
				LayerHeight.Value = project.SelectedLayer.Shape.Height;
				ModifyCheckBox.Visible = true;
			}
			else if (project.SelectedLayer.Text != null)	// Is a text layer
			{
				LayerSizePanel.Visible = false;
				ModifyCheckBox.Visible = true;
				LayerTextBox.Text = project.SelectedLayer.Text.Text;
			}
			else    // Is not a shape
			{
				LayerSizePanel.Visible = true;
				LayerWidth.Value = project.SelectedLayer.LayerSize.Width;
				LayerHeight.Value = project.SelectedLayer.LayerSize.Height;
				ModifyCheckBox.Visible = false;
			}
		}

		private void LayersTreeView_AfterCheck(object sender, TreeViewEventArgs e)
		{
			if (ModifyLayers)
			{
				int ind = LayersTreeView.Nodes.Count - e.Node.Index - 1;

				project.AllLayers[ind].Visible = e.Node.Checked;
				project.SelectLayer(project.SelectedLayerIndex);
				DrawingBox.Refresh();
			}
		}

		private void LoadImageButton_Click(object sender, EventArgs e)
		{
			if (project != null)
			{
				OpenFileDialog d = new OpenFileDialog();
				d.Filter = "Images (*.BMP;*.JPG;*.GIF,*.PNG,*.TIFF)|*.BMP;*.JPG;*.GIF;*.PNG;*.TIFF|" + "All files (*.*)|*.*";
				if (d.ShowDialog() == DialogResult.OK)
				{
					try
					{
						Bitmap bmp = new Bitmap(d.FileName);
						AddLayer("Layer " + (LayersTreeView.Nodes.Count + 1).ToString());
						project.SelectLayer(project.AllLayers.Count - 1);
						project.SelectedLayer.Image = bmp;
						DrawingBox.Refresh();
						SetShapeBoxes();
					}
					catch (IOException)
					{
						MessageBox.Show("An error occured while loading the image.", "Error");
					}
				}
				d.Dispose();
				
			}
		}

		// ============== Tools buttons events ======================================
		private void SetToolInfo()
		{
			ColorButton.BackColor = CurrentTool.ToolColor;
			WidthBar.Value = CurrentTool.ToolWidth;
			AlphaBox.Value = CurrentTool.ToolColor.A;

		}

		void SetFillColorButtonVisible()
		{
			if (CurrentTool == Tools[3] || CurrentTool == Tools[4])
			{
				FillColorButton.Visible = true;
				FillPipetteButton.Visible = true;
				if (CurrentTool.FillColor.A != 0)
					FillAlphaPanel.Visible = true;
			}
			else
			{
				FillColorButton.Visible = false;
				FillPipetteButton.Visible = false;
				FillAlphaPanel.Visible = false;
			}
		}

		private void BrushButton_Click(object sender, EventArgs e)
		{
			if (!ModifyCheckBox.Checked && project != null)
			{
				project.Compositing = CompositingMode.SourceOver;
				CurrentTool = Tools[0];
				SetToolInfo();
				CurUse = ToolUse.Draw;

				LastUsedButton.BackColor = Color.Transparent;
				BrushButton.BackColor = Color.LightBlue;
				LastUsedButton = BrushButton;
				SetFillColorButtonVisible();
			}
		}

		private void EraserButton_Click(object sender, EventArgs e)
		{
			if (!ModifyCheckBox.Checked && project != null)
			{
				project.Compositing = CompositingMode.SourceCopy;
				CurrentTool = Tools[1];
				WidthBar.Value = CurrentTool.ToolWidth;
				CurUse = ToolUse.Draw;

				LastUsedButton.BackColor = Color.Transparent;
				EraserButton.BackColor = Color.LightBlue;
				LastUsedButton = EraserButton;
				SetFillColorButtonVisible();
			}
		}

		void SetPrimaryColor(Color color, ToolBase tool)
		{
			if (ModifyCheckBox.Checked)
			{
				if (project.SelectedLayer.Shape != null)
					project.SelectedLayer.Shape.Color = color;
				else
					project.SelectedLayer.Text.Color = color;
				project.SelectedLayer.RefreshContents();
				DrawingBox.Refresh();
			}
			else
			{
				tool.ToolColor = color;
			}

			AlphaBox.Value = color.A;
			ColorButton.BackColor = color;
		}

		private void ColorButton_Click(object sender, EventArgs e)
		{
			if (project != null)
			{
				if ((project.Compositing == CompositingMode.SourceOver && CurUse != ToolUse.Transform) || (CurUse == ToolUse.Transform && ModifyCheckBox.Checked && project != null))
				{
					ColorDialog d = new ColorDialog();
					if (d.ShowDialog() == DialogResult.OK)
					{
						SetPrimaryColor(d.Color, CurrentTool);
					}
					d.Dispose();
				}
			}
		}

		void SetFillColor(Color color, ToolBase tool)
		{
			if (ModifyCheckBox.Checked)
			{
				project.SelectedLayer.Shape.FillColor = color;
				project.SelectedLayer.RefreshContents();
				DrawingBox.Refresh();
				FillAlphaBox.Value = project.SelectedLayer.Shape.FillColor.A;
			}
			else
			{
				tool.FillColor = color;
				FillAlphaBox.Value = CurrentTool.FillColor.A;
			}

			FillColorButton.BackColor = color;
			FillAlphaPanel.Visible = true;
		}
		private void FillColorButton_Click(object sender, EventArgs e)
		{
			if (project != null)
			{
				ColorDialog d = new ColorDialog();
				if (d.ShowDialog() == DialogResult.OK)
				{
					SetFillColor(d.Color, CurrentTool);

				}
				d.Dispose();
			}
		}
		// Shapes
		private void ShapeToolsButton_ButtonClick(object sender, EventArgs e)
		{
			if (!ModifyCheckBox.Checked && project != null)
			{
				CurrentTool = LastShapeTool;
				CurUse = ToolUse.Shape;
				project.Compositing = CompositingMode.SourceOver;
				SetToolInfo();

				LastUsedButton.BackColor = Color.Transparent;
				ShapeToolsButton.BackColor = Color.LightBlue;
				LastUsedButton = ShapeToolsButton;
				SetFillColorButtonVisible();
			}
		}

		private void lineToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (!ModifyCheckBox.Checked && project != null)
			{
				project.Compositing = System.Drawing.Drawing2D.CompositingMode.SourceOver;
				CurrentTool = Tools[2];
				SetToolInfo();
				LastShapeTool = Tools[2];
				ShapeToolsButton.Image = ShapeToolsButton.DropDownItems[0].Image;
				CurUse = ToolUse.Shape;

				LastUsedButton.BackColor = Color.Transparent;
				ShapeToolsButton.BackColor = Color.LightBlue;
				LastUsedButton = ShapeToolsButton;
				SetFillColorButtonVisible();
			}
		}

		private void RectangleButton_Click(object sender, EventArgs e)
		{
			if (!ModifyCheckBox.Checked && project != null)
			{
				project.Compositing = CompositingMode.SourceOver;
				CurrentTool = Tools[3];
				SetToolInfo();
				LastShapeTool = Tools[3];
				ShapeToolsButton.Image = ShapeToolsButton.DropDownItems[1].Image;
				CurUse = ToolUse.Shape;

				LastUsedButton.BackColor = Color.Transparent;
				ShapeToolsButton.BackColor = Color.LightBlue;
				LastUsedButton = ShapeToolsButton;
				SetFillColorButtonVisible();
			}
		}
		private void EllipseButton_Click(object sender, EventArgs e)
		{
			if (!ModifyCheckBox.Checked && project != null)
			{
				project.Compositing = CompositingMode.SourceOver;
				CurrentTool = Tools[4];
				SetToolInfo();
				LastShapeTool = Tools[4];
				ShapeToolsButton.Image = ShapeToolsButton.DropDownItems[2].Image;
				CurUse = ToolUse.Shape;

				LastUsedButton.BackColor = Color.Transparent;
				ShapeToolsButton.BackColor = Color.LightBlue;
				LastUsedButton = ShapeToolsButton;
				SetFillColorButtonVisible();
			}
		}
		private void MoveToolButton_Click(object sender, EventArgs e)
		{
			if (project != null)
			{
				project.Compositing = CompositingMode.SourceOver;
				CurrentTool = Tools[5];
				//WidthBar.Value = CurrentTool.ToolWidth;
				//ColorButton.BackColor = CurrentTool.ToolColor;
				CurUse = ToolUse.Transform;

				LastUsedButton.BackColor = Color.Transparent;
				MoveToolButton.BackColor = Color.LightBlue;
				LastUsedButton = MoveToolButton;
				SetFillColorButtonVisible();
			}
		}
		private void TextButton_Click(object sender, EventArgs e)
		{
			if (!ModifyCheckBox.Checked && project != null)
			{
				project.Compositing = CompositingMode.SourceOver;
				CurrentTool = Tools[6];
				SetToolInfo();
				CurUse = ToolUse.Shape;

				LastUsedButton.BackColor = Color.Transparent;
				TextButton.BackColor = Color.LightBlue;
				LastUsedButton = TextButton;
				SetFillColorButtonVisible();
			}
		}

		private void PipetteToolButton_Click(object sender, EventArgs e)
		{
			if ((CurUse == ToolUse.Shape || CurrentTool == Tools[0] || ModifyCheckBox.Checked) && project != null)
			{
				PrevTool = CurrentTool;
				CurrentTool = Tools[7];
				PrevUse = CurUse;
				CurUse = ToolUse.Transform;

				LastUsedButton.BackColor = Color.Transparent;
				PipetteToolButton.BackColor = Color.LightBlue;
				//LastUsedButton = PipetteToolButton;
				SetFillColorButtonVisible();
			}
		}

		private void FillPipetteButton_Click(object sender, EventArgs e)
		{
			if ((CurUse == ToolUse.Shape || CurrentTool == Tools[0] || ModifyCheckBox.Checked) && project != null)
			{
				PrevTool = CurrentTool;
				CurrentTool = Tools[8];
				PrevUse = CurUse;
				CurUse = ToolUse.Transform;

				LastUsedButton.BackColor = Color.Transparent;
				FillPipetteButton.BackColor = Color.LightBlue;
			}
		}

		// ============================== Parameters buttons =======================
		private void ResizeProjButton_Click(object sender, EventArgs e)
		{
			ProjectCreationWindow d = new ProjectCreationWindow();

			if (d.ShowDialog() == DialogResult.OK) 
			{
				project.Size = d.NewProjectSize;
				DrawingBox.Refresh();
				SetShapeBoxes();
				CountDrawingBoxSize();
				BufferLayer.LayerSize = d.NewProjectSize;
			}

			d.Dispose();
		}

		private void WidthBar_ValueChanged(object sender, EventArgs e)
		{
			if (ModifyCheckBox.Checked)
			{
				if (project.SelectedLayer.Shape != null)
				{
					project.SelectedLayer.Shape.ToolWidth = WidthBar.Value;
				}
				else if (project.SelectedLayer.Text != null)
					project.SelectedLayer.Text.Size = WidthBar.Value;
				project.SelectedLayer.RefreshContents();
				DrawingBox.Refresh();
			}
			else
				CurrentTool.ToolWidth = WidthBar.Value;
			ToolWidthBox.Value = WidthBar.Value;
		}

		private void ToolWidthBox_ValueChanged(object sender, EventArgs e)
		{
			if (ToolWidthBox.Value != WidthBar.Value)
				WidthBar.Value = (int)ToolWidthBox.Value;
		}
		private void AlphaBox_ValueChanged(object sender, EventArgs e)
		{
			Color temp;
			if (ModifyCheckBox.Checked)
			{
				if (project.SelectedLayer.Shape != null)
				{
					temp = Color.FromArgb((int)AlphaBox.Value, project.SelectedLayer.Shape.Color);
					project.SelectedLayer.Shape.Color = temp;
				}
				else
				{
					temp = Color.FromArgb((int)AlphaBox.Value, project.SelectedLayer.Text.Color);
					project.SelectedLayer.Text.Color = temp;
				}
					project.SelectedLayer.RefreshContents();
				DrawingBox.Refresh();
			}
			else
			{
				temp = Color.FromArgb((int)AlphaBox.Value, CurrentTool.ToolColor);
				CurrentTool.ToolColor = temp;
			}
			
			ColorButton.BackColor = temp;
		}

		private void FillAlphaBox_ValueChanged(object sender, EventArgs e)
		{
			if (FillAlphaBox.Value == 0)
			{
				FillAlphaPanel.Visible = false;
				FillColorButton.BackColor = Color.Transparent;
			}
			else
			{
				Color temp;
				if (ModifyCheckBox.Checked)
				{
					temp = Color.FromArgb((int)FillAlphaBox.Value, project.SelectedLayer.Shape.FillColor);
					project.SelectedLayer.Shape.FillColor = temp;
					project.SelectedLayer.RefreshContents();
					DrawingBox.Refresh();
				}
				else
				{
					temp = Color.FromArgb((int)FillAlphaBox.Value, CurrentTool.FillColor);
					CurrentTool.FillColor = temp;
				}

				FillColorButton.BackColor = temp;
			}
		}

		private void LayerWidth_ValueChanged(object sender, EventArgs e)
		{
			if (project.SelectedLayer.Shape != null)
			{
				if (project.SelectedLayer.Shape.Width != (int)LayerWidth.Value)
				{
					project.SelectedLayer.Shape.Width = (int)LayerWidth.Value;
					project.SelectedLayer.RefreshContents();
				}
			}
			else if (project.SelectedLayer.Width != (int)LayerWidth.Value)
				project.SelectedLayer.Width = (int)LayerWidth.Value;
			DrawingBox.Refresh();
		}

		private void LayerHeight_ValueChanged(object sender, EventArgs e)
		{
			if (project.SelectedLayer.Shape != null && project.SelectedLayer.Shape.Height != (int)LayerWidth.Value)
			{
				if (project.SelectedLayer.Shape.Height != (int)LayerWidth.Value)
				{
					project.SelectedLayer.Shape.Height = (int)LayerHeight.Value;
					project.SelectedLayer.RefreshContents();
				}
			}
			else if (project.SelectedLayer.Height != (int)LayerHeight.Value)
				project.SelectedLayer.Height = (int)LayerHeight.Value;
			DrawingBox.Refresh();
		}

		/// <summary>
		/// When checked - all boxes contain info about the existing shape.
		/// When unchecked - all boxed contain info about current tool.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ModifyCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			if (ModifyCheckBox.Checked)
			{
				if (project.SelectedLayer.Shape != null)
				{
					ColorButton.BackColor = project.SelectedLayer.Shape.Color;
					FillColorButton.BackColor = project.SelectedLayer.Shape.FillColor;
					AlphaBox.Value = project.SelectedLayer.Shape.Color.A;
					FillAlphaBox.Value = project.SelectedLayer.Shape.FillColor.A;
					WidthBar.Value = project.SelectedLayer.Shape.ToolWidth;
					if (project.SelectedLayer.Shape.Tool.GetType() != typeof(LineTool))
					{
						FillColorButton.Visible = true;
						FillPipetteButton.Visible = true;
						if (project.SelectedLayer.Shape.FillColor.A != 0)
							FillAlphaPanel.Visible = true;
					}
				}
				else if (project.SelectedLayer.Text != null)
				{
					ColorButton.BackColor = project.SelectedLayer.Text.Color;
					AlphaBox.Value = project.SelectedLayer.Text.Color.A;
					WidthBar.Value = (int)project.SelectedLayer.Text.Size;

					TextGB.Visible = true;
				}
			}
			else
			{
				ColorButton.BackColor = CurrentTool.ToolColor;
				FillColorButton.BackColor = CurrentTool.FillColor;
				AlphaBox.Value = CurrentTool.ToolColor.A;
				FillAlphaBox.Value = CurrentTool.FillColor.A;
				WidthBar.Value = CurrentTool.ToolWidth;
				ToolWidthBox.Value = CurrentTool.ToolWidth;
				TextGB.Visible = false;
			}
		}

		private void FillColorButton_VisibleChanged(object sender, EventArgs e)
		{
			if (ModifyCheckBox.Checked)
			{
				FillColorButton.Visible = true;
				FillPipetteButton.Visible = true;
			}
		}

		private void FillPipetteButton_VisibleChanged(object sender, EventArgs e)
		{
			if (ModifyCheckBox.Checked)
			{
				FillColorButton.Visible = true;
				FillPipetteButton.Visible = true;
			}
		}

		private void LayerTextBox_TextChanged(object sender, EventArgs e)
		{
			project.SelectedLayer.Text.Text = LayerTextBox.Text;
			project.SelectedLayer.RefreshContents();
			DrawingBox.Refresh();
		}

		private void FontSelectionButton_Click(object sender, EventArgs e)
		{
			FontDialog d = new FontDialog();
			TextArgs temp = project.SelectedLayer.Text;
			d.Font = temp.Font;
			d.Color = temp.Color;

			if (d.ShowDialog() == DialogResult.OK)
			{
				project.SelectedLayer.Text.Font = d.Font;
				project.SelectedLayer.RefreshContents();
				DrawingBox.Refresh();
			}
			d.Dispose();
		}

		







		// ===========================

	}
}
