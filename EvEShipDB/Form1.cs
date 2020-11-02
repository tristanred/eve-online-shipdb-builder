using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Collections;
using System.Net.Http.Headers;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Runtime.DesignerServices;

namespace EvEShipDB
{
    public partial class databaseForm : Form
    {
        DataTable dataSource;
        string inputFilePath = "";

        public databaseForm()
        {
            InitializeComponent();
        }

        private void inputBox_TextChanged(object sender, EventArgs e)
        {
            if(this.shipsList.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a row first.");
                return;
            }

            DataGridViewRow selectedElement = this.shipsList.SelectedRows[0];
            

            string newText = inputBox.Text;

            string[] attributes = newText.Split('\n');

            foreach (string item in attributes)
            {
                string[] attr = item.Split('\t');

                if(attr.Length == 1) // Group attribute, no data
                {
                    continue;
                }

                string name = attr[0].Replace(",", "").Replace("\r", "");
                string value = attr[1].Replace(",", "").Replace("\r", "");

                setAttributeInGrid(name, value, selectedElement);
            }
        }

        private void setAttributeInGrid(string name, string value, DataGridViewRow row)
        {
            foreach (DataGridViewCell cell in row.Cells)
            {
                if(cell.OwningColumn.Name == name)
                {
                    cell.Value = value;
                }
            }
        }

        private void loadButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog diag = new OpenFileDialog();
            diag.Filter = "CSV Files|*.csv";
            var result = diag.ShowDialog();

            if(result != DialogResult.OK)
                return;

            inputFilePath = diag.FileName;

            this.dataSource = new DataTable();

            string[] data = File.ReadAllLines(inputFilePath);
            string[] headers = data[0].Split(',');

            foreach (string header in headers)
            {
                dataSource.Columns.Add(header.Replace(",", ""));
            }

            foreach (string lines in data.Skip(1))
            {
                //string[] attributes = lines.Split(',');
                string[] attributes = lines.Split(',').Select(p => p.Replace(",", "").Replace("\r", "")).ToArray();

                dataSource.Rows.Add(attributes);
            }

            this.shipsList.DataSource = dataSource;
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = getCurrentRow();
            if(row != null)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    cell.Value = "";
                }
            }
        }

        private DataGridViewRow getCurrentRow()
        {
            if (this.shipsList.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select a row first.");
                
                return null;
            }

            return this.shipsList.SelectedRows[0];
        }

        private void nameEditBox_TextChanged(object sender, EventArgs e)
        {
            DataGridViewRow row = getCurrentRow();
            if (row != null)
            {
                setAttributeInGrid("Name", nameEditBox.Text, row);
            }
        }

        private void classEditBox_TextChanged(object sender, EventArgs e)
        {
            DataGridViewRow row = getCurrentRow();
            if (row != null)
            {
                setAttributeInGrid("Class", classEditBox.Text, row);
            }

        }

        private void techEditBox_TextChanged(object sender, EventArgs e)
        {
            DataGridViewRow row = getCurrentRow();
            if (row != null)
            {
                setAttributeInGrid("Tech", techEditBox.Text, row);
            }
        }

        private void factionEditBox_TextChanged(object sender, EventArgs e)
        {
            DataGridViewRow row = getCurrentRow();
            if (row != null)
            {
                setAttributeInGrid("Faction", factionEditBox.Text, row);
            }
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            StreamWriter outFile = File.CreateText(inputFilePath);
            CsvWriter writer = new CsvWriter(outFile, CultureInfo.InvariantCulture);

            foreach (DataColumn item in this.dataSource.Columns)
            {
                writer.WriteField(item.ColumnName);
            }
            writer.NextRecord();

            foreach (DataRow item in this.dataSource.Rows)
            {
                List<string> values = item.ItemArray.Cast<Object>().Select(p =>
                {
                    if (p.GetType() == typeof(System.DBNull))
                    {
                        return "";
                    }

                    return p;
                }).Cast<string>().ToList();

                foreach (string attribute in values)
                {
                    writer.WriteField(attribute);
                }

                writer.NextRecord();
            }

            writer.Dispose();
            outFile.Dispose();
        }

        private void addButton_Click(object sender, EventArgs e)
        {
            this.dataSource.Rows.Add();
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            DataGridViewRow row = getCurrentRow();
            if(row != null)
            {
                this.dataSource.Rows.RemoveAt(row.Index);
            }
        }
    }
}
