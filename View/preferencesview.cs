using System;
using System.Reflection;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

public class PreferencesView : UserControl
{
    private TableLayoutPanel tlp0;
    private Label lUsername;
    private Label lPassword;
    private Label lHostname;
    private Label lDatabase;
    private Label lPooling;
    private TextBox tUsername;
    private TextBox tPassword;
    private TextBox tHostname;
    private TextBox tDatabase;
    private CheckBox cbPooling;
    private Button btnSave;
    private Label lVersion;

    public delegate void OnSavePrefsHandler(DataTable prefs);
    public event OnSavePrefsHandler OnSavePrefs = null;

    public PreferencesView()
    {
        this.SuspendLayout();

        tlp0 = new TableLayoutPanel();
        tlp0.Dock = DockStyle.Fill;
        Controls.Add(tlp0);

        tlp0.ColumnCount = 2;

        tlp0.RowCount++;
        tlp0.Controls.Add(lUsername = new Label());
        tlp0.Controls.Add(tUsername = new TextBox());
        tlp0.RowCount++;
        tlp0.Controls.Add(lPassword = new Label());
        TextBox passwordTB = new TextBox();
        passwordTB.UseSystemPasswordChar = true;
        tlp0.Controls.Add(tPassword = passwordTB);
        tlp0.RowCount++;
        tlp0.Controls.Add(lHostname = new Label());
        tlp0.Controls.Add(tHostname = new TextBox());
        tlp0.RowCount++;
        tlp0.Controls.Add(lDatabase = new Label());
        tlp0.Controls.Add(tDatabase = new TextBox());
        tlp0.RowCount++;
        tlp0.Controls.Add(lPooling = new Label());
        tlp0.Controls.Add(cbPooling = new CheckBox());
        tlp0.RowCount++;
        tlp0.Controls.Add(btnSave = new Button());
        tlp0.RowCount++;
        tlp0.Controls.Add(lVersion = new Label(), 0, tlp0.RowCount - 1);

        tUsername.Dock = DockStyle.Fill;
        tPassword.Dock = DockStyle.Fill;
        tHostname.Dock = DockStyle.Fill;
        tDatabase.Dock = DockStyle.Fill;

        lUsername.Text = "Username";
        lPassword.Text = "Password";
        lHostname.Text = "Hostname";
        lDatabase.Text = "Database";
        lPooling.Text = "Connection Pooling";
        btnSave.Text = "Save";
        btnSave.Click += new EventHandler(btnSave_OnClick);

        lVersion.Text = "v" + Assembly.GetExecutingAssembly().GetName().Version;

        // Work around weird bug w/ last column's SizeType.AutoSize growing but not shrinking
        Util.FixTableLayoutStyles(tlp0);

        this.ResumeLayout();
    }

    private void btnSave_OnClick(object sender, EventArgs e)
    {
        if (OnSavePrefs != null) {
            DataTable dt = new DataTable("Preferences");
            dt.Columns.Add("Name", Type.GetType("System.String"));
            dt.Columns.Add("Value", Type.GetType("System.String"));
            dt.Rows.Add(new Object[] {"Username", tUsername.Text});
            dt.Rows.Add(new Object[] {"Password", tPassword.Text});
            dt.Rows.Add(new Object[] {"Hostname", tHostname.Text});
            dt.Rows.Add(new Object[] {"Database", tDatabase.Text});
            dt.Rows.Add(new Object[] {"Pooling", cbPooling.Checked ? "true" : "false"});
            OnSavePrefs(dt);
        }
    }

    public DataTable DataSource {
        set
        {
            tUsername.Text = Util.GetColumnString(value, "Username", "");
            tPassword.Text = Util.GetColumnString(value, "Password", "");
            tHostname.Text = Util.GetColumnString(value, "Hostname", "");
            tDatabase.Text = Util.GetColumnString(value, "Database", "");
            cbPooling.Checked = Util.GetColumnBool(value, "Pooling", false);
        }
    }
}
