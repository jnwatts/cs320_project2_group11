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
    private DataGridView dgvExtendedAttributes = null;

    private PartEntry origPart = null;
    private PartEntry newPart = null;

    public delegate void OnSavePartHandler(PartEntry partEntry);
    public event OnSavePartHandler OnSavePart = null;

    public PartEntry Part {
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
            if (OnSavePart != null) {
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
        dgvAttributes.AllowUserToAddRows = false;
        dgvAttributes.AllowUserToDeleteRows = false;
        dgvAttributes.DataBindingComplete += new DataGridViewBindingCompleteEventHandler(dgv_DataBindingComplete);
        tlp0.Controls.Add(dgvAttributes, 0, 1);
        tlp0.SetColumnSpan(dgvAttributes, 3);

        dgvExtendedAttributes = new DataGridView();
        dgvExtendedAttributes.Dock = DockStyle.Top;
        dgvExtendedAttributes.MultiSelect = false;
        dgvExtendedAttributes.AllowUserToAddRows = false;
        dgvExtendedAttributes.AllowUserToDeleteRows = false;
        dgvExtendedAttributes.DataBindingComplete += new DataGridViewBindingCompleteEventHandler(dgv_DataBindingComplete);
        tlp0.Controls.Add(dgvExtendedAttributes, 0, 2);
        tlp0.SetColumnSpan(dgvExtendedAttributes, 3);

        Util.FixTableLayoutStyles(tlp0);
        
        this.ResumeLayout();
    }

    private void dgv_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
    {
        DataGridView dgv = (DataGridView)sender;
        if (dgv.Columns.Contains("Part_num")) {
            dgv.Columns["Part_num"].Visible = false;
        }
        if (dgv.Columns.Contains("Part_type_id")) {
            dgv.Columns["Part_type_id"].Visible = false;
        }
    }

    public void Reset()
    {
        if (origPart != null) {
            newPart = new PartEntry(origPart);
            txtPart.Text = newPart.Part_num + ", " + newPart.Part_type;
            dgvAttributes.DataSource = newPart.Attributes;
            dgvExtendedAttributes.DataSource = newPart.ExtendedAttributes;
        }
    }
}
