using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinDynamicDesktop
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LocationIQService service = new LocationIQService();
            LocationIQPlace place = service.getLocationData(textBox1.Text);
            MessageBox.Show(place.lat.ToString() + place.lon.ToString());
        }
    }
}
