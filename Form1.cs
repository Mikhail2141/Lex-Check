using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Keylogger
{
    public partial class Form1 : Form
    {
        private string selectedDirectory;
        private int totalFilesCount;
        private static int processedFilesCount;
        private CancellationTokenSource cts;

        public Form1()
        {
            InitializeComponent();
        }

        public void DeleteButtonStart()
        {
            button1.Visible = false;
        }

        public async Task WordsSearch()
        {
            try
            {
                 
                cts = new CancellationTokenSource();

                string word = textBox1.Text.Trim().ToLower();
                bool found = false;
                List<string> filesWithWord = await FileSearch(selectedDirectory, word, cts.Token);

                if (filesWithWord.Count > 0)
                {
                    found = true;
                    MessageBox.Show($"Слово \"{word}\" найдено в папке {selectedDirectory}.");
                    textBox2.Clear();
                    foreach (var file in filesWithWord)
                    {
                        textBox2.AppendText("\n" + file);
                    }
                }
                else
                {
                    MessageBox.Show($"Слово \"{word}\" не найдено в папке {selectedDirectory}.");
                }
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Операция поиска отменена.");
            }
            finally
            {
                cts.Dispose();
            }
        }

        private async Task<List<string>> FileSearch(string directory, string word, CancellationToken token)
        {
            // Определим общее количество файлов
            totalFilesCount = CountTotalFiles(directory);
            processedFilesCount = 0;

            List<string> result = new List<string>();
            var tasks = new List<Task>();

            string[] files = Directory.GetFiles(directory);

            foreach (string file in files)
            {
                if (token.IsCancellationRequested)
                    break;

                tasks.Add(Task.Run(() => CheckFileForMatch(this, file, word, result, token)));
            }

            await Task.WhenAll(tasks);

            // Обрабатываем вложенные директории
            string[] subDirs = Directory.GetDirectories(directory);
            foreach (string subDir in subDirs)
            {
                if (token.IsCancellationRequested)
                    break;

                result.AddRange(await FileSearch(subDir, word, token));
            }

            return result;
        }

        private int CountTotalFiles(string directory)
        {
            return Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories).Count();
        }

        private static void CheckFileForMatch(Form1 form1 , string path, string term, ICollection<string> results, CancellationToken token)
        {
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    string content = sr.ReadToEnd();
                    if (content.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        lock (results)
                        {
                            results.Add(path);
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                Interlocked.Increment(ref processedFilesCount); // Увеличим количество обработанных файлов
                form1.UpdateProgressBar(); // Сразу обновляем прогрессбар
            }
        }

        private void UpdateProgressBar()
        {
            this.Invoke((MethodInvoker)(() =>
            {
                progressBar1.Maximum = totalFilesCount;
                progressBar1.Value = processedFilesCount;
            }));
        }

        private void ResetProgressBar()
        {
            this.Invoke((MethodInvoker)(() =>
            {
                progressBar1.Value = 0;
                progressBar1.Refresh();
            }));
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
         
        }

        async private void button1_Click(object sender, EventArgs e)
        {
            ResetProgressBar();
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