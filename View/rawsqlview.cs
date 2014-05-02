using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using MySql.Data.MySqlClient;

public class RawSqlView : UserControl
{
    private TableLayoutPanel tlp0;
    private Button btnExecute = null;
    private ComboBox dropDown = null;
    private TextBox tMessage = null;
    private TextBox tCmd = null;
    private DataGridView g = null;

    private ArrayList dropDownList;
    private ArrayList dropDownQueries;

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

        dropDown = new ComboBox();
        dropDown.Width *= 2;

        dropDown = new ComboBox();
        dropDown.Width *= 2;
        populateDropDown();
        populateQueries();
        dropDown.DataSource = dropDownList;
        dropDown.SelectedValueChanged +=
            new EventHandler(dropDown_SelectedValueChanged);
        dropDown.DropDownStyle = ComboBoxStyle.DropDownList;
        tlp0.Controls.Add(dropDown);



        tMessage = new TextBox();
        tMessage.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        tMessage.Text = "";
        tMessage.Multiline = true;
        tMessage.Height *= 4;
        tMessage.ReadOnly = true;
        tlp0.RowCount++;
        tlp0.Controls.Add(tMessage);
        
        g = new DataGridView();
        g.BorderStyle = BorderStyle.None;
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

    public string Message
    {
        set
        {
            tMessage.Text = value;
        }
    }

    public bool OperationFailed
    {
        set
        {
            if (value) {
                tMessage.Font = new Font(tMessage.Font, FontStyle.Bold);
            } else {
                tMessage.Font = new Font(tMessage.Font, FontStyle.Regular);
            }
        }
    }

    private void btnExecute_OnClick(object sender, EventArgs e)
    {
        if (OnSQLExecute == null) {
            return;
        }

        OnSQLExecute(tCmd.Text);
    }

    private void dropDown_SelectedValueChanged(object sender, EventArgs e)
    {
        if (dropDown.SelectedIndex != 0)
        {
            tCmd.Text = (string)dropDownQueries[dropDown.SelectedIndex - 1];
        }
    }

    private void populateDropDown()
    {
        dropDownList = new ArrayList();
        dropDownList.Add("Select a query...");
        dropDownList.Add("Vendor parts w/ mfg and vendor");
        dropDownList.Add("Manufacturers and parts");
        dropDownList.Add("Parts w/ type attributes");
    }

    private void populateQueries()
    {
        dropDownQueries = new ArrayList();
        dropDownQueries.Add("SELECT * FROM Vendor_parts AS VP INNER JOIN Manufacturers AS M ON VP.Manufacturer_id = M.Manufacturer_id INNER JOIN Vendors AS V ON VP.Vendor_id = V.Vendor_id;");
        dropDownQueries.Add("SELECT * FROM Vendor_parts AS VP RIGHT OUTER JOIN Manufacturers AS M ON VP.Manufacturer_id = M.Manufacturer_id;");
        dropDownQueries.Add("SELECT * FROM Parts AS P NATURAL LEFT JOIN Capacitor_attributes AS A NATURAL LEFT JOIN Part_types AS T WHERE T.Type = 'Capacitor'");
    }
}
