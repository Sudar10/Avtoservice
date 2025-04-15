using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Runtime.Remoting.Messaging;
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
using static Avtoservice.MainWindow;

namespace Avtoservice
{
    /// <summary>
    /// Логика взаимодействия для Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
        private int recordsToShow = 10;
        //public string iag = "";
        private int currentPage = 0; // Текущая страница
        private int totalRecords; // Общее количество записей
        public string fnd = "";
        public string iag = "";
        private string searchQuery = "";
        private string sortBy = "Фамилия";
        private bool sortAscending = true;
        private List<Client> listclient;
        private Frame fr = new Frame();
        public Page1(Frame frame)
        {
            InitializeComponent();
            List<Gender> genders = new List<Gender> { };
            listclient = helper.GetContext().Client.ToList();
            genders = helper.GetContext().Gender.ToList();
            genders.Add(new Gender { Name = "Все типы" });
            GenderBox.ItemsSource = genders.OrderBy(Gender => Gender.Code);
            SortBox.Items.Add("Без сортировки");
            SortBox.Items.Add("Фамилия");
            SortBox.Items.Add("Дата последнего посещения");
            SortBox.Items.Add("Количество посещений");
            SortBox.SelectedIndex = 0;
            BirthCB.Items.Add("Без Фильтра");
            BirthCB.Items.Add("ДР в этом месяце");
            BirthCB.SelectedIndex = 0;
            Load();
        }
        public void Load()
        {
            var clientsQuery = listclient.AsQueryable();
            var clientVisits = helper.GetContext().ClientService.ToList();
            var lastVisitDict = clientVisits.GroupBy(cs => cs.ClientID).ToDictionary(g => g.Key, g => g.OrderByDescending(cs => cs.StartTime).FirstOrDefault()?.StartTime);
            var visitCountDict = clientVisits.GroupBy(cs => cs.ClientID).ToDictionary(g => g.Key, g => g.Count());

            foreach (var client in listclient)
            {
                if (lastVisitDict.TryGetValue(client.ID, out DateTime? lastVisit))
                {
                    client.LastVisits = lastVisit.HasValue ? lastVisit.Value.ToString("g") : "Ещё не посещал";
                }
                else
                {
                    client.LastVisits = "Ещё не посещал"; // Если нет посещений, устанавливаем сообщение
                }
            }
            foreach (var clientes in listclient)
            {
                if (visitCountDict.TryGetValue(clientes.ID, out int visitCount))
                {
                    clientes.CountVisit = visitCount; // Устанавливаем количество посещений
                }
                else
                {
                    clientes.CountVisit = 0; // Если нет посещений, устанавливаем 0
                }
            }
            // Фильтруем по гендеру, если выбранный элемент не "Все типы"
            if (GenderBox.SelectedItem is Gender selectedGender && selectedGender.Name != "Все типы")
            {
                clientsQuery = clientsQuery.Where(client => client.GenderCode == selectedGender.Code);
            }
           
            
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {  
                clientsQuery = clientsQuery.Where(Client => 
                Client.FirstName.Contains(searchQuery) || 
                Client.LastName.Contains(searchQuery) ||
                Client.Patronymic.Contains(searchQuery) ||
                Client.Email.Contains(searchQuery) ||
                Client.Phone.Contains(searchQuery));
    
            } 
            totalRecords = clientsQuery.Count(); // Получаем общее количество записей

            var currentRecords = clientsQuery.OrderBy(Client => Client.ID)
                                      .Skip(currentPage * recordsToShow)
                                      .Take(recordsToShow)
                                      .ToList();
            if (BirthCB.SelectedItem != null && BirthCB.SelectedItem == "ДР в этом месяце")
            {
                var currentMonth = DateTime.Now.Month;
                clientsQuery = clientsQuery.Where(c => c.Birthday.HasValue && c.Birthday.Value.Month == currentMonth);
            }

            switch (sortBy)
            {
                case "Фамилия":
                    clientsQuery = sortAscending ? clientsQuery.OrderBy(Client => Client.FirstName)
                        : clientsQuery.OrderByDescending(c => c.LastName);
                    break;
                case "Без сортировки":

                    break;
                case "Дата последнего посещения":
                    clientsQuery = sortAscending
                        ? clientsQuery.OrderBy(c => c.LastVisits)
                        : clientsQuery.OrderByDescending(c => c.LastVisits);
                    break;
                case "Количество посещений":
                    clientsQuery = sortAscending
                        ? clientsQuery.OrderBy(c => c.CountVisit)
                        : clientsQuery.OrderByDescending(c => c.CountVisit);
                    break;
            }
            
            clientGrid.ItemsSource = clientsQuery.ToList();
           
            // Обновляем текст Label с количеством записей
            RecordCountLabel.Content = $"{(currentPage * recordsToShow) + currentRecords.Count} из {totalRecords}";
            var currentDiplay  = clientsQuery.Skip((currentPage - 1)  * recordsToShow).Take(recordsToShow).ToList();
            clientGrid.ItemsSource = currentDiplay;
            UpdateNavigationButtons();
        }

