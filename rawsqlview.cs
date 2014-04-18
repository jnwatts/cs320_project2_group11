using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

public class RawSqlView : UserControl
{
    private TableLayoutPanel tlp0;
    private Button btnExecute = null;
    private TextBox tError = null;
    private TextBox tCmd = null;
    private DataGridView g = null;

    public delegate void SQLExecuteHandler(string command);
    public event SQLExecuteHandler OnSQLExecute = null;

    public RawSqlView()
    {
        this.SuspendLayout();

        tlp0 = new TableLayoutPanel();
        tlp0.Dock = DockStyle.Fill;
        Controls.Add(tlp0);

        tlp0.ColumnCount = 1;

        tCmd = new TextBox();
        tCmd.Text = "SHOW DATABASES;";
        tCmd.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        tCmd.Multiline = true;
        tCmd.Height *= 4;
        KeyEventHandler keh = new KeyEventHandler(delegate(object sender, KeyEventArgs e) {
                    if (e.KeyData == (Keys.Control | Keys.Return) || e.KeyData == (Keys.Alt | Keys.Return)) {
                        btnExecute_OnClick(sender, new EventArgs());
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                    }
                });
        tCmd.KeyDown += keh;
        tlp0.RowCount++;
        tlp0.Controls.Add(tCmd);

        btnExecute = new Button();
        btnExecute.Text = "Execute";
        btnExecute.Click += new EventHandler(btnExecute_OnClick);
        btnExecute.Anchor = AnchorStyles.Left;
        tlp0.RowCount++;
        tlp0.Controls.Add(btnExecute);

        tError = new TextBox();
        tError.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        tError.Text = "";
        tError.Multiline = true;
        tError.Height *= 4;
        tError.ReadOnly = true;
        tlp0.RowCount++;
        tlp0.Controls.Add(tError);
        
        g = new DataGridView();
        g.Dock = DockStyle.Fill;
        g.AutoGenerateColumns = true;
        tlp0.RowCount++;
        tlp0.Controls.Add(g);

        Util.FixTableLayoutStyles(tlp0);

        this.ResumeLayout();
    }

    public Object DataSource
    {
        get
        {
            return g.DataSource;
        }

        set
        {
            g.DataSource = value;
        }
    }

    public string Error
    {
        set
        {
            tError.Text = value;
        }
    }

    private void btnExecute_OnClick(object sender, EventArgs e)
    {
        if (OnSQLExecute == null) {
            return;
        }

        OnSQLExecute(tCmd.Text);
    }
}
