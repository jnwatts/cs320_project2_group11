using System;
using System.Drawing;
using System.Windows.Forms;

public class MainApp : Form
{
    public static void Main()
    {
        Application.Run(new MainApp());
    }

    public MainApp()
    {
        Button b = new Button();
        b.Width += 50;
        b.Text = "Hello, World?";
        b.Click += new EventHandler(Button_Click);
        Controls.Add(b);
    }

    private void Button_Click(object sender, EventArgs e)
    {
        MessageBox.Show("Hello, World!");
    }
}
