using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace dotnetpaint
{
	/// <summary>
	/// Перелічення, що містить усі можливі використання інструментів програми.
	/// </summary>
	public enum ToolUse { Draw, Shape, Transform };

	/// <summary>
	/// Головне вікно програми.
	/// </summary>
	public partial class Courseach : Form
	{
		/// Мітка, яка позначає, що кнопка мишки натиснута
		private bool DBMouseDown;
		/// Мітка, яка позначає, що шари можна модифікувати
		private bool ModifyLayers;
		/// Перелічення, яке позначає, яку функцію виконує поточний інструмент
		private ToolUse CurUse;
		/// Перелічення, яке позначає, яку функцію виконував попередній інструмент
		private ToolUse PrevUse;
		/// Буфер зображення, щоб при оновленні поверхні малювання не було підмигувань
		private Layer BufferLayer;
		/// Поточний проект
		private Project project;
		/// Усі доступні інструменти
		private ToolBase[] Tools;
		/// Поточний інструмент
		private ToolBase CurrentTool;
		/// Останній використаний інструмент для малювання фігури
		private ToolBase LastShapeTool;
		/// Попередній інструмент
		private ToolBase PrevTool;
		/// Кнопка, яка відповідає за останній обраний інструмент
		private ToolStripItem LastUsedButton;
		/// Шлях для збереження проекту у dat файл
		private string SavePath;
		/// <summary>
		/// Конструктор, який присвоює атрибутам деякі значення за замовчуванням.
		/// </summary>
		public Courseach()
		{
			InitializeComponent();
			DBMouseDown = false;
			ModifyLayers = true;

			Tools = new ToolBase[9];
			Tools[0] = new BrushTool();                     // Пензлик
			Tools[1] = new BrushTool(Color.Transparent, 5); // Стирачка - пензлик, що малює прозорим кольором із заміною поточного кольору.
			Tools[2] = new LineTool();                      // Інструмент лінії
			Tools[3] = new RectangleTool();                 // Інструмент прямокутника
			Tools[4] = new EllipseTool();                   // Інструмент еліпса
			Tools[5] = new MoveTool();                      // Інструмент для переміщення шару
			Tools[6] = new TextInitTool();                  // Інструмент ініціалізації тексту
			Tools[7] = new PipetteTool();                   // Піпетка для визначення основного кольору
			Tools[8] = new PipetteTool();                   // Піпетка для визначення кольору заливки фігури

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

		// ========== Завантаження та збереження проекту ========================
		/// <summary>
		/// Подія кнопки, що відповідає за створення пустого проекту.
		/// </summary>
		private void BlankProjButton_Click(object sender, EventArgs e)
		{
			// Діалог для введення розміру проекту
			ProjectCreationWindow d = new ProjectCreationWindow();
			if (d.ShowDialog() == DialogResult.OK)
			{
				InitProject(d.NewProjectSize);
			}
			d.Dispose();
		}
		/// <summary>
		/// Подія кнопки, що відповідає за створення нового проекту на основі зображення з пристрою.
		/// </summary>
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
				catch (ArgumentException)
				{
					MessageBox.Show("File you've selected isn't an image.", "Error");
				}
			}

			d.Dispose();
		}
		/// <summary>
		/// Подія кнопки, що відповідає за завантаження існуючого проекту із файлу на пристрої.
		/// </summary>
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
						// Якщо в програмі уже відкритий проект його варто зберегти.
						Project temp = (Project)formatter.Deserialize(fs);
						if (project != null && MessageBox.Show("Do you want to save current project?", "Project will be lost", MessageBoxButtons.YesNo) == DialogResult.Yes)
							SaveIntoFile(SavePath);
						project = temp;
						project.SelectLayer(project.Layers.Count - 1);
						SavePath = d.FileName;
						LayersTreeView.Nodes.Clear();	// Очищення елементів форми, які відображають шари попереднього проекту
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
		/// <summary>
		/// Відповідає за підготовку усіх потрібних для роботи елементів програми, з врахунком на те, що проект уже завантажений у програму із файлу.
		/// </summary>
		private void InitProject()
		{
			BufferLayer = new Layer(project.Size);
			DrawingBox.Location = Point.Empty;
			CountDrawingBoxSize();

			// Додавання усіх шарів проекту в елемент форми, який їх відображає
			ModifyLayers = false;	// Тимчасово відключити модифікацію шарів
			foreach (Layer l in project.Layers)
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
		/// <summary>
		/// Підраховує розмір та пропорції робочої поверхні на основі розмірів проекту.
		/// </summary>
		private void CountDrawingBoxSize()
		{
			Point newLoc = Point.Empty;
			// Відношення ширини проекту до його висоти.
			float PR = (float)project.Width / project.Height;
			// Відношення ширини робочої поверхні до її висоти.
			float DR = (float)DrawSurfPanel.Width / DrawSurfPanel.Height;
			// Залежно від значень цих відношень розмір робочої поверхні потрібно маштабувати по різному, щоб вона повністю помістилась на екрані.
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
		/// <summary>
		/// Відповідає за підготовку усіх потрібних для роботи елементів програми, створює проект.
		/// </summary>
		/// <param name="projSize">Розмір нового проекту.</param>
		private void InitProject(Size projSize)
		{
			// Якщо в програмі уже відкритий проект - його варто зберегти.
			if (project != null)
			{
				SaveIntoFile(SavePath);
				LayersTreeView.Nodes.Clear();
			}
			// Створення нового проекту, підготовка буфера та робочої поверхні
			project = new Project(projSize);
			BufferLayer = new Layer(projSize);
			CountDrawingBoxSize();
			// Додавання початкового білого шару
			AddLayer("Background");
			project.Layers[0].Graphics.FillRectangle(new SolidBrush(Color.White), 0, 0, project.Width, project.Height);
			// Вибір цього шару та активація елементів інтерфейсу для роботи
			LayersTreeView.SelectedNode = LayersTreeView.Nodes[0];
			AddLayerButton.Enabled = true;
			ToolParametersPanel.Visible = true;
			LayerSizePanel.Visible = true;
			DrawingBox.Refresh();
		}
		/// <summary>
		/// Відповідає за підготовку усіх потрібних для роботи елементів програми, створює проект на основі зображення.
		/// </summary>
		/// <param name="img">Зображення, яке стане основою проекту.</param>
		private void InitProject(Bitmap img)
		{
			InitProject(img.Size);
			project.SelectedLayer.Image = img;
			DrawingBox.Refresh();
		}
		/// <summary>
		/// Збереження проекту у файл зі розширенням зображення.
		/// </summary>
		/// <param name="format">Бажаний формат збереженого зображення, однак якщо в діалозі ввести назву файлу із іншим розширенням, буде використане воно.</param>
		private void SaveIntoImage(string format)
		{
			SaveFileDialog d = new SaveFileDialog
			{
				Filter = "Images (*.bmp;*.jpg;*.png,*.tiff)|*.bmp;*.jpg;*.png;*.tiff|" + "All files (*.*)|*.*",
				DefaultExt = format
			};

			try
			{
				if (d.ShowDialog() == DialogResult.OK)
				{
					BufferLayer.Image.Save(d.FileName);
					MessageBox.Show("File created successfully!", "File created");
				}
			}
			catch (Exception)
			{
				MessageBox.Show("Something went wrong while saving the project.", "Error");
			}

			d.Dispose();
		}
		/// <summary>
		/// Подія кнопки, яка відповідає за збереження проекту в зображення формату PNG.
		/// </summary>
		private void SavePngButton_Click(object sender, EventArgs e)
		{
			if (project != null)
				SaveIntoImage("png");
		}
		/// <summary>
		/// Подія кнопки, яка відповідає за збереження проекту в зображення формату JPG.
		/// </summary>
		private void SaveJpgButton_Click(object sender, EventArgs e)
		{
			if (project != null)
				SaveIntoImage("jpg");
		}
		/// <summary>
		/// Подія кнопки, яка відповідає за збереження проекту в зображення формату BMP.
		/// </summary>
		private void SaveBmpButton_Click(object sender, EventArgs e)
		{
			if (project != null)
				SaveIntoImage("bmp");
		}
		/// <summary>
		/// Подія кнопки, яка відповідає за збереження проекту в зображення формату TIFF.
		/// </summary>
		private void SaveTiffButton_Click(object sender, EventArgs e)
		{
			if (project != null)
				SaveIntoImage("tiff");
		}
		/// <summary>
		/// Подія кнопки, яка відповідає за збереження проекту у файл із розширенням DAT.
		/// </summary>
		private void dATFileToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (project != null)
				SaveIntoFile(null);
		}
		/// <summary>
		/// Відповідає за збереження проекту у файл із розширенням DAT.
		/// </summary>
		/// <param name="path">Шлях для збереження файлу.</param>
		private void SaveIntoFile(string path)
		{
			// BinaryFormatter використовується для серіалізації проекту
			BinaryFormatter formatter = new BinaryFormatter();
			// Діалог щоб отримати шлях до нього
			SaveFileDialog d = new SaveFileDialog
			{
				DefaultExt = "dat"
			};
			// Якщо переданий у метод шлях не існує - визначимо його тут.
			if (path == null && d.ShowDialog() == DialogResult.OK)
			{
				path = d.FileName;
				// Та збережемо в атрибут, який зберігає шлях до поточного проекту.
				SavePath = path;
			}
			d.Dispose();
			// Якщо користувач все таки хоче зберегти проект
			if (path != null)
			{
				try
				{
					// FileStream використовується для запису серіалізованого проекту у файл
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
		/// <summary>
		/// Подія кнопки, яка відповідає за швидке збереження проекту у відомий шлях його розташування на пристрої. Якщо шлях не відомий - користувач зможе його вказати.
		/// </summary>
		private void SaveFileButton_Click(object sender, EventArgs e)
		{
			if (project != null)
				SaveIntoFile(SavePath);
		}
		/// <summary>
		/// Подія, яка викликається при закритті програми. Запитує користувача, чи хоче він зберегти проект у файл.
		/// </summary>
		private void Courseach_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (project != null && MessageBox.Show("Do you want to save current project?", "Project will be lost", MessageBoxButtons.YesNo) == DialogResult.Yes)
				SaveIntoFile(SavePath);
		}
		/// <summary>
		/// Подія кнопки, яка закриває програму.
		/// </summary>
		private void QuitButton_Click(object sender, EventArgs e)
		{
			Close();
		}
		// ============ Малювання на екран ===============
		/// <summary>
		/// Подія, яка відповіда за перемальовку робочої поверхні.
		/// </summary>
		private void DrawingBox_Paint(object sender, PaintEventArgs e)
		{
			if (project != null)
			{
				// Очистити поточне зображення, щоб е було накладання
				BufferLayer.Graphics.Clear(Color.Transparent);
				project.GetFinalImage(BufferLayer);
				if (BufferLayer.Image != null)
					e.Graphics.DrawImage(BufferLayer.Image, 0, 0, DrawingBox.Width, DrawingBox.Height);
			}
		}
		/// <summary>
		/// Подія, яка викликається при зміні розміру програми. При виклику змінює розмір робочої поверхні, щоб вона завжди займала максимально багато місця.
		/// </summary>
		private void Courseach_Resize(object sender, EventArgs e)
		{
			if (project != null)
			{
				CountDrawingBoxSize();
				DrawingBox.Refresh();
			}
		}
		// ========== Події мишки, потрібні для малювання ============================
		/// <summary>
		/// Подія, що викликається при натиску кнопки мишки на робочй поверхні.
		/// </summary>
		private void DrawingBox_MouseDown(object sender, MouseEventArgs e)
		{
			if (project != null && project.SelectedLayer != null)	// Тільки здійснювати дії коли в програмі відкритий проект
			{
				// Не дозволяти використання інструментів поки відбувається модифікація вмісту шару, за вийнятком інструменту переміщення шару.
				if (ModifyCheckBox.Checked && CurUse != ToolUse.Transform)
					MessageBox.Show("You can't use tools while shape modification is in progress.", "Drawing not allowed");
				/* Малювання на шарах, які містять дані про текст або фігуру, заборонено. Використання інших інструментів дозволене,
				 бо вони або не змінюють зображення шару, або створюють новий шар для роботи.*/
				else if (CurUse == ToolUse.Draw && (project.SelectedLayer.Shape != null || project.SelectedLayer.Text != null))
				{
					if (MessageBox.Show("You can't draw on shape layers. in order to do this you need to resterise the layer." +
						" Would you like to resterise this layer? You will no longer be able to change the layer shape after this action.",
						"Drawing not allowed", MessageBoxButtons.YesNo) == DialogResult.Yes)
					{
						// Якщо користувач хоче позбутись від даних фігури або тексту, що містяться в шарі, та залишити тільки зображення.
						// Така дія дозваоляє малювати на цьому шарі.
						project.SelectedLayer.Shape = null;
						project.SelectedLayer.Text = null;
						SetDimentionsBoxes();
					}
				}
				// В іншому випадку використання любих інструментів дозволене
				else
				{
					DBMouseDown = true;
					Point UsePoint = new Point();
					// Оскільки розміри робочої поверхні та проекту відрізняються, передача координат поверхні для роботи з шаром напряму не буде відображати позицію курсову на зображенні.
					// Тому потрібно визначити, в яких координатах на зображенні знаходиться курсор.
					UsePoint.X = e.X * project.Width / DrawingBox.Width;
					UsePoint.Y = e.Y * project.Height / DrawingBox.Height;
					if (CurUse == ToolUse.Draw)
					{
						// Можливий випадок, коли зображення було переміщене.
						UsePoint.X -= project.SelectedLayer.Location.X;
						UsePoint.Y -= project.SelectedLayer.Location.Y;
					}
					
					if (CurUse == ToolUse.Shape)   // Якщо поточний інструмент відповідає за роботу із фігурою.
					{
						// Створити новий шар для цієї фігури (текст, доданий із допомогою інструменту ініціалізації тесту, вважається фігурою).
						AddLayer("Layer " + (LayersTreeView.Nodes.Count + 1).ToString());
						LayersTreeView.SelectedNode = LayersTreeView.Nodes[0];
					}
					// Піпетка та інструмент для тексту відрізняються своєю роботою від інших, тому їх потрібно використовувати по іншому.
					if (!(CurrentTool is PipetteTool))
						CurrentTool.InitTool(UsePoint, project.SelectedLayer);	// Ініціалізація інструменту
					if (CurrentTool is TextInitTool)
					{
						// Використання цього інструменту вказує на місце, звідки буде малюватись текст.
						DBMouseDown = false;
						ModifyCheckBox.Checked = true;
						ModifyCheckBox.Visible = true;
						LayerSizePanel.Visible = false;
						LayerTextBox.Text = "";
						LayerTextBox.Focus();
					}
					else if (CurrentTool is PipetteTool)
					{
						// При використанні, піпетка обере колір із зображення, присвоїть його попередньому інструменту, та цей інструмент стане поточним.
						CurrentTool.InitTool(UsePoint, BufferLayer);
						DBMouseDown = false;
						// Є дві піпетки: одна для основного кольору, та одна для кольору заливки фігури.
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
		/// <summary>
		/// Подія, що викликається при руху мишки по робочій поверхні. Реалізує використання інструменту.
		/// </summary>
		private void DrawingBox_MouseMove(object sender, MouseEventArgs e)
		{
			// Тільки використовувати інструмент, якщо кнопка мишки натиснута.
			if (DBMouseDown)
			{
				// Підрахунок позиції курсору на зображенні
				Point UsePoint = new Point();
				UsePoint.X = e.X * project.Width / DrawingBox.Width;
				UsePoint.Y = e.Y * project.Height / DrawingBox.Height;
				if (CurUse == ToolUse.Draw)
				{
					UsePoint.X -= project.SelectedLayer.Location.X;
					UsePoint.Y -= project.SelectedLayer.Location.Y;
				}
				// Використання інструменту
				CurrentTool.UseTool(UsePoint);
				DrawingBox.Refresh();
			}
		}
		/// <summary>
		/// Подія, що викликається, коли кнопка мишки перестає бути натиснутою.
		/// </summary>
		private void DrawingBox_MouseUp(object sender, MouseEventArgs e)
		{
			if (DBMouseDown)
			{
				DBMouseDown = false;
				SetDimentionsBoxes();
			}
		}
		// ================== Робота з шарами =========================
		/// <summary>
		/// Подія кнопки, що відповідає за додавання новго шару в проект.
		/// </summary>
		private void AddLayerButton_Click(object sender, EventArgs e)
		{
			AddLayer("Layer " + (LayersTreeView.Nodes.Count + 1).ToString());
		}
		/// <summary>
		/// Відповідає за додавання нового шару в проект. Новий шар додається над всіма іншими.
		/// </summary>
		/// <param name="layerName">Назва нового шару.</param>
		private void AddLayer(string layerName)
		{
			project.AddLayer(layerName);
			// Шари зберігаються в послідовному списку, та малюються в порядку, де перший індекс (0) це самий нижній шар, а останній - самий верхній.
			// Індекси цих шарівв, щовідображаються користувачу, обернені, тобто 0 шар буде верхнім, а останній у списку нижнім.
			// Це потрібно тільки для зручності та наглядності користувача, бо в такому випадку в LayersTreeView шари будуть відображтись в тому ж порядку, в якому малюються.

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
			// Обрати новододаний шар.
			SelectNode(0);
		}
		/// <summary>
		/// Відповідає за вибір робочго шару в LayersTreeView та проекті.
		/// </summary>
		/// <param name="index">Індекс бажаного шару.</param>
		private void SelectNode(int index)
		{
			LayersTreeView.SelectedNode = LayersTreeView.Nodes[index];
			// Індекс цього шару в об'єкті проекту обернений.
			project.SelectLayer(LayersTreeView.Nodes.Count - index - 1);
			// Визначення позиції шару відносно інших шарів, та вирішення які дії над шарами повинні бути доступні.
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
			// Винесення розмірів шару на екран.
			SetDimentionsBoxes();
			ModifyCheckBox.Checked = false;
		}
		/// <summary>
		/// Подія кнопки, яка відповідає за видалення виділеного шару.
		/// </summary>
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
		/// <summary>
		/// Подія кнопки, яка відповідає за перенесення шару під нижній шар.
		/// </summary>
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
		/// <summary>
		/// Подія кнопки, яка відповідає за перенесення шару над верхній шар.
		/// </summary>
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
		/// Подія LayersTreeView, яка відповідає за обрання робочого шару.
		/// </summary>
		private void LayersTreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			SelectNode(e.Node.Index);
		}
		/// <summary>
		/// Відповідає за винесення розмірів шару на екран. Якщо поточний шар містить в собі інформацію про фігуру - будуть винесені її розміри.
		/// </summary>
		private void SetDimentionsBoxes()
		{
			if (project.SelectedLayer.Shape != null)    // Якщо шар містить фігуру
			{
				// Винести її розміри
				LayerSizePanel.Visible = true;
				LayerWidth.Value = project.SelectedLayer.Shape.Width;
				LayerHeight.Value = project.SelectedLayer.Shape.Height;
				ModifyCheckBox.Visible = true;
			}
			else if (project.SelectedLayer.Text != null)	// якщо шар містить текст
			{
				// Сховати панель з розмірами шару
				LayerSizePanel.Visible = false;
				ModifyCheckBox.Visible = true;
				LayerTextBox.Text = project.SelectedLayer.Text.Text;
			}
			else    // Якщо це просто шар із зображенням
			{
				// Винести його розміри
				LayerSizePanel.Visible = true;
				LayerWidth.Value = project.SelectedLayer.LayerSize.Width;
				LayerHeight.Value = project.SelectedLayer.LayerSize.Height;
				ModifyCheckBox.Visible = false;
			}
		}
		/// <summary>
		/// Подія LayersTreeView, яка вмикає або вимикає видимість шарів.
		/// </summary>
		private void LayersTreeView_AfterCheck(object sender, TreeViewEventArgs e)
		{
			if (ModifyLayers)
			{
				int ind = LayersTreeView.Nodes.Count - e.Node.Index - 1;

				project.Layers[ind].Visible = e.Node.Checked;
				project.SelectLayer(project.SelectedLayerIndex);
				DrawingBox.Refresh();
			}
		}
		/// <summary>
		/// Подія кнопки, яка відповідає за додання нового шару із зображенням з пристрою.
		/// </summary>
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
						project.SelectLayer(project.Layers.Count - 1);
						project.SelectedLayer.Image = bmp;
						DrawingBox.Refresh();
						SetDimentionsBoxes();
					}
					catch (IOException)
					{
						MessageBox.Show("An error occured while loading the image.", "Error");
					}
				}
				d.Dispose();
				
			}
		}
		/// <summary>
		/// Подія LayersTreeView, що відповідає за зміну назви шару в проекті після зміни назви поля на фаормі із його назвою.
		/// </summary>
		private void LayersTreeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
		{
			project.Layers[LayersTreeView.Nodes.Count - e.Node.Index - 1].Name = e.Label;
		}
		// ============== Події кнопок інструментів ======================================
		/// <summary>
		/// Відповідає за винесення основної інформації про інструмент на екран.
		/// </summary>
		private void SetToolInfo()
		{
			ColorButton.BackColor = CurrentTool.ToolColor;
			WidthBar.Value = CurrentTool.ToolWidth;
			AlphaBox.Value = CurrentTool.ToolColor.A;
		}
		/// <summary>
		/// Визначає, чи потрібно робити видимими елементи управління кольором заливки.
		/// </summary>
		void SetFillColorButtonVisible()
		{
			// Тільки робити їх видимими коли іде робота із прямокутником або еліпсом.
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
		/// <summary>
		/// Подія кнопки, що відповідає за вибір інструменту пензлика.
		/// </summary>
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
		/// <summary>
		/// Подія кнопки, що відповідає за вибір інструменту стирачки.
		/// </summary>
		private void EraserButton_Click(object sender, EventArgs e)
		{
			if (!ModifyCheckBox.Checked && project != null)
			{
				project.Compositing = CompositingMode.SourceCopy;
				CurrentTool = Tools[1];
				CurrentTool.ToolColor = Color.Transparent;
				WidthBar.Value = CurrentTool.ToolWidth;
				CurUse = ToolUse.Draw;

				LastUsedButton.BackColor = Color.Transparent;
				EraserButton.BackColor = Color.LightBlue;
				LastUsedButton = EraserButton;
				SetFillColorButtonVisible();
			}
		}
		/// <summary>
		/// Відповідає за установку основного кольору для інструменту.
		/// </summary>
		/// <param name="color">Колір.</param>
		/// <param name="tool">Інструмент.</param>
		private void SetPrimaryColor(Color color, ToolBase tool)
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
		/// <summary>
		/// Подія кнопки, що відповідає за вибір основного кольору інструменту.
		/// </summary>
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
		/// <summary>
		/// Відповідає за установку кольору заливки для інструменту.
		/// </summary>
		/// <param name="color">Колір.</param>
		/// <param name="tool">Інструмент.</param>
		private void SetFillColor(Color color, ToolBase tool)
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
		/// <summary>
		/// Подія кнопки, що відповідає за вибір кольору заливки інструменту.
		/// </summary>
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
		/// <summary>
		/// Подія кнопки, що відповідає за вибір останнього обраного інструменту фігури.
		/// </summary>
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
		/// <summary>
		/// Подія кнопки, що відповідає за вибір інструменту лінії.
		/// </summary>
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
		/// <summary>
		/// Подія кнопки, що відповідає за вибір інструменту прямокутника.
		/// </summary>
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
		/// <summary>
		/// Подія кнопки, що відповідає за вибір інструменту еліпса.
		/// </summary>
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
		/// <summary>
		/// Подія кнопки, що відповідає за вибір інструменту переміщення зображення.
		/// </summary>
		private void MoveToolButton_Click(object sender, EventArgs e)
		{
			if (project != null)
			{
				project.Compositing = CompositingMode.SourceOver;
				CurrentTool = Tools[5];
				CurUse = ToolUse.Transform;

				LastUsedButton.BackColor = Color.Transparent;
				MoveToolButton.BackColor = Color.LightBlue;
				LastUsedButton = MoveToolButton;
				SetFillColorButtonVisible();
			}
		}
		/// <summary>
		/// Подія кнопки, що відповідає за вибір інструменту ініціалізації тексту.
		/// </summary>
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
		/// <summary>
		/// Подія кнопки, що відповідає за вибір інструменту піпетки для вибору основного кольору.
		/// </summary>
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
				SetFillColorButtonVisible();
			}
		}
		/// <summary>
		/// Подія кнопки, що відповідає за вибір інструменту піпетки для вибору кольору заливки.
		/// </summary>
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

		// ============================== Кнопки параметрів шарів та інструментів =======================
		/// <summary>
		/// Подія кнопки, що відповідає за зміну розміру проекту.
		/// </summary>
		private void ResizeProjButton_Click(object sender, EventArgs e)
		{
			if (project != null)
			{
				ProjectCreationWindow d = new ProjectCreationWindow();

				if (d.ShowDialog() == DialogResult.OK)
				{
					project.Size = d.NewProjectSize;
					DrawingBox.Refresh();
					SetDimentionsBoxes();
					CountDrawingBoxSize();
					BufferLayer.LayerSize = d.NewProjectSize;
				}

				d.Dispose();
			}
		}
		/// <summary>
		/// Подія повзунка значень для зміни розміру інструменту.
		/// </summary>
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
		/// <summary>
		/// Подія числового поля для зміни розміру інструменту.
		/// </summary>
		private void ToolWidthBox_ValueChanged(object sender, EventArgs e)
		{
			if (ToolWidthBox.Value != WidthBar.Value)
				WidthBar.Value = (int)ToolWidthBox.Value;
		}
		/// <summary>
		/// Подія числового поля для зміни прозорості основного кольору.
		/// </summary>
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
				ColorButton.BackColor = temp;
				DrawingBox.Refresh();
			}
			else if (CurrentTool != Tools[1])
			{
				temp = Color.FromArgb((int)AlphaBox.Value, CurrentTool.ToolColor);
				CurrentTool.ToolColor = temp;
				ColorButton.BackColor = temp;
			}
			
			
		}
		/// <summary>
		/// Подія числового поля для зміни прозорості кольору заливки.
		/// </summary>
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
		/// <summary>
		/// Подія числового поля для зміни довжини шару або фігури.
		/// </summary>
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
		/// <summary>
		/// Подія числового поля для зміни висоти шару або фігури.
		/// </summary>
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
		/// Подія зміни значення поля, яке вказує, відбувається зараз модифікація фігури чи ні.
		/// Коли checked - відбувається, в іншому випадку - ні.
		/// </summary>
		private void ModifyCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			// Якщо модифікація починається
			if (ModifyCheckBox.Checked)
			{
				// Якщо поточний шар містить інформацію про фігуру.
				if (project.SelectedLayer.Shape != null)
				{
					// Заповнити поля на формі інформацією про цю фігуру.
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
				// Якщо поточний шар містить інформацію про текст.
				else if (project.SelectedLayer.Text != null)
				{
					// Заповнити поля на формі інформацією цього тексту.
					ColorButton.BackColor = project.SelectedLayer.Text.Color;
					AlphaBox.Value = project.SelectedLayer.Text.Color.A;
					WidthBar.Value = (int)project.SelectedLayer.Text.Size;

					TextGB.Visible = true;
				}
			}
			// Якщо модифікація закінчується.
			else
			{
				// Заповнити поля форми інформацією про поточний інструмент.
				ColorButton.BackColor = CurrentTool.ToolColor;
				FillColorButton.BackColor = CurrentTool.FillColor;
				AlphaBox.Value = CurrentTool.ToolColor.A;
				FillAlphaBox.Value = CurrentTool.FillColor.A;
				WidthBar.Value = CurrentTool.ToolWidth;
				ToolWidthBox.Value = CurrentTool.ToolWidth;
				TextGB.Visible = false;
			}
		}
		/// <summary>
		/// Подія, що викликається при зміні видимості кнопки вибору кольору заливки.
		/// </summary>
		private void FillColorButton_VisibleChanged(object sender, EventArgs e)
		{
			// Якщо відбувається модифікація фігури це поле ховати не потрібно в будь якому випадку.
			if (ModifyCheckBox.Checked)
			{
				FillColorButton.Visible = true;
			}
		}
		/// <summary>
		/// Подія, що викликається при зміні видимості кнопки вибору піпетки для кольору заливки.
		/// </summary>
		private void FillPipetteButton_VisibleChanged(object sender, EventArgs e)
		{
			// Якщо відбувається модифікація фігури це поле ховати не потрібно в будь якому випадку.
			if (ModifyCheckBox.Checked)
			{
				FillPipetteButton.Visible = true;
			}
		}
		/// <summary>
		/// Подія зміни тексту в полі з текстом шару. Присвоює текст із цього поля тексту шару.
		/// </summary>
		private void LayerTextBox_TextChanged(object sender, EventArgs e)
		{
			project.SelectedLayer.Text.Text = LayerTextBox.Text;
			project.SelectedLayer.RefreshContents();
			DrawingBox.Refresh();
		}
		/// <summary>
		/// Подія кнопки, що відповідає за вибір шрифту тексту.
		/// </summary>
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
	}
}