using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace PDFBinder
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            UpdateUI();
        }

        public void AddInputFile(string file)
        {
            switch (Combiner.TestSourceFile(file))
            {
                case Combiner.SourceTestResult.Unreadable:
                    MessageBox.Show(string.Format("File could not be opened as a PDF document:\n\n{0}", file), "Illegal file type", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
                case Combiner.SourceTestResult.Protected:
                    MessageBox.Show(string.Format("PDF document does not allow copying:\n\n{0}", file), "Permission denied", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    break;
                case Combiner.SourceTestResult.Ok:
                    inputListBox.Items.Add(file);
                    break;
            }
        }

        public void UpdateUI()
        {
            if (inputListBox.Items.Count < 2)
            {
                completeButton.Enabled = false;
                helpLabel.Text = "Drop PDF-documents in the box above, or choose \"add document\" from the toolbar";
                loadSaveBtn.Text = "Load";
            }
            else
            {
                completeButton.Enabled = true;
                helpLabel.Text = "Click the \"bind!\" button when you are done adding documents";
                loadSaveBtn.Text = "Save";
            }

            if (inputListBox.SelectedIndex < 0)
            {
                removeButton.Enabled = moveUpButton.Enabled = moveDownButton.Enabled = move10down.Enabled = false;
            }
            else
            {
                removeButton.Enabled = true;
                moveUpButton.Enabled = (inputListBox.SelectedIndex > 0);
                moveDownButton.Enabled = (inputListBox.SelectedIndex < inputListBox.Items.Count - 1);
                move10down.Enabled = (inputListBox.SelectedIndex < inputListBox.Items.Count + 9);
            }
        }

        private void inputListBox_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop, false) ? DragDropEffects.All : DragDropEffects.None;
        }

        private void inputListBox_DragDrop(object sender, DragEventArgs e)
        {
            var fileNames = (string[]) e.Data.GetData(DataFormats.FileDrop);
            Array.Sort(fileNames);

            foreach (var file in fileNames)
            {
                AddInputFile(file);
            }

            UpdateUI();
        }

        private void combineButton_Click(object sender, EventArgs e)
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (var combiner = new Combiner(saveFileDialog.FileName))
                {
                    progressBar.Visible = true;
                    this.Enabled = false;

                    for (int i = 0; i < inputListBox.Items.Count; i++)
                    {
                        combiner.AddFile((string)inputListBox.Items[i]);
                        progressBar.Value = (int)(((i + 1) / (double)inputListBox.Items.Count) * 100);
                    }


                    this.Enabled = true;
                    progressBar.Visible = false;
                }

                System.Diagnostics.Process.Start(saveFileDialog.FileName);
            }
        }

        private void addFileButton_Click(object sender, EventArgs e)
        {
            if (addFileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (string file in addFileDialog.FileNames)
                {
                    AddInputFile(file);
                }

                UpdateUI();
            }
        }

        private void inputListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateUI();
        }

        private void removeButton_Click(object sender, EventArgs e)
        {
            while(inputListBox.SelectedIndices.Count > 0)
            {
                inputListBox.Items.Remove(inputListBox.SelectedItem);
            }
        }

        private void moveItemButton_Click(object sender, EventArgs e)
        {

            var dataItems = inputListBox.SelectedItems;
            var indexes = inputListBox.SelectedIndices;
            var count = dataItems.Count;

            for (int i = (sender == moveUpButton ? 0 : count-1);
                         (sender == moveUpButton ? i < count : i >= 0);
                         i = (sender == moveUpButton ? i+1 : i-1))
            {
                object dataItem = inputListBox.SelectedItems[i];
                int index = inputListBox.SelectedIndices[i];

                if (sender == moveUpButton)
                    index--;
                if (sender == moveDownButton)
                    index++;

                if(index >= 0 && index < inputListBox.Items.Count)
                {
                    inputListBox.Items.Remove(dataItem);
                    inputListBox.Items.Insert(index, dataItem);

                    inputListBox.SelectedIndex = index;

                }

            }



        }

        private void move10down_Click(object sender, EventArgs e)
        {
            for(int i = 0; i<10; i++)
            {
                moveDownButton.PerformClick();
            }
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            var listContentFileName = "listconent.txt";
            if(inputListBox.Items.Count < 2 && System.IO.File.Exists(listContentFileName))
            {
                var allLines = System.IO.File.ReadAllLines(listContentFileName);
                inputListBox.Items.AddRange(allLines);
            } else
            {
                var allLines = new List<string>();
                
                for(int i=0; i < inputListBox.Items.Count; i++)
                {
                    allLines.Add((string)inputListBox.Items[i]);
                }
                System.IO.File.WriteAllLines(listContentFileName, allLines);
            }

        }
    }
}