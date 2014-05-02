using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

public class PartsView : UserControl
{
    private DataGridView dgv = null;
    private ComboBox cbPartType = null;
    private List<PartTypeEntry> parttypes = null;
    public List<PartTypeEntry> PartTypes {
        set {
            parttypes = value;
            updatePartTypes();
        }
    }

    public PartsView()
    {
        this.SuspendLayout();

        //dgv = new DataGridView();
        cbPartType = new ComboBox();
        updatePartTypes();
        this.Controls.Add(cbPartType);
        
        this.ResumeLayout();
    }

    private void updatePartTypes()
    {
        cbPartType.Items.Clear();
        if (parttypes != null) {
            foreach (PartTypeEntry pt in parttypes) {
                cbPartType.Items.Add(pt);
            }
        }
    }
}
