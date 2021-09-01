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
using System.Windows.Shapes;

//in progress
//TODO
namespace LocalDatabase_Client.ProgressBar
{
    public partial class ProgressBar : Window
    {
        public int progress { set; get; }
        public ProgressBar()
        {
            progress = 0;
            InitializeComponent();
            Task t1 = new Task(() => Start());
            t1.Start();
        }
        private void Start()
        {
            while(progress < 100)
            {
                Application.Current.Dispatcher.Invoke(new Action(() => { pbStatus.Value = progress; }));
            }
        }
    }
}
