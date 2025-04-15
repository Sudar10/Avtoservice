using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Логика взаимодействия для PageEdit.xaml
    /// </summary>
    public partial class PageEdit : Page
    {

        Client cl = new Client();
        List<string> tags = new List<string>();
        public PageEdit(Client client)
        {
            Gender gender = new Gender();
            InitializeComponent();
            var allTags = helper.GetContext().Tag.ToList();
            CbGender.ItemsSource = helper.GetContext().Gender.Select(p => p.Name).ToList();
            dataGridTagList.ItemsSource = helper.GetContext().Tag.ToList();
            if (client != null)
            {
                btnEdit.Visibility = Visibility.Visible;
                btnAdd.Visibility = Visibility.Hidden;
                lblID.Visibility = Visibility.Visible;
                tbID.Visibility = Visibility.Visible;
                CbGender.SelectedItem = gender.Name;
                this.tbID.Text = client.ID.ToString();
                this.tbName.Text = client.FirstName;
                this.tbSurname.Text = client.LastName;
                this.tbPatronymic.Text = client.Patronymic;
                this.dpBirthday.Text = client.Birthday.ToString();
                this.tbEmail.Text = client.Email;
                this.tbPhone.Text = client.Phone;
                var pathAppClientImage = "/Images/";
                var imagePath = pathAppClientImage + client.PhotoPath;
                BitmapImage bitmap = new BitmapImage(new Uri(imagePath, UriKind.Relative));
                this.ImageClient.Source = bitmap;
                CbGender.SelectedIndex = CbGender.Items.IndexOf(client.Gender.Name);
                cl = helper.GetContext().Client.Find(client.ID);

                TagOfClient tags = new TagOfClient();
                var clientTags = helper.GetContext().TagOfClient.Where(tc => tc.ClientID == client.ID).Select(tc => tc.Tag).ToList();
                foreach (var tag in clientTags)
                {
                    dataGridTagClient.Items.Add(tag);
                }
            }
            else
            {
                btnEdit.Visibility = Visibility.Hidden;
                btnAdd.Visibility = Visibility.Visible;
                lblID.Visibility = Visibility.Hidden;
                tbID.Visibility = Visibility.Hidden;
                btnAddTag.Visibility = Visibility.Hidden;
            }
        }
        private void tbName_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex(@"^[а-яА-ЯёЁa-zA-Zs-]*$");
            e.Handled = !regex.IsMatch(e.Text) || (tbName.Text.Length >= 50);
        }

        private void tbPhone_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                Regex reg = new Regex("[^0-9]+");
                e.Handled = reg.IsMatch(e.Text);
            }
            catch (Exception ex) { MessageBox.Show("Перепроверьте данные и введите их корректно в виде 8-(961)-888-88-88"); };
        }
        private void tbEmail_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var emailRegex = new Regex(@"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$");

            e.Handled = emailRegex.IsMatch(e.Text);
        }
        private void tbSurname_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex(@"^[а-яА-ЯёЁa-zA-Zs-]*$");
            e.Handled = !regex.IsMatch(e.Text) || (tbSurname.Text.Length >= 50);
        }

        private void tbPatronymic_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex(@"^[а-яА-ЯёЁa-zA-Zs-]*$");
            e.Handled = !regex.IsMatch(e.Text) || (tbPatronymic.Text.Length >= 50);
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<string> errors = new List<string>();

                if (string.IsNullOrWhiteSpace(tbName.Text))
                    errors.Add("Имя не может быть пустым.");

                if (string.IsNullOrWhiteSpace(tbSurname.Text))
                    errors.Add("Фамилия не может быть пустой.");

                if (!dpBirthday.SelectedDate.HasValue)
                    errors.Add("Дата рождения не выбрана.");

                if (string.IsNullOrWhiteSpace(tbEmail.Text))
                    errors.Add("Некорректный email.");

                if (string.IsNullOrWhiteSpace(tbPhone.Text))
                    errors.Add("Телефон не может быть пустым.");

                if (CbGender.SelectedIndex < 0)
                    errors.Add("Пол не выбран.");

                // Если есть ошибки, выводим их пользователю
                if (errors.Count > 0)
                {
                    MessageBox.Show(string.Join("\n", errors), "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                    return; // Прерываем выполнение, если есть ошибки
                }

                Client client = new Client();
                if (CbGender.SelectedIndex == 1)
                {
                    client.GenderCode = "м";

                }
                else
                {
                    client.GenderCode = "ж";
                }
                client.FirstName = tbName.Text;
                client.LastName = tbSurname.Text;
                client.Patronymic = tbPatronymic.Text;
                client.Birthday = dpBirthday.SelectedDate.Value;
                client.Email = tbEmail.Text;
                client.Phone = tbPhone.Text;
                client.RegistrationDate = DateTime.Now;
                if (cl.PhotoPath == null)
                    client.PhotoPath = null;
                else
                    client.PhotoPath = "client/" + cl.PhotoPath.Trim();

                helper.GetContext().Client.Add(client);
                helper.GetContext().SaveChanges();
                MessageBox.Show("Пользователь успешно добавлен");
                int clientId = client.ID;

                foreach (var item in dataGridTagClient.Items)
                {
                    var tag = item as Tag;

                    if (tag != null)
                    {
                        // Получаем ID тега из базы данных
                        int? tagId = helper.GetContext().Tag
                            .Where(tc => tc.Title == tag.Title)
                            .Select(tc => (int?)tc.ID) // Приводим к int? для обработки возможного отсутствия
                            .FirstOrDefault();

                        if (tagId.HasValue) // Проверяем, что тег найден
                        {
                            // Проверяем, существует ли уже такая запись в TagOfClient
                            bool exists = helper.GetContext().TagOfClient
                                .Any(toc => toc.ClientID == clientId && toc.TagID == tagId.Value);

                            if (exists)
                            {
                            }
                            else
                            {
                                // Создаем новую запись в таблице TagOfClient
                                var tagOfClient = new TagOfClient
                                {
                                    ClientID = clientId,
                                    TagID = tagId.Value // Используем значение ID
                                };

                                // Добавляем запись в контекст и сохраняем изменения
                                helper.GetContext().TagOfClient.Add(tagOfClient);
                                helper.GetContext().SaveChanges();

                                // Выводим сообщение об успешном добавлении
                                MessageBox.Show($"Тег '{tag.Title}' успешно добавлен для клиента с ID {clientId}.");
                            }
                        }
                        else
                        {
                            MessageBox.Show($"Тег '{tag.Title}' не найден в базе данных.");
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<string> errors = new List<string>();

                if (string.IsNullOrWhiteSpace(tbName.Text))
                    errors.Add("Имя не может быть пустым.");

                if (string.IsNullOrWhiteSpace(tbSurname.Text))
                    errors.Add("Фамилия не может быть пустой.");

                if (!dpBirthday.SelectedDate.HasValue)
                    errors.Add("Дата рождения не выбрана.");

                if (string.IsNullOrWhiteSpace(tbEmail.Text))
                    errors.Add("Некорректный email.");

                if (string.IsNullOrWhiteSpace(tbPhone.Text))
                    errors.Add("Телефон не может быть пустым.");

                
                // Если есть ошибки, выводим их пользователю
                if (errors.Count > 0)
                {
                    MessageBox.Show(string.Join("\n", errors), "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Error);
                    return; // Прерываем выполнение, если есть ошибки
                }

                cl.GenderCode = CbGender.SelectedIndex.ToString();
                cl.FirstName = tbName.Text;
                cl.LastName = tbSurname.Text;
                cl.Patronymic = tbPatronymic.Text;
                cl.Birthday = dpBirthday.SelectedDate.Value;
                cl.Email = tbEmail.Text;
                cl.Phone = tbPhone.Text;
                cl.PhotoPath = "client/" + cl.PhotoPath.Trim();

                helper.GetContext().Entry(cl).State = EntityState.Modified;
                helper.GetContext().SaveChanges();
                MessageBox.Show("Пользователь успешно отредактирован");

                int clientId = cl.ID;

                foreach (var item in dataGridTagClient.Items)
                {
                    var tag = item as Tag;

                    if (tag != null)
                    {
                        // Получаем ID тега из базы данных
                        int? tagId = helper.GetContext().Tag
                            .Where(tc => tc.Title == tag.Title)
                            .Select(tc => (int?)tc.ID) // Приводим к int? для обработки возможного отсутствия
                            .FirstOrDefault();

                        if (tagId.HasValue) // Проверяем, что тег найден
                        {
                            // Проверяем, существует ли уже такая запись в TagOfClient
                            bool exists = helper.GetContext().TagOfClient
                                .Any(toc => toc.ClientID == clientId && toc.TagID == tagId.Value);

                            if (exists)
                            {
                            }
                            else
                            {
                                // Создаем новую запись в таблице TagOfClient
                                var tagOfClient = new TagOfClient
                                {
                                    ClientID = clientId,
                                    TagID = tagId.Value // Используем значение ID
                                };

                                // Добавляем запись в контекст и сохраняем изменения
                                helper.GetContext().TagOfClient.Add(tagOfClient);
                                helper.GetContext().SaveChanges();

                                // Выводим сообщение об успешном добавлении
                                MessageBox.Show($"Тег '{tag.Title}' успешно добавлен для клиента с ID {clientId}.");
                            }
                        }
                        else
                        {
                            MessageBox.Show($"Тег '{tag.Title}' не найден в базе данных.");
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void btnAddTag_OnClick(object sender, RoutedEventArgs e)
        {

        }

        private void btnBase_OnClickDownloadImage(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp"; // Фильтр по типам изображений

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                string filePath = openFileDialog.FileName;

                // Проверяем размер файла (не более 2 МБ)
                FileInfo fileInfo = new FileInfo(filePath);
                if (fileInfo.Length > 2 * 1024 * 1024) // 2 МБ
                {
                    MessageBox.Show("Размер изображения не должен превышать 2 МБ.");
                    return;
                }

                // Загружаем изображение в ImageControl
                BitmapImage bitmap = new BitmapImage(new Uri(filePath));
                this.ImageClient.Source = bitmap;
                cl.PhotoPath = System.IO.Path.GetFileName(filePath);
            }

        }

        private void btnBack_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void btnLeft_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = dataGridTagList.SelectedItems.Cast<Tag>().ToList();

            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите хотя бы один тег для добавления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Выход из метода, если ничего не выбрано
            }
            // Переносим выбранные элементы в dataGridTagClient
            foreach (var item in selectedItems)
            {

                if (!dataGridTagClient.Items.Contains(item))
                {
                    dataGridTagClient.Items.Add(item);
                }
                else
                {
                    // Здесь можно обработать ситуацию, когда элемент уже существует
                    MessageBox.Show($"Выбранный элемент уже существует в списке.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            dataGridTagClient.Items.Refresh();
        }

        private void btnRight_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = dataGridTagClient.SelectedItems.Cast<Tag>().ToList();

            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите хотя бы один тег для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Выход из метода, если ничего не выбрано
            }

            var result = MessageBox.Show("Вы уверены, что хотите удалить тег?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                int clientId = cl.ID;

                foreach (var item in selectedItems)
                {
                    // Удаляем элемент из dataGridTagClient
                    dataGridTagClient.Items.Remove(item);

                    var tag = item as Tag;

                    if (tag != null)
                    {
                        // Удаляем запись из базы данных
                        var tagToRemove = helper.GetContext().TagOfClient
                            .FirstOrDefault(tc => tc.ClientID == clientId && tc.TagID == tag.ID);

                        if (tagToRemove != null)
                        {
                            helper.GetContext().TagOfClient.Remove(tagToRemove);
                            helper.GetContext().SaveChanges();
                        }
                    }
                }

                dataGridTagClient.Items.Refresh(); // Обновляем отображение
            }
        }


    }
}
    

