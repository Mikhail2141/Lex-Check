using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Keylogger
{
    public partial class Form1 : Form
    {
        private string selectedDirectory; 
        public Form1()
        {
            InitializeComponent();
        }

        public void deleteButtonStart()
        {
            button1.Visible = false;
        }

         
        public async Task WordsSearch()
        {
            
            string word = textBox1.Text.ToLower().Trim();
            bool found = false;

            string[] allFiles = Directory.GetFiles(selectedDirectory);
            var filesWithWord = new List<string>();
            for (int file = 0; file < allFiles.Length;  file++)
            {
                string fileText = File.ReadAllText(allFiles[file]);
                if (fileText.ToLower().Contains(word))
                {

                    filesWithWord.Add(allFiles[file]);
                    found = true;

                }
            }

            if (found)
            {
                
                MessageBox.Show($"Слово \"{word}\" найдено в папке {selectedDirectory}.");
                foreach (var item in filesWithWord)
                {
                    textBox2.Text += "\n" + item.ToString();
                }
                
                
            }
            else
            {
                MessageBox.Show($"Слово \"{word}\" не найдено в папке {selectedDirectory}.");
            }
        }

        async private void button1_Click(object sender, EventArgs e)
        {
            await WordsSearch(); 
        }
         
        private void button2_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                selectedDirectory = folderBrowserDialog1.SelectedPath;  
                MessageBox.Show($"Вы выбрали папку: {selectedDirectory}");
            }
        }
    }
}