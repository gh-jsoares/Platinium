using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Platinium
{
    /// <summary>
    /// Interaction logic for UserControlModule.xaml
    /// </summary>
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
