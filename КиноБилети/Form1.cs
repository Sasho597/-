using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TheaterTicketSystem
{
    // Използваме чисто име на класа, съвпадащо с името на файла
    public class Form1 : Form
    {
        private System.ComponentModel.IContainer components = null;

        // Конфигурация на залата
        private const int ROWS = 8;
        private const int COLS = 10;
        private const int TICKET_PRICE = 12; // Цена на билет в лв.

        // UI Елементи
        private ComboBox cmbMovies;
        private ComboBox cmbDates;
        private ComboBox cmbTimes;
        private Label lblTotal;
        private Button btnBuy;
        private Panel pnlSeats;

        // Матрица за местата и списък за избраните
        private Button[,] seatButtons = new Button[ROWS, COLS];
        private List<Point> selectedSeats = new List<Point>();

        // База данни в оперативната памет за заетите места
        private Dictionary<string, HashSet<string>> bookedSeatsDatabase = new Dictionary<string, HashSet<string>>();

        public Form1()
        {
            // Основни настройки на формата
            this.ClientSize = new Size(820, 600);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Система за резервация на билети за театър";
            this.BackColor = Color.FromArgb(240, 240, 243);

            // Ръчно изграждане на интерфейса софтуерно
            BuildUserInterface();

            // Зареждане на първоначални данни
            LoadDefaultData();
            UpdateSeatGrid();
        }

        private void BuildUserInterface()
        {
            // Падащо меню за постановки
            Label lblMovie = new Label { Text = "Изберете постановка:", Location = new Point(30, 20), Width = 220, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            cmbMovies = new ComboBox { Location = new Point(30, 45), Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            cmbMovies.SelectedIndexChanged += new EventHandler(this.Projection_Changed);

            // Падащо меню за дата
            Label lblDate = new Label { Text = "Дата:", Location = new Point(270, 20), Width = 80, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            cmbDates = new ComboBox { Location = new Point(270, 45), Width = 130, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            cmbDates.SelectedIndexChanged += new EventHandler(this.Projection_Changed);

            // Падащо меню за час
            Label lblTime = new Label { Text = "Час:", Location = new Point(420, 20), Width = 80, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            cmbTimes = new ComboBox { Location = new Point(420, 45), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9) };
            cmbTimes.SelectedIndexChanged += new EventHandler(this.Projection_Changed);

            // Сцена на театъра
            Label lblScreen = new Label
            {
                Text = "С  Ц  Е  Н  А",
                Location = new Point(30, 100),
                Width = 545,
                Height = 25,
                BackColor = Color.DarkGray,
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            // Панел, в който се подреждат бутоните за местата
            pnlSeats = new Panel { Location = new Point(30, 140), Width = 545, Height = 340 };

            int btnWidth = 45;
            int btnHeight = 35;
            int spacing = 8;

            for (int r = 0; r < ROWS; r++)
            {
                for (int c = 0; c < COLS; c++)
                {
                    Button btn = new Button();
                    btn.Size = new Size(btnWidth, btnHeight);
                    btn.Location = new Point(c * (btnWidth + spacing), r * (btnHeight + spacing));
                    btn.Text = $"{r + 1}-{c + 1}";
                    btn.Font = new Font("Segoe UI", 7.5f, FontStyle.Regular);
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 1;
                    btn.Tag = new Point(r, c);
                    btn.Click += new EventHandler(this.Seat_Click);

                    seatButtons[r, c] = btn;
                    pnlSeats.Controls.Add(btn);
                }
            }

            // Легенда за цветовете
            GroupBox grpLegend = new GroupBox { Text = "Легенда", Location = new Point(600, 135), Width = 190, Height = 130, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            Panel pnlFree = new Panel { BackColor = Color.MediumSeaGreen, Location = new Point(15, 30), Size = new Size(20, 20) };
            Label lblFree = new Label { Text = "Свободно", Location = new Point(45, 32), Font = new Font("Segoe UI", 9, FontStyle.Regular) };
            Panel pnlSelected = new Panel { BackColor = Color.Orange, Location = new Point(15, 60), Size = new Size(20, 20) };
            Label lblSelected = new Label { Text = "Избрано", Location = new Point(45, 62), Font = new Font("Segoe UI", 9, FontStyle.Regular) };
            Panel pnlTaken = new Panel { BackColor = Color.Crimson, Location = new Point(15, 90), Size = new Size(20, 20) };
            Label lblTaken = new Label { Text = "Заето", Location = new Point(45, 92), Font = new Font("Segoe UI", 9, FontStyle.Regular) };
            grpLegend.Controls.AddRange(new Control[] { pnlFree, lblFree, pnlSelected, lblSelected, pnlTaken, lblTaken });

            // Текст за обща сума
            lblTotal = new Label
            {
                Text = "Избрани места: 0 | Обща сума: 0 лв.",
                Location = new Point(30, 510),
                Width = 540,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(45, 45, 48)
            };

            // Бутон за купуване
            btnBuy = new Button
            {
                Text = "КУПИ БИЛЕТИ",
                Location = new Point(600, 495),
                Width = 190,
                Height = 45,
                BackColor = Color.DodgerBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Enabled = false
            };
            btnBuy.FlatAppearance.BorderSize = 0;
            btnBuy.Click += new EventHandler(this.btnBuy_Click);

            // Добавяне на компонентите към формата
            this.Controls.AddRange(new Control[] {
                lblMovie, cmbMovies, lblDate, cmbDates, lblTime, cmbTimes,
                lblScreen, pnlSeats, grpLegend, lblTotal, btnBuy
            });
        }

        private void LoadDefaultData()
        {
            cmbMovies.Items.AddRange(new string[] { "Хамлет", "Ромео и Жулиета", "Дон Кихот", "Женитба" });
            cmbMovies.SelectedIndex = 0;

            cmbDates.Items.AddRange(new string[] { "Днес (" + DateTime.Now.ToString("dd.MM") + ")", "Утре (" + DateTime.Now.AddDays(1).ToString("dd.MM") + ")" });
            cmbDates.SelectedIndex = 0;

            cmbTimes.Items.AddRange(new string[] { "16:00", "19:00", "21:00" });
            cmbTimes.SelectedIndex = 1;
        }

        private string GetCurrentProjectionKey()
        {
            string movie = cmbMovies.SelectedItem?.ToString() ?? "";
            string date = cmbDates.SelectedItem?.ToString() ?? "";
            string time = cmbTimes.SelectedItem?.ToString() ?? "";
            return $"{movie}_{date}_{time}";
        }

        private void UpdateSeatGrid()
        {
            selectedSeats.Clear();
            UpdateTotalLabel();

            string currentKey = GetCurrentProjectionKey();

            if (!bookedSeatsDatabase.ContainsKey(currentKey))
            {
                bookedSeatsDatabase[currentKey] = new HashSet<string>();
                Random rand = new Random(currentKey.GetHashCode());
                for (int i = 0; i < 8; i++)
                {
                    bookedSeatsDatabase[currentKey].Add($"{rand.Next(ROWS)}_{rand.Next(COLS)}");
                }
            }

            HashSet<string> takenSeats = bookedSeatsDatabase[currentKey];

            for (int r = 0; r < ROWS; r++)
            {
                for (int c = 0; c < COLS; c++)
                {
                    string seatKey = $"{r}_{c}";
                    Button btn = seatButtons[r, c];

                    if (takenSeats.Contains(seatKey))
                    {
                        btn.BackColor = Color.Crimson;
                        btn.Enabled = false;
                    }
                    else
                    {
                        btn.BackColor = Color.MediumSeaGreen;
                        btn.Enabled = true;
                    }
                }
            }
        }

        private void Seat_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            Point pos = (Point)btn.Tag;

            if (btn.BackColor == Color.MediumSeaGreen)
            {
                btn.BackColor = Color.Orange;
                selectedSeats.Add(pos);
            }
            else if (btn.BackColor == Color.Orange)
            {
                btn.BackColor = Color.MediumSeaGreen;
                selectedSeats.Remove(pos);
            }

            UpdateTotalLabel();
        }

        private void UpdateTotalLabel()
        {
            int total = selectedSeats.Count * TICKET_PRICE;
            lblTotal.Text = $"Избрани места: {selectedSeats.Count} | Обща сума: {total} лв.";
            btnBuy.Enabled = selectedSeats.Count > 0;
        }
        private void Projection_Changed(object sender, EventArgs e)
        {
            UpdateSeatGrid();
        }
        private void btnBuy_Click(object sender, EventArgs e)
        {
            if (selectedSeats.Count == 0) return;
            string currentKey = GetCurrentProjectionKey();
            string movie = cmbMovies.SelectedItem.ToString();
            string date = cmbDates.SelectedItem.ToString();
            string time = cmbTimes.SelectedItem.ToString();
            string ticketInfo = "========================================\n";
            ticketInfo += "         ТЕАТРАЛЕН БИЛЕТ / КАСА         \n";
            ticketInfo += "========================================\n";
            ticketInfo += $"Постановка:     {movie}\n";
            ticketInfo += $"Дата:           {date}\n";
            ticketInfo += $"Час:            {time}\n";
            ticketInfo += "----------------------------------------\n";
            ticketInfo += "Места (Ред - Място):\n";
            foreach (Point seat in selectedSeats)
            {
                string seatKey = $"{seat.X}_{seat.Y}";
                bookedSeatsDatabase[currentKey].Add(seatKey);
                ticketInfo += $" - Ред {seat.X + 1}, Място {seat.Y + 1}\n";
            }
            int totalAmount = selectedSeats.Count * TICKET_PRICE;
            ticketInfo += "----------------------------------------\n";
            ticketInfo += $"Обща сума:       {totalAmount} лв.\n";
            ticketInfo += $"Издаден на:      {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}\n";
            ticketInfo += "========================================\n";
            string fileName = $"Ticket_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
            try
            {
                File.WriteAllText(fileName, ticketInfo);
                MessageBox.Show($"Успешна покупка!\nБилетът е записан във файл: {fileName}", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Грешка при запис: {ex.Message}", "Грешка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            UpdateSeatGrid();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
