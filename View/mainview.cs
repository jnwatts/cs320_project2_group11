using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

public class MainView : Form
{
    TabControl tabControl = null;
    TabPage rsvTab = null;
    TabPage pvTab = null;
    TabPage partsTab = null;
    TabPage partEditTab = null;
    RawSqlView rsv = null;
    PreferencesView pv = null;
    PartsView parts = null;
    PartEditView partEdit = null;

    public PreferencesView PrefsView { get { return pv; } }
    public RawSqlView RawSqlView { get { return rsv; } }
    public PartsView PartsView { get { return parts; } }
    public PartEditView PartEditView { get { return partEdit; } }

    public MainView()
    {
        this.SuspendLayout();

        this.DoubleBuffered = true;

        this.Height = 600;
        this.Width = 600;

        tabControl = new TabControl();
        tabControl.Dock = DockStyle.Fill;
        Controls.Add(tabControl);

        rsvTab = new TabPage("Raw SQL");
        rsv = new RawSqlView();
        rsv.Dock = DockStyle.Fill;
        rsvTab.Controls.Add(rsv);
        tabControl.TabPages.Add(rsvTab);

        pvTab = new TabPage("Preferences");
        pv = new PreferencesView();
        pv.Dock = DockStyle.Fill;
        pvTab.Controls.Add(pv);
        tabControl.TabPages.Add(pvTab);

        partsTab = new TabPage("Parts");
        parts = new PartsView();
        parts.Dock = DockStyle.Fill;
        partsTab.Controls.Add(parts);
        tabControl.TabPages.Add(partsTab);

        partEditTab = new TabPage("Edit Part");
        partEdit = new PartEditView();
        partEdit.Dock = DockStyle.Fill;
        partEditTab.Controls.Add(partEdit);
        tabControl.TabPages.Add(partEditTab);

        this.ResumeLayout();
    }
}
