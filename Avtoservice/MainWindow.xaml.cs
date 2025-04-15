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

namespace Avtoservice
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            frame.Content =  new Page1(frame);
        }

        private void frame_LoadCompleted(object sender, NavigationEventArgs e)
        {

        }
        public class helper
        {
            public static AvtoservesEntities1 ent;
            public static AvtoservesEntities1 GetContext()
            {
                if (ent == null)
                {
                    ent = new AvtoservesEntities1();
                }
                return ent;
            }
        }

    }
}
