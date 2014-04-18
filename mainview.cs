using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

public class MainView : Form
{
    Button btnConnect = null;
    Button btnExecute = null;
    Label lError = null;
    TextBox tCon = null;
    TextBox tCmd = null;
    DataGridView g = null;

    public delegate void SQLExecuteHandler(string command);
    public event SQLExecuteHandler OnSQLExecute = null;

    public delegate void SQLConnectHandler(string command);
    public event SQLConnectHandler OnSQLConnect = null;

    public MainView()
    {
        tCon = new TextBox();
        tCon.Text = "Server=localhost;" +
            "Database=cs320_project2;" +
            "User ID=cs320;" +
            "Password=aiN3xei3;" +
            "Pooling=false";
        tCon.Top = 0;
        tCon.Width = this.Width;
        lError.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
        Controls.Add(tCon);

        btnConnect = new Button();
        btnConnect.Text = "Set connection string";
        btnConnect.Width += 100;
        btnConnect.Top = tCon.Bottom;
        btnConnect.Click += new EventHandler(btnExecute_OnClick);
        Controls.Add(btnConnect);

        tCmd = new TextBox();
        tCmd.Text = "SHOW DATABASES;";
        tCmd.Top = lError.Bottom;
        Controls.Add(tCmd);

        btnExecute = new Button();
        btnExecute.Text = "Execute";
        btnExecute.Click += new EventHandler(btnExecute_OnClick);
        Controls.Add(btnExecute);

        lError = new Label();
        lError.Top = btnExecute.Bottom;
        lError.Width = this.Width;
        lError.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
        lError.Text = "";
        Controls.Add(lError);

        
        g = new DataGridView();
        g.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
        g.Top = tCmd.Bottom;
        g.Width = this.Width;
        g.Height = this.Height - g.Top;
        g.AutoGenerateColumns = true;
        Controls.Add(g);
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
            t.Text = value;
        }
    }

    private void btnConnect_OnClick(object sender, EventArgs e)
    {
        if (OnSQLConnect == null) {
            return;
        }
        OnSQLConnect(tCon.Text);
    }

    private void btnExecute_OnClick(object sender, EventArgs e)
    {
        if (OnSQLExecute == null) {
            return;
        }

        OnSQLExecute(tCmd.Text);
    }
}
