using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace PictureSort
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}
		// адреса папок
		//папка для сортировки
		string mainFL;
		//папка с изображениями
		string localFolder;
		//имя папки(800х600, 1024х768...)
		string fldTOCP;

		Stopwatch st;
		TimeSpan ts;


		private void button1_Click(object sender, EventArgs e)
		{
			//выкл. запрет на доступ к элементам созданым в разных потоках
			CheckForIllegalCrossThreadCalls = false;
			st = Stopwatch.StartNew();
			textBox3.Text = "Идет обработка...";
			// присваиваем адреса папок
			if (textBox1.Text.EndsWith(@"\") && textBox2.Text.EndsWith(@"\"))
			{
				localFolder = textBox1.Text;
				mainFL = textBox2.Text;
			}
			else
			{
				textBox1.Text += @"\";
				textBox2.Text += @"\";
				localFolder = textBox1.Text;
				mainFL = textBox2.Text;
			}
			//очистка лога
			textBox3.Text = "";
			//в случае если адреса папок не введены
			if (textBox1.Text == "" || textBox2.Text == "")
			{
				MessageBox.Show("Неправильно заданы адреса папок");
			}
			else
			{
				checkBox1.Enabled = false;
				backgroundWorker1.RunWorkerAsync();
			}

		}

		//функция соритировки
		public int sortIMG(string path)
		{
			//имя конечного файла
			string endFile = "";
			//имя папки (разрешение)
			fldTOCP = "";
			//backup имени файла( на тот случай если даже изменив имя при совпадении, все равно появляется колизия
			string BCendFile = "";

			try
			{		//если переданый в функцию файл изображение		
				if (path.EndsWith(".jpg") || path.EndsWith(".bmp") || path.EndsWith(".png"))
				{
					//создаем изображение
					Bitmap bm = new Bitmap(path);
					//получаем его размеры
					fldTOCP = bm.PhysicalDimension.Width.ToString() + "x" + bm.PhysicalDimension.Height.ToString();
					//уничтожаем что бы освободить файл
					bm.Dispose();
					//создаем директорию с разрешением
					Directory.CreateDirectory(mainFL + fldTOCP);
					//генерируем путь куда скопировать его
					//дир. для сортировки + дир. с его разрешением + имя файла
					endFile = mainFL + fldTOCP  + path.Substring(path.LastIndexOf(@"\"));
					//отвечает за повторное прохождение цикла в случае коллизии
					bool fileexist = true;
					//счетчик в случае коллизии, помогает создать уникальное имя
					int i = 0;
					//делаем backup пути куда копировать( на случай коллизии)
					BCendFile = endFile;
					while (fileexist)
					{
						//получаем информацию о файлу
						FileInfo infENDFILE = new FileInfo(endFile);
						//если его не существует, тогда копируем/перемещаем
						if (!infENDFILE.Exists)
						{
							//перемещаем
							if (radioButton1.Checked == true)
							{
								File.Move(path, endFile);
								//в цикл больше не возвращаемся
								fileexist = false;
							} 
							//копируем
							if(radioButton2.Checked == true)
							{
								File.Copy(path, endFile);
								//в цикл больше не возвращаемся
								fileexist = false;
							}
	
						}
						else
						{
							//если возникли коллизия(такой файл уже существует)
							//колдунство с именем файла если
							//таковой уже есть в конечной директории
							//если уже не первый раз заходим в цикл, возвращаем переменной endFile, начальное значение
							endFile = BCendFile;
							//копируем переменную endFile, для обработки и присваение ей уник. умени
							string tmp = endFile;
							//вырезаем расширение файла
							string format = tmp.Substring(tmp.LastIndexOf("."));
							//удалеяем расширение
							tmp = tmp.Remove(tmp.LastIndexOf("."),4);
							//дописываем шаг цикла( генерация уникальноги имени)
							tmp = tmp + "_" + i.ToString();
							//дописываем расширение
							tmp = tmp + format;
							//получаем имя вида ...xxx_1.jpg или xxx_2.jpg
							endFile = tmp ;
							//снова проходим цикл для копирования или перемещения файла
							fileexist = true;
							//
							//в случае если снова такой файл уже существует, тогда снова проходим цикл, но в начале 
							//мы придаем переменной endFile, то значение которое было в самом начале, но т.к. i увеличилось,
							//имя будет уже другим.
							//

						
						}
						//увеличиваем счетчик при каждом проходу цикла
						i++;
					}				
				}
			}
			catch (System.Exception ex)
			{
                textBox3.Text += ex.Message; //MessageBox.Show(ex.Message);
			}
			
			return 0;
		}

		//функция которая проходит по директориям и отдает на обработку все найденые файлы
		public int GetDirectory(string dr)
		{

			//получаем список директорий, в переданой нам директории
			string[] DIR = Directory.GetDirectories(dr);
			//проходим по этому списку
			foreach (string dir in DIR)
			{	
				//получаем список всех файлов в каждой из директории и
				//предаем их в функцию сортировки
				string[] lsFLS = Directory.GetFiles(dir);
				foreach (string img in lsFLS)
				{

					sortIMG(img);
				}	
				//рекурсия от каждой директории(максимальное углубление )
				GetDirectory(dir);

				
			}

			return 0;
		}
		//2ой поток
		private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
		{
			//получаем список файлов в указаной директории с изображениями
			Thread.CurrentThread.Priority = ThreadPriority.Highest;
			string[] lsFLS = Directory.GetFiles(localFolder);
			foreach (string img in lsFLS)
			{
				//сортируем их
				sortIMG(img);
			}
			//если указано 'обрабатывать подпапки', тогда обрабатываем все папки которыем там есть
			if (checkBox1.Checked == true)
			{
				GetDirectory(localFolder);
			}

			st.Stop();

			ts = st.Elapsed;
			//информируем пользователя
			textBox3.Text = "Сортировка завершена!";
			textBox3.Text += "\nВремя: \n" + ts.ToString();
			checkBox1.Enabled = true;

		}

		private void Form1_Load(object sender, EventArgs e)
		{

		}

		private void button3_Click(object sender, EventArgs e)
		{
			//выбираем директорию с изображениями
			if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
			{
				textBox1.Text = folderBrowserDialog1.SelectedPath;
			}
		
		}

		private void button2_Click(object sender, EventArgs e)
		{
			//выбираем директорию с изображниями
			if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
			{
				textBox2.Text = folderBrowserDialog1.SelectedPath;
				MessageBox.Show("Убедитесь, что директория для соритировки, не находиться в директории с сортируемыми изображениями.", "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}

	}
}
