using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

public class PreferencesView : UserControl
{
    private FlowLayoutPanel flp0;
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

    public delegate void OnSavePrefsHandler(DataTable prefs);
    public event OnSavePrefsHandler OnSavePrefs = null;

    public PreferencesView()
    {
        this.SuspendLayout();

        //TODO: Convert to TableLayoutPanel

        flp0 = new FlowLayoutPanel();
        flp0.FlowDirection = FlowDirection.TopDown;
        flp0.WrapContents = false;
        flp0.AutoScroll = true;
        flp0.AutoSize = true;
        flp0.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        Controls.Add(flp0);

        flp0.Controls.Add(lUsername = new Label());
        flp0.Controls.Add(tUsername = new TextBox());
        flp0.Controls.Add(lPassword = new Label());
        flp0.Controls.Add(tPassword = new TextBox());
        flp0.Controls.Add(lHostname = new Label());
        flp0.Controls.Add(tHostname = new TextBox());
        flp0.Controls.Add(lDatabase = new Label());
        flp0.Controls.Add(tDatabase = new TextBox());
        flp0.Controls.Add(lPooling = new Label());
        flp0.Controls.Add(cbPooling = new CheckBox());
        flp0.Controls.Add(btnSave = new Button());

        lUsername.Text = "Username";
        lPassword.Text = "Password";
        lHostname.Text = "Hostname";
        lDatabase.Text = "Database";
        lPooling.Text = "Connection Pooling";
        btnSave.Text = "Save";
        btnSave.Click += new EventHandler(btnSave_OnClick);
        
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
