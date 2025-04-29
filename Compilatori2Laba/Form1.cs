using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Compilatori2Laba
{
    public partial class Compiler : Form
    {
        private bool isTextChanged = false;
        private string currentFilePath = string.Empty;

        public Compiler()
        {
            InitializeComponent();
        }

        private void Compiler_Load(object sender, EventArgs e) { }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            isTextChanged = true;
        }

        // 🔹 Функция проверки несохранённых изменений перед важными действиями
        private bool CheckForUnsavedChanges()
        {
            if (!isTextChanged) return true; // Если изменений нет – выходим

            DialogResult result = MessageBox.Show(
                "Сохранить изменения перед продолжением?",
                "Несохранённые изменения",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                return SaveFile();
            }
            else if (result == DialogResult.No)
            {
                return true;
            }

            return false; // "Отмена" – прерываем действие
        }

        // 🔹 Функция сохранения файла
        private bool SaveFile()
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                return SaveFileAs(); // Если нет пути, вызываем "Сохранить как"
            }

            try
            {
                File.WriteAllText(currentFilePath, richTextBox1.Text);
                isTextChanged = false;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // 🔹 Функция "Сохранить как"
        private bool SaveFileAs()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text Files|*.txt",
                Title = "Сохранить файл"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                currentFilePath = saveFileDialog.FileName;
                return SaveFile();
            }

            return false; // Пользователь отменил сохранение
        }

        // 🔹 Открытие файла с проверкой изменений
        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForUnsavedChanges()) return;

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text Files|*.txt",
                Title = "Открыть текстовый файл"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    currentFilePath = openFileDialog.FileName;
                    richTextBox1.Text = File.ReadAllText(currentFilePath);
                    isTextChanged = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при открытии файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // 🔹 Создание нового файла с проверкой изменений
        private void создатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForUnsavedChanges()) return;

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text Files|*.txt",
                Title = "Создать новый текстовый файл",
                FileName = "Новый файл.txt"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.Create(saveFileDialog.FileName).Close();
                    currentFilePath = saveFileDialog.FileName;
                    richTextBox1.Clear();
                    isTextChanged = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при создании файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // 🔹 Сохранение изменений перед выходом из программы
        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CheckForUnsavedChanges())
            {
                Close();
            }
        }

        // 🔹 Проверка перед закрытием формы
        private void Compiler_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!CheckForUnsavedChanges())
            {
                e.Cancel = true; // Отменяем выход, если пользователь передумал
            }
        }

        // 🔹 Сохранение файла
        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        // 🔹 Сохранение файла как
        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileAs();
        }

        // 🔹 Функции редактирования текста
        private void отменитьToolStripMenuItem_Click(object sender, EventArgs e) => richTextBox1.Undo();
        private void повторитьToolStripMenuItem_Click(object sender, EventArgs e) => richTextBox1.Redo();
        private void вырезатьToolStripMenuItem_Click(object sender, EventArgs e) => richTextBox1.Cut();
        private void копироватьToolStripMenuItem_Click(object sender, EventArgs e) => richTextBox1.Copy();
        private void вставитьToolStripMenuItem_Click(object sender, EventArgs e) => richTextBox1.Paste();
        private void удалитьToolStripMenuItem_Click(object sender, EventArgs e) => richTextBox1.SelectedText = string.Empty;
        private void выделитьВсеToolStripMenuItem_Click(object sender, EventArgs e) => richTextBox1.SelectAll();

        // 🔹 Открытие справки и информации о программе
        private void button10_Click(object sender, EventArgs e)
        {
            var AboutProgramm = new AboutProgramm();
            AboutProgramm.Show();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            var Spravka = new Spravka();
            Spravka.Show();
        }

        // 🔹 Функция для анализа текста
        private void buttonScan_Click(object sender, EventArgs e)
        {
            // Чтение текста из редактора
            string inputText = richTextBox1.Text;

            // Создаем сканер и получаем список токенов
            Scanner scanner = new Scanner();
            List<Token> tokens = scanner.Scan(inputText);

            // Создаем парсер и выполняем синтаксический анализ всех деклараций
            Parser parser = new Parser(tokens);
            List<ParseResult> results = parser.ParseAllDeclarations();

            // Вывод результатов в richTextBox2
            richTextBox2.Clear();
            int totalErrors = 0;
            int declNumber = 1;
            foreach (var res in results)
            {
                richTextBox2.AppendText($"Декларация #{declNumber}:\n");
                foreach (var state in res.States)
                {
                    richTextBox2.AppendText(state + "\n");
                }
                if (res.ErrorMessages.Count > 0)
                {
                    richTextBox2.AppendText("Обнаружены ошибки:\n");
                    foreach (var err in res.ErrorMessages)
                    {
                        richTextBox2.AppendText(err + "\n");
                    }
                }
                else
                {
                    richTextBox2.AppendText("Анализ успешно завершен. Ошибок не обнаружено.\n");
                }
                richTextBox2.AppendText("\n");
                totalErrors += res.ErrorCount;
                declNumber++;
            }
            richTextBox2.AppendText($"Общее количество ошибок: {totalErrors}\n");
        }










        //private void buttonScan_Click(object sender, EventArgs e) 
        //{ --- lr3
        //    //// Читаем текст из редактора
        //    //string inputText = richTextBox1.Text;

        //    //// Получаем токены с помощью сканера
        //    //Scanner scanner = new Scanner();
        //    //List<Token> tokens = scanner.Scan(inputText);

        //    //// Создаем парсер и выполняем разбор объявления
        //    //Parser parser = new Parser(tokens);
        //    //ParseResult parseResult = parser.ParseDeclaration();

        //    //// Очищаем окно вывода результатов
        //    //richTextBox2.Clear();

        //    //// Вывод пути состояний КА
        //    //richTextBox2.AppendText("Путь состояний КА:\n");
        //    //foreach (var state in parseResult.States)
        //    //{
        //    //    richTextBox2.AppendText(state + "\n");
        //    //}
        //    //richTextBox2.AppendText("\nРезультат анализа:\n");

        //    //// Если ошибок нет
        //    //if (parseResult.IsSuccess)
        //    //{
        //    //    richTextBox2.AppendText("Анализ успешно завершен. Ошибок не обнаружено.\n");
        //    //}
        //    //else
        //    //{
        //    //    richTextBox2.AppendText("Обнаружены ошибки:\n");
        //    //    foreach (var error in parseResult.ErrorMessages)
        //    //    {
        //    //        richTextBox2.AppendText(error + "\n");
        //    //    }
        //    //}
        //    // Читаем исходный текст из редактора----------------------------------------------- lr3
        //    string inputText = richTextBox1.Text;

        //    // Получаем список токенов с помощью сканера
        //    Scanner scanner = new Scanner();
        //    List<Token> tokens = scanner.Scan(inputText);

        //    // Создаем и запускаем парсер для разбора объявления
        //    Parser parser = new Parser(tokens);
        //    var parseResult = parser.ParseDeclaration();

        //    // Очищаем окно вывода результатов
        //    richTextBox2.Clear();

        //    // Выводим путь состояний конечного автомата
        //    richTextBox2.AppendText("Путь состояний КА:\n");
        //    foreach (var state in parseResult.States)
        //    {
        //        richTextBox2.AppendText(state + "\n");
        //    }

        //    richTextBox2.AppendText("\nРезультат анализа:\n");
        //    // Если разбор успешен – выводим сообщение об отсутствии ошибок
        //    if (parseResult.IsSuccess)
        //    {
        //        richTextBox2.AppendText("Анализ успешно завершен. Ошибок не обнаружено.\n");
        //    }
        //    // Иначе – выводим каждое сообщение об ошибке
        //    else
        //    {
        //        richTextBox2.AppendText("Обнаружены ошибки:\n");
        //        foreach (var error in parseResult.ErrorMessages)
        //        {
        //            richTextBox2.AppendText(error + "\n");
        //        }
        //    }//--------------------------------------------------------------------------------
        //}
        //private void buttonScan_Click(object sender, EventArgs e)
        //{
        //    string inputText = richTextBox1.Text;
        //    var scanner = new Scanner();
        //    List<Token> tokens = scanner.Scan(inputText);

        //    richTextBox2.Clear();
        //    foreach (var token in tokens)
        //    {
        //        richTextBox2.AppendText($"Строка: {token.Line}, с позиции {token.StartPos} по {token.EndPos} — {token.Type}: \"{token.Lexeme}\" (код {(int)token.Code})\n");
        //    }
        //}
    }
}
//using System;
//using System.IO;
//using System.Windows.Forms;

