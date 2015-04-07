using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

public class PartEditView : UserControl
{
    private TableLayoutPanel tlp0 = null;
    private TextBox txtPart = null;
    private Button btnSave = null;
    private Button btnReset = null;
    private DataGridView dgvAttributes = null;

    private Part origPart = null;
    private Part newPart = null;

    public delegate void OnSavePartHandler(Part part);
    public event OnSavePartHandler OnSavePart = null;

    public Part Part {
        get {
            return newPart;
        }

        set {
            origPart = value;
            this.Reset();
        }
    }

    public PartEditView()
    {
        this.SuspendLayout();

        tlp0 = new TableLayoutPanel();
        tlp0.Dock = DockStyle.Fill;
        this.Controls.Add(tlp0);

        txtPart = new TextBox();
        txtPart.Text = "";
        txtPart.Width *= 2;
        txtPart.ReadOnly = true;
        tlp0.Controls.Add(txtPart, 0, 0);

        btnSave = new Button();
        btnSave.Text = "Save";
        btnSave.Click += new EventHandler(delegate (object sender, EventArgs e) {
            if (OnSavePart != null && newPart != null) {
                foreach (DataRow row in newPart.Attributes.Rows) {
                    row.EndEdit();
                }
                OnSavePart(newPart);
            }
        });
        tlp0.Controls.Add(btnSave, 1, 0);

        btnReset = new Button();
        btnReset.Text = "Reset";
        btnReset.Click += new EventHandler(delegate (object sender, EventArgs e) {
            this.Reset();
        });
        tlp0.Controls.Add(btnReset, 2, 0);

        dgvAttributes = new DataGridView();
        dgvAttributes.Dock = DockStyle.Top;
        dgvAttributes.MultiSelect = false;
        dgvAttributes.RowHeadersVisible = false;
        dgvAttributes.AllowUserToAddRows = false;
        dgvAttributes.AllowUserToDeleteRows = false;
        dgvAttributes.DataBindingComplete += new DataGridViewBindingCompleteEventHandler(dgv_DataBindingComplete);
        dgvAttributes.CellParsing += new DataGridViewCellParsingEventHandler(dgv_CellParsing);
        tlp0.Controls.Add(dgvAttributes, 0, 1);
        tlp0.SetColumnSpan(dgvAttributes, 3);

        tlp0.RowCount = 2;
        tlp0.ColumnCount = 3;

        Util.FixTableLayoutStyles(tlp0);
        
        this.ResumeLayout();
    }

    private void dgv_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
    {
        DataGridView dgv = (DataGridView)sender;
        dgv.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
        if (dgv.Columns.Contains("Part_num")) {
            dgv.Columns.Remove("Part_num");
        }
        if (dgv.Columns.Contains("Part_type_id")) {
            dgv.Columns.Remove("Part_type_id");
        }
    }

    private void dgv_CellParsing(object sender, DataGridViewCellParsingEventArgs e)
    {
        if (e != null && e.Value != null) {
            if (e.Value.ToString().Contains("\r") || e.Value.ToString().Contains("\n")) {
                e.Value = e.Value.ToString().Replace("\r", "").Replace("\n", "");
                e.ParsingApplied = true;
            }
        }
    }

    public void Reset()
    {
        if (origPart != null) {
            newPart = new Part(origPart);
            txtPart.Text = newPart.Part_num + ", " + newPart.Part_type;
            dgvAttributes.DataSource = newPart.Attributes;
        }
    }
}
