using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Platinium
{
    public partial class UserControlModule : UserControl
    {
        private static UserControlModule _instance { get; set; }
        public static UserControlModule Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new UserControlModule();
                return _instance;
            }
        }
        public UserControlModule()
        {
            InitializeComponent();
        }
    }
}