//namespace Compilatori2Laba
//{
//    public partial class Compiler : Form
//    {
//        private bool isTextChanged = false;
//        private string currentFilePath = string.Empty;

//        public Compiler()
//        {
//            InitializeComponent();
//        }

//        private void Compiler_Load(object sender, EventArgs e) { }

//        private void richTextBox1_TextChanged(object sender, EventArgs e)
//        {
//            isTextChanged = true;
//        }

//        // 🔹 Функция проверки несохранённых изменений перед важными действиями
//        private bool CheckForUnsavedChanges()
//        {
//            if (!isTextChanged) return true; // Если изменений нет – выходим

//            DialogResult result = MessageBox.Show(
//                "Сохранить изменения перед продолжением?",
//                "Несохранённые изменения",
//                MessageBoxButtons.YesNoCancel,
//                MessageBoxIcon.Warning);

//            if (result == DialogResult.Yes)
//            {
//                return SaveFile();
//            }
//            else if (result == DialogResult.No)
//            {
//                return true;
//            }

//            return false; // "Отмена" – прерываем действие
//        }

//        // 🔹 Функция сохранения файла
//        private bool SaveFile()
//        {
//            if (string.IsNullOrEmpty(currentFilePath))
//            {
//                return SaveFileAs(); // Если нет пути, вызываем "Сохранить как"
//            }