        private void UpdateNavigationButtons()
        {
            ButtonPrevious.IsEnabled = currentPage > 0; 
            ButtonNext.IsEnabled = (currentPage + 1) * recordsToShow < totalRecords; 
        }

        private void ButtonPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (currentPage > 0)
            {
                currentPage--;
                Load(); 
            }
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            if ((currentPage + 1) * recordsToShow < totalRecords)
            {
                currentPage++;
                Load(); 
            }
        }

        private void Button10_Click(object sender, RoutedEventArgs e)
        {
            recordsToShow = 10;
            currentPage = 0;
            Load();
        }

        private void Button50_Click(object sender, RoutedEventArgs e)
        {
            recordsToShow = 50;
            currentPage = 0;
            Load();
        }

        private void Button200_Click(object sender, RoutedEventArgs e)
        {
            recordsToShow = 200;
            currentPage = 0; 
            Load();
        }

        private void ButtonAll_Click(object sender, RoutedEventArgs e)
        {
            recordsToShow =helper.GetContext().Client.Count(); 
            currentPage = 0;
            Load();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            searchQuery = SearchBox.Text;
            currentPage = 0;
            Load();
        }


        private void SelectedCombox(object sender, SelectionChangedEventArgs e)
        {
            iag = ((Gender)GenderBox.SelectedItem).Code;
            Load();
        }

        private void SortBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SortBox.SelectedItem != null)
            {
                sortBy = SortBox.SelectedItem.ToString();
                currentPage = 0;
                Load();
            }
        }
        private void btnList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Client client = clientGrid.SelectedItems[0] as Client;
               ClientVisit pageadd = new ClientVisit(client);
                NavigationService.Navigate(pageadd);
            }
            catch (Exception ex) { MessageBox.Show("Пользователь не был выбран"); }

        }

        private void AddClients_Click(object sender, RoutedEventArgs e)
        {
            PageEdit pageadd = new PageEdit(null);
            NavigationService.Navigate(pageadd);
        }

        private void clientGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PageEdit pageEdit = new PageEdit((Client)clientGrid.SelectedItem);
            NavigationService.Navigate(pageEdit);
        }

        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            if (clientGrid.SelectedItems.Count > 0)
            {
                Client client = clientGrid.SelectedItems[0] as Client;

                if (client != null)
                {
                    var context = helper.GetContext();

                    // Проверяем наличие связанных тегов
                    var relatedTags = context.TagOfClient.Where(toc => toc.ClientID == client.ID).ToList();

                    if (relatedTags.Any())
                    {
                        // Если есть связанные теги, выводим сообщение и удаляем их
                        var result = MessageBox.Show("У данного пользователя есть привязанные теги. Вы уверены, что хотите удалить пользователя и все связанные теги?",
                                                      "Подтверждение удаления",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Warning);

                        if (result == MessageBoxResult.Yes)
                        {
                            // Удаляем связанные теги
                            context.TagOfClient.RemoveRange(relatedTags);
                        }
                        else
                        {
                            return; // Если пользователь отменил, выходим из метода
                        }
                    }

                    // Удаляем клиента
                    context.Client.Remove(client);
                    context.SaveChanges();

                    // Обновляем коллекцию в интерфейсе
                    var clientsList = clientGrid.ItemsSource as ObservableCollection<Client>;
                    if (clientsList != null)
                    {
                        clientsList.Remove(client); // Удаляем клиента из коллекции
                    }

                    MessageBox.Show("Удаление успешно выполнено");
                    Load();
                }
            }
        }

        private void CB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Load();
        }
    }

}

