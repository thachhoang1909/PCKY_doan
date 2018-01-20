using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CKYthuattoan
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            string appPath = System.IO.Path.GetDirectoryName(Application.ExecutablePath);
            string fileName = appPath + @"\output.png";
            Image myImage = Image.FromFile(fileName);
            pictureBox1.Image = myImage;
        }
        private void Form2_Load(object sender, EventArgs e)
        {
            
        }

    }
}