//            try
//            {
//                File.WriteAllText(currentFilePath, richTextBox1.Text);
//                isTextChanged = false;
//                return true;
//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
//                return false;
//            }
//        }

//        // 🔹 Функция "Сохранить как"
//        private bool SaveFileAs()
//        {
//            SaveFileDialog saveFileDialog = new SaveFileDialog
//            {
//                Filter = "Text Files|*.txt",
//                Title = "Сохранить файл"
//            };

//            if (saveFileDialog.ShowDialog() == DialogResult.OK)
//            {
//                currentFilePath = saveFileDialog.FileName;
//                return SaveFile();
//            }

//            return false; // Пользователь отменил сохранение
//        }

//        // 🔹 Открытие файла с проверкой изменений
//        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
//        {
//            if (!CheckForUnsavedChanges()) return;

//            OpenFileDialog openFileDialog = new OpenFileDialog
//            {
//                Filter = "Text Files|*.txt",
//                Title = "Открыть текстовый файл"
//            };

//            if (openFileDialog.ShowDialog() == DialogResult.OK)
//            {
//                try
//                {
//                    currentFilePath = openFileDialog.FileName;
//                    richTextBox1.Text = File.ReadAllText(currentFilePath);
//                    isTextChanged = false;
//                }
//                catch (Exception ex)
//                {
//                    MessageBox.Show($"Ошибка при открытии файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
//                }
//            }
//        }

//        // 🔹 Создание нового файла с проверкой изменений
//        private void создатьToolStripMenuItem_Click(object sender, EventArgs e)
//        {
//            if (!CheckForUnsavedChanges()) return;

//            SaveFileDialog saveFileDialog = new SaveFileDialog
//            {
//                Filter = "Text Files|*.txt",
//                Title = "Создать новый текстовый файл",
//                FileName = "Новый файл.txt"
//            };

//            if (saveFileDialog.ShowDialog() == DialogResult.OK)
//            {
//                try
//                {
//                    File.Create(saveFileDialog.FileName).Close();
//                    currentFilePath = saveFileDialog.FileName;
//                    richTextBox1.Clear();
//                    isTextChanged = false;
//                }
//                catch (Exception ex)
//                {
//                    MessageBox.Show($"Ошибка при создании файла: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
//                }
//            }
//        }

//        // 🔹 Сохранение изменений перед выходом из программы
//        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
//        {
//            if (CheckForUnsavedChanges())
//            {
//                Close();
//            }
//        }

//        // 🔹 Проверка перед закрытием формы
//        private void Compiler_FormClosing(object sender, FormClosingEventArgs e)
//        {
//            if (!CheckForUnsavedChanges())
//            {
//                e.Cancel = true; // Отменяем выход, если пользователь передумал
//            }
//        }

//        // 🔹 Сохранение файла
//        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
//        {
//            SaveFile();
//        }

//        // 🔹 Сохранение файла как
//        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
//        {
//            SaveFileAs();
//        }

//        // 🔹 Функции редактирования текста
//        private void отменитьToolStripMenuItem_Click(object sender, EventArgs e) => richTextBox1.Undo();
//        private void повторитьToolStripMenuItem_Click(object sender, EventArgs e) => richTextBox1.Redo();
//        private void вырезатьToolStripMenuItem_Click(object sender, EventArgs e) => richTextBox1.Cut();
//        private void копироватьToolStripMenuItem_Click(object sender, EventArgs e) => richTextBox1.Copy();
//        private void вставитьToolStripMenuItem_Click(object sender, EventArgs e) => richTextBox1.Paste();
//        private void удалитьToolStripMenuItem_Click(object sender, EventArgs e) => richTextBox1.SelectedText = string.Empty;
//        private void выделитьВсеToolStripMenuItem_Click(object sender, EventArgs e) => richTextBox1.SelectAll();

//        // 🔹 Открытие справки и информации о программе
//        private void button10_Click(object sender, EventArgs e)
//        {
//            var AboutProgramm = new AboutProgramm();
//            AboutProgramm.Show();
//        }

//        private void button11_Click(object sender, EventArgs e)
//        {
//            var Spravka = new Spravka();
//            Spravka.Show();
//        }
//        private void buttonScan_Click(object sender, EventArgs e)
//        {
//            richTextBox2.Text = richTextBox1.Text;
//            //string inputText = richTextBox1.Text;
//            //richTextBox2.Clear();  // Очищаем richTextBox2 перед выводом новых результатов
//            //Analyze(inputText);  // Запускаем анализ текста
//        }
//    }
//}

