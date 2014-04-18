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
    RawSqlView rsv = null;
    PreferencesView pv = null;

    public PreferencesView PrefsView { get { return pv; } }
    public RawSqlView RawSqlView { get { return rsv; } }

    public MainView()
    {
        this.SuspendLayout();

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

        this.ResumeLayout();
    }
}
