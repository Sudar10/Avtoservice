﻿
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
    /// Логика взаимодействия для ClientVisit.xaml
    /// </summary>
    public partial class ClientVisit : Page
    {
        private Client client;
        public ClientVisit(Client _client)
        {
            InitializeComponent();
            client = _client;
            Load();
        }
        public void Load()
        {

            dataGridInfClient.ItemsSource = client.ClientService.ToList();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();

        }
    }
}
