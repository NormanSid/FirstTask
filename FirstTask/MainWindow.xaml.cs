using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace FirstTask
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Списки данных. ObservableCollection уведомит ListView/ListBox при изменении содержимого
        public ObservableCollection<Hairstyle> HairStyles { get; } = new ObservableCollection<Hairstyle>();
        public ObservableCollection<Stylist> Stylists { get; } = new ObservableCollection<Stylist>();
        public ObservableCollection<string> AvailableTimes { get; } = new ObservableCollection<string>();

        // Обычный List для хранения записей. UI не требует от него автообновления, поэтому List быстрее и проще
        public List<Booking> Bookings { get; } = new List<Booking>();

        //Свойство для хранения выбора пользователя. Привязаны к XAML через {Binding...}
        public Hairstyle SelectedHairstyle { get; set; }
        public Stylist SelectedStylist { get; set; }
        public DateTime? SelectedDate { get; set; } // ? разрешает null, т.к. при запуске дата не выбрана
        public string SelectedTime { get; set; }

        // Флаг направления сортировки. true = по возрастанию, false = по убыванию
        private bool sort = true;
        public MainWindow()
        {
            InitializeComponent();
            //Указываем, что источником всех привязок {Binding} в этом окне будет сам объект this (привязываем окно как источник данных для всех binding в XAML
            DataContext = this;

            // Создаем объекты с указанием типа
            HairStyles.Add(new Hairstyle() { Name = "Мужская стрижка", Price = 800 });
            HairStyles.Add(new Hairstyle() { Name = "Женская стрижка", Price = 800 });
            HairStyles.Add(new Hairstyle() { Name = "Окрашивание", Price = 2500 });
            HairStyles.Add(new Hairstyle() { Name = "Укладка", Price = 600 });

            Stylists.Add(new Stylist() { Name = "Анна" });
            Stylists.Add(new Stylist() { Name = "Дмитрий" });
            Stylists.Add(new Stylist() { Name = "Елена" });
            UpdateTimes();
        }
        // Пересчёт доступных часов
        private void UpdateTimes()
        {
            AvailableTimes.Clear(); //удаляем старые значения
            if (SelectedStylist == null || SelectedDate == null) // Защита от null
                return;
            // Генерация слотов с 10:00 до 17:00
            for (int h = 10; h < 18; h++)
            {
                string time = $"{h:00}:00"; // Интерполяция строк
                // Проверяем занятость через LINQ Any()
                bool isBusy = Bookings.Any(b => b.Stylist.Name == SelectedStylist.Name && b.Date.Date == SelectedDate.Value.Date && b.Time == time);
                if (!isBusy) AvailableTimes.Add(time);
            }
        }
        // Кнопка сортировки
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            sort = !sort; // меняем направления на противоположное

            // LINQ OrderBy сортирует и создает временный List
            var sorted = sort ? HairStyles.OrderBy(h => h.Price).ToList() : HairStyles.OrderByDescending(h => h.Price).ToList();

            // Очищаем ObservableCollection -> UI автоматически удаляет строки
            HairStyles.Clear();
            // Добавляем в новом порядке -> UI автоматически отрисовывает заново
            foreach (var h in sorted) HairStyles.Add(h);
        }

        // События срабатывает при изменении выбора в ComboBox и Calendar
        private void Calendar_SelectedDatesChanged(object sender, SelectionChangedEventArgs e) => UpdateTimes();
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdateTimes();
        
        // Кнопка "записаться"
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Простая валидаця обязательных полей
            if (SelectedStylist == null || SelectedDate == null || string.IsNullOrEmpty(SelectedTime))
            {
                MessageBox.Show("Пожалуйста, выберите мастера, дату и время", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Создаем запись
            Bookings.Add(new Booking()
            {
                Stylist = SelectedStylist,
                Date = SelectedDate.Value, // .Value безопасен после проврки на null
                Time = SelectedTime,
                Hairstyle = SelectedHairstyle
            });
            UpdateTimes(); // Занятое время автоматически исчезнет из ListBox
            // Формируем сообщение
            string msg = $"запись создана!\nМастер: {SelectedStylist.Name}\nДата: {SelectedDate.Value.ToShortDateString()}\nВремя: {SelectedDate}\n";
            msg += SelectedHairstyle != null ? $"Прическа: {SelectedHairstyle.Name} ({SelectedHairstyle.Price} Р)" : "Прическа: Не выбрана";
            MessageBox.Show(msg, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    // Классы. public-свойства обязательны для работы привязок binding
    public class Hairstyle { public string Name { get; set; } public int Price { get; set; } }
    public class Stylist { public string Name { get; set; } }
    public class Booking { public Stylist Stylist { get; set; } public DateTime Date { get; set; } public string Time { get; set; } public Hairstyle Hairstyle { get; set; } }
}
