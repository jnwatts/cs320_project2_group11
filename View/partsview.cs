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
    private List<PartTypeEntry> parttypes = null;
    private Button btnShowParts = null;
    private Button btnEditPart = null;

    public delegate void OnShowPartsHandler(PartTypeEntry partType);
    public event OnShowPartsHandler OnShowParts = null;

    public delegate void OnEditPartHandler(DataRow row);
    public event OnEditPartHandler OnEditPart = null;

    public List<PartTypeEntry> PartTypes {
        set {
            parttypes = value;
            updatePartTypes();
        }
    }

    public object DataSource
    {
        set {
            if (dgv != null) {
                dgv.DataSource = value;
                if (dgv.Rows.Count > 0) {
                    dgv.Rows[0].Selected = true;
                }
            }
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
        tlp0.RowCount++;
        tlp0.Controls.Add(cbPartType, 0, 0);

        btnShowParts = new Button();
        btnShowParts.Text = "Show Parts";
        btnShowParts.Click += new EventHandler(btnShowParts_OnClick);
        tlp0.Controls.Add(btnShowParts, 1, 0);

        btnEditPart = new Button();
        btnEditPart.Text = "Edit Part";
        btnEditPart.Click += new EventHandler(btnEditPart_OnClick);
        tlp0.Controls.Add(btnEditPart, 2, 0);

        dgv = new DataGridView();
        dgv.Dock = DockStyle.Fill;
        dgv.MultiSelect = false;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        tlp0.Controls.Add(dgv, 0, 1);
        tlp0.SetColumnSpan(dgv, 3);

        Util.FixTableLayoutStyles(tlp0);
        
        this.ResumeLayout();
    }

    private void updatePartTypes()
    {
        int index = cbPartType.SelectedIndex;
        cbPartType.Items.Clear();
        if (parttypes != null) {
            foreach (PartTypeEntry pt in parttypes) {
                cbPartType.Items.Add(pt);
            }
            if (index < cbPartType.Items.Count) {
                cbPartType.SelectedIndex = index;
            }
        }
    }

    private void btnShowParts_OnClick(object sender, EventArgs e)
    {
        if (OnShowParts != null) {
            PartTypeEntry partType = (PartTypeEntry)cbPartType.SelectedItem;
            OnShowParts(partType);
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
}
