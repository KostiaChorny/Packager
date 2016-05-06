using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Packager.Logic;
using System.IO;

namespace Packager
{
    public partial class MainWindow : Form
    {
        private FilePackage current;

        public FilePackage Current
        {
            get { return current; }
            set
            {
                current?.Dispose();
                current = value;
            }
        }


        public MainWindow()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog.Filter = "File Package|*.pkg|All Files|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Current = new FilePackage(openFileDialog.FileName);

                    UpdateList();
                }
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show($"File {ex.FileName} not found!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Uknown error: {ex.Message}");
            }
        }

        private void UpdateList()
        {
            filesList.Clear();
            if (Current != null)
            {
                foreach (var item in Current.Files)
                {
                    filesList.Items.Add(item.FileName);
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void createToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog.Filter = "All Files|*.*";
                openFileDialog.Multiselect = true;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    FilePackageBuilder builder = new FilePackageBuilder();
                    foreach (var path in openFileDialog.FileNames)
                    {
                        builder.AddFile(path);
                    }

                    saveFileDialog.Filter = openFileDialog.Filter = "File Package|*.pkg|All Files|*.*";
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        Current = builder.Build(saveFileDialog.FileName);
                        UpdateList();
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show($"File {ex.FileName} not found!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Uknown error: {ex.Message}");
            }
        }

        private void extractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Current == null)
            {
                MessageBox.Show("Open or create package file before!");
                return;
            }                

            if (filesList.SelectedItems.Count == 0)
            {
                MessageBox.Show("Select files from list!");
                return;
            }

            try
            {
                if (filesList.SelectedItems.Count == 1)
                {
                    string filename = filesList.SelectedItems[0].Text;
                    saveFileDialog.FileName = filename;
                    saveFileDialog.Filter = "All Files|*.*";
                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        Current.GetFile(filename, saveFileDialog.FileName);
                    }
                }

                if (filesList.SelectedItems.Count > 1)
                {
                    if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    {
                        foreach (ListViewItem item in filesList.SelectedItems)
                        {
                            string filename = item.Text;
                            Current.GetFile(filename, folderBrowserDialog.SelectedPath + "\\" + filename);
                        }
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show($"File {ex.FileName} not found!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Uknown error: {ex.Message}");
            }
        }

        private void extractAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (var file in Current.Files)
                    {
                        Current.GetFile(file.FileName, folderBrowserDialog.SelectedPath + "\\" + file.FileName);
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show($"File {ex.FileName} not found!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Uknown error: {ex.Message}");
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Current = null;
            filesList.Clear();
        }

        private void addFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Current == null)
            {
                MessageBox.Show("Open or create package file before!");
                return;
            }


            try
            {
                openFileDialog.Filter = "All Files|*.*";
                openFileDialog.Multiselect = true;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (var path in openFileDialog.FileNames)
                    {
                        Current.AddFile(path);
                    }
                }
                UpdateList();
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show($"File {ex.FileName} not found!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Uknown error: {ex.Message}");
            }
        }
    }
}
