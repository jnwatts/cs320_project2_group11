using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

public class PartsView : UserControl
{
    private TableLayoutPanel tlp0 = null;
    private DataGridView dgv = null;
    private ComboBox cbPartType = null;
    private List<PartType> parttypes = null;
    private Button btnRefresh = null;
    private Button btnNewPart = null;
    private Button btnEditPart = null;
    private Button btnDeletePart = null;

    public delegate void OnShowPartsHandler(PartType partType);
    public event OnShowPartsHandler OnShowParts = null;

    public delegate void OnNewPartHandler(PartType partType);
    public event OnNewPartHandler OnNewPart = null;

    public delegate void OnEditPartHandler(DataRow row);
    public event OnEditPartHandler OnEditPart = null;

    public delegate void OnDeletePartHandler(DataRow row);
    public event OnDeletePartHandler OnDeletePart = null;

    public List<PartType> PartTypes {
        set {
            parttypes = value;
            updatePartTypes();
        }
    }

    public object DataSource
    {
        set {
            if (dgv != null) {
                int index = 0;
                if (dgv.SelectedRows.Count > 0) {
                    index = dgv.SelectedRows[0].Index;
                }
                dgv.DataSource = value;
                if (dgv.Rows.Count > 0) {
                    if (index >= dgv.Rows.Count) {
                        index = dgv.Rows.Count - 1;
                    }
                    if (index > 0) {
                        dgv.Rows[0].Selected = false;
                    }
                    dgv.Rows[index].Selected = true;
                    if (dgv.Rows[index].Cells[0].Visible)
                    {
                        dgv.CurrentCell = dgv.Rows[index].Cells[0];
                    }
                }
            }
        }
    }

    public PartType SelectedPartType {
        get {
            return (PartType)cbPartType.SelectedItem;
        }
        set {
            cbPartType.SelectedItem = value;
        }
    }

    public PartsView()
    {
        this.SuspendLayout();

        tlp0 = new TableLayoutPanel();
        tlp0.Dock = DockStyle.Fill;
        this.Controls.Add(tlp0);

        cbPartType = new ComboBox();
        cbPartType.DropDownStyle = ComboBoxStyle.DropDownList;
        cbPartType.SelectedIndexChanged += new EventHandler(delegate (object sender, EventArgs e) {
            this.RefreshParts();
        });
        tlp0.RowCount++;
        tlp0.Controls.Add(cbPartType, 0, 0);

        btnRefresh = new Button();
        btnRefresh.Text = "Refresh";
        btnRefresh.Click += new EventHandler(btnRefresh_OnClick);
        tlp0.Controls.Add(btnRefresh, 1, 0);

        btnNewPart = new Button();
        btnNewPart.Text = "New Part";
        btnNewPart.Click += new EventHandler(btnNewPart_OnClick);
        tlp0.Controls.Add(btnNewPart, 2, 0);

        btnEditPart = new Button();
        btnEditPart.Text = "Edit Part";
        btnEditPart.Click += new EventHandler(btnEditPart_OnClick);
        tlp0.Controls.Add(btnEditPart, 3, 0);

        btnDeletePart = new Button();
        btnDeletePart.Text = "Delete Part";
        btnDeletePart.Click += new EventHandler(btnDeletePart_OnClick);
        tlp0.Controls.Add(btnDeletePart, 4, 0);

        dgv = new DataGridView();
        dgv.Dock = DockStyle.Fill;
        dgv.MultiSelect = false;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.AllowUserToAddRows = false;
        dgv.AllowUserToDeleteRows = false;
        dgv.ReadOnly = true;
        dgv.DataBindingComplete += new DataGridViewBindingCompleteEventHandler(dgv_DataBindingComplete);
        dgv.CellDoubleClick += new DataGridViewCellEventHandler(delegate (object sender, DataGridViewCellEventArgs e) {
            if (OnEditPart != null) {
                DataRowView view = (DataRowView)dgv.Rows[e.RowIndex].DataBoundItem;
                DataRow row = view.Row;
                OnEditPart(row);
            }
        });
        tlp0.RowCount++;
        tlp0.Controls.Add(dgv, 0, 1);
        tlp0.SetColumnSpan(dgv, 5);

        Util.FixTableLayoutStyles(tlp0);
        
        this.ResumeLayout();
    }

    private void updatePartTypes()
    {
        int index = cbPartType.SelectedIndex;
        cbPartType.Items.Clear();
        if (parttypes != null) {
            foreach (PartType pt in parttypes) {
                cbPartType.Items.Add(pt);
            }
            if (index < 0) {
                index = 0;
            }
            if (index < cbPartType.Items.Count) {
                cbPartType.SelectedIndex = index;
            }
        }
    }

    private void dgv_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
    {
        DataGridView dgv = (DataGridView)sender;
        if (dgv.Columns.Contains("Part_type_id")) {
            dgv.Columns["Part_type_id"].Visible = false;
        }
        if (dgv.Columns.Contains("Type")) {
            dgv.Columns["Type"].Visible = false;
        }
    }

    private void btnRefresh_OnClick(object sender, EventArgs e)
    {
        this.RefreshParts();
    }

    private void btnNewPart_OnClick(object sender, EventArgs e)
    {
        if (OnNewPart != null) {
            OnNewPart(this.SelectedPartType);
        }
    }

    private void btnEditPart_OnClick(object sender, EventArgs e)
    {
        if (OnEditPart != null && dgv.SelectedRows.Count > 0) {
            DataRowView view = (DataRowView)dgv.SelectedRows[0].DataBoundItem;
            DataRow row = view.Row;
            OnEditPart(row);
        }
    }

    private void btnDeletePart_OnClick(object sender, EventArgs e)
    {
        if (OnDeletePart != null && dgv.SelectedRows.Count > 0) {
            DataRowView view = (DataRowView)dgv.SelectedRows[0].DataBoundItem;
            DataRow row = view.Row;
            OnDeletePart(row);
        }
    }

    public void RefreshParts()
    {
        if (OnShowParts != null) {
            OnShowParts(this.SelectedPartType);
        }
    }
}
