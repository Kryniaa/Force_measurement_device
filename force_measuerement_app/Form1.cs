using System.IO.Ports;
using System.Linq.Expressions;
using System.Windows.Forms.DataVisualization.Charting;

namespace force_measuerement_app
{
    /// <summary>
    /// Główna klasa formularza aplikacji. Odpowiada za interfejs użytkownika, 
    /// komunikację przez port szeregowy oraz wizualizację danych z 10-kanałowego czujnika siły.
    /// </summary>
    public partial class Form1 : Form
    {
        /// <summary>
        /// Obiekt obsługujący fizyczne/wirtualne połączenie z portem COM (np. STM32).
        /// Tworzony na nowo przy każdej próbie połączenia (patrz btnConnect_Click) - obiekt
        /// SerialPort potrafi zostać w niespójnym stanie po awaryjnym/nieudanym Close(),
        /// więc bezpieczniej jest zaczynać każde połączenie od świeżej instancji.
        /// Stary obiekt jest ZAWSZE zwalniany przez Dispose() (patrz SafeClosePort), inaczej
        /// sterownik trzyma uchwyt, a w liście portów pojawiają się duplikaty (np. kilka razy COM5).
        /// </summary>
        private SerialPort myPort = new SerialPort();

        /// <summary>
        /// Kanoniczny, jedyny "prawdziwy" stan połączenia w aplikacji. WSZYSTKIE miejsca w kodzie,
        /// które muszą wiedzieć czy jesteśmy połączeni, pytają o TO pole - nigdy bezpośrednio
        /// o myPort.IsOpen ani o tekst przycisku btnConnect.
        /// </summary>
        private bool _isConnected = false;

        /// <summary>
        /// Nazwa portu, z którym jesteśmy aktualnie połączeni. Używana przez detektor
        /// fizycznego odłączenia (CheckIfConnectedPortStillExists), żeby wiedzieć, którego
        /// portu ma szukać na liście dostępnych portów w systemie.
        /// </summary>
        private string? _connectedPortName = null;

        /// <summary>
        /// Tablica przechowująca zbuforowane wartości tarowania dla każdego z 10 kanałów.
        /// Od tych wartości odejmowany jest pomiar bieżący, by uzyskać siłę netto.
        /// </summary>
        private double[] _tareOffsets = new double[10];

        /// <summary>
        /// Tablica przechowująca aktualne, bezwzględne odczyty siły (prosto z ADC, przed uwzględnieniem tary).
        /// Służy do zapamiętania stanu w momencie wciśnięcia przycisku "Tare".
        /// </summary>
        private double[] _currentAbsoluteForces = new double[10];

        /// <summary>
        /// Flaga stanu określająca, czy aplikacja aktualnie przetwarza i rysuje przychodzące dane pomiarowe.
        /// </summary>
        private bool isMeasuring = false;

        /// <summary>
        /// Ustawiana przy starcie pomiaru. Pierwsza odebrana paczka danych zostaje użyta
        /// jako tara, po czym flaga jest kasowana.
        /// </summary>
        private bool _autoTarePending = false;

        /// <summary>
        /// Konstruktor głównego okna aplikacji. Inicjalizuje wygenerowane komponenty WinForms.
        /// </summary>
        public Form1()
        {
            InitializeComponent();

            // Zapewnia bezpieczne zamknięcie portu, gdy użytkownik zamyka okno programu
            // podczas aktywnego połączenia.
            this.FormClosing += Form1_FormClosing;
        }

        // =====================================================================
        // WYKRYWANIE FIZYCZNEGO ODŁĄCZENIA KABLA USB
        // =====================================================================
        // Windows wysyła komunikat WM_DEVICECHANGE do KAŻDEGO okna top-level za każdym razem,
        // gdy zmienia się drzewo urządzeń w systemie (podpięcie/odpięcie czegokolwiek przez USB) -
        // bez potrzeby żadnej dodatkowej rejestracji. Dzięki temu appka wie o wypięciu kabla
        // NATYCHMIAST, zamiast czekać aż SerialPort sam zgłosi błąd przy kolejnej próbie
        // odczytu/zapisu (co, jak widać na screenach, bywa niewiarygodne - port potrafi
        // "wisieć" jako pozornie otwarty, a program i tak zaczyna zgłaszać sprzeczne błędy).

        private const int WM_DEVICECHANGE = 0x0219;
        private const int DBT_DEVNODES_CHANGED = 0x0007;

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_DEVICECHANGE && m.WParam.ToInt32() == DBT_DEVNODES_CHANGED)
            {
                CheckIfConnectedPortStillExists();
            }
        }

        /// <summary>
        /// Zwraca listę portów COM widocznych w systemie: bez duplikatów i posortowaną
        /// naturalnie (COM3, COM5, COM10 zamiast COM10, COM3, COM5).
        /// GetPortNames() czyta wpisy z rejestru, które po awaryjnym odłączeniu kabla
        /// potrafią się dublować - stąd Distinct().
        /// </summary>
        private static string[] GetUniquePortNames()
        {
            return SerialPort.GetPortNames()
                .Distinct()
                .OrderBy(p => p.Length)
                .ThenBy(p => p)
                .ToArray();
        }

        /// <summary>
        /// Wywoływane przy KAŻDEJ zmianie w drzewie urządzeń USB w systemie (nie tylko naszego
        /// czujnika). Sprawdza, czy port, z którym jesteśmy połączeni, nadal żyje.
        /// Sama lista z rejestru (GetPortNames) jest zawodna - stary wpis potrafi tam zostać
        /// po wypięciu kabla - dlatego dodatkowo "dotykamy" portu (CtsHolding), co na
        /// usuniętym urządzeniu rzuca wyjątek.
        /// </summary>
        private void CheckIfConnectedPortStillExists()
        {
            if (!_isConnected || _connectedPortName == null)
                return;

            bool gone = false;
            try
            {
                // Odczyt właściwości pyta sterownik o stan linii - na usuniętym urządzeniu rzuca wyjątek.
                _ = myPort.CtsHolding;
            }
            catch
            {
                gone = true;
            }

            if (gone || !GetUniquePortNames().Contains(_connectedPortName))
            {
                HandleDisconnection("USB cable physically unplugged.", deviceGone: true);
            }
        }

        /// <summary>
        /// Zdarzenie wywoływane jednorazowo podczas uruchamiania okna aplikacji.
        /// Odpowiada za pełną konfigurację wizualną wykresu (tytuły, osie, siatki i formatowanie serii).
        /// </summary>
        private void Form1_Load(object? sender, EventArgs e)
        {
            // Czyszczenie domyślnych ustawień kontrolki Chart
            chartWykres.Series.Clear();
            chartWykres.Titles.Clear();

            // Konfiguracja głównego tytułu wykresu
            chartWykres.Titles.Add("Force measurement");
            chartWykres.Titles[0].Font = new Font("Arial", 14, FontStyle.Bold);

            // Konfiguracja dynamicznego napisu ze średnią wartością (prawy górny róg)
            chartWykres.Titles.Add("Average: 0.00 N");
            chartWykres.Titles[1].Font = new Font("Arial", 12, FontStyle.Bold);
            chartWykres.Titles[1].ForeColor = Color.Red;
            chartWykres.Titles[1].Alignment = ContentAlignment.TopRight;
            chartWykres.Titles[1].Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Top;

            // --- KONFIGURACJA OSI Y (Siła w Niutonach) ---
            var obszar = chartWykres.ChartAreas[0];
            obszar.AxisY.Minimum = 0;
            obszar.AxisY.Maximum = 300;
            obszar.AxisY.Interval = 50; // Skok głównej siatki co 50 N

            // Włączenie i konfiguracja siatki pomocniczej dla większej czytelności (co 25 N)
            obszar.AxisY.MinorGrid.Enabled = true;
            obszar.AxisY.MinorGrid.Interval = 25;
            obszar.AxisY.MinorGrid.LineColor = Color.LightGray;
            obszar.AxisY.MinorGrid.LineDashStyle = ChartDashStyle.Dash;

            obszar.AxisY.Title = "Force [N]";
            obszar.AxisY.TitleFont = new Font("Arial", 10, FontStyle.Bold);

            // Ukrycie pionowych linii siatki z osi X, aby wykres był czystszy
            obszar.AxisX.MajorGrid.Enabled = false;

            // --- KONFIGURACJA SERII DANYCH (Słupki) ---
            var seria = chartWykres.Series.Add("Sila");
            seria.Points.Clear();
            seria.ChartType = SeriesChartType.Column; // Wykres kolumnowy
            seria.Color = Color.CornflowerBlue;
            seria["PointWidth"] = "0.7"; // Delikatne zwężenie słupków
            //labele na slupkach przyklejone do slupkow
            seria.Font = new Font("Arial", 10, FontStyle.Bold); // Czcionka dla wartości nad słupkami

            // Wstępne wygenerowanie 10 słupków z wartością 0 i podpisami osi X (CH 1 - CH 10)
            for (int i = 0; i < 10; i++)
            {
                seria.Points.AddXY(i + 1, 0);
                seria.Points[i].AxisLabel = "CH " + (i + 1);
            }

            //labele nad wykresem nie przyklejone do slupkow
            //var seriaWartosci = chartWykres.Series.Add("WartosciSily");
            //seriaWartosci.ChartType = SeriesChartType.Point;
            //seriaWartosci.Color = Color.Transparent;
            //seriaWartosci.Font = new Font("Arial", 10, FontStyle.Bold);

            //for (int i = 0; i < 10; i++)
            //{
            //    seriaWartosci.Points.AddXY(i + 1, 250);
            //    seriaWartosci.Points[i].Label = "0.0";
            //}
        }

        /// <summary>
        /// Główny silnik przetwarzania danych pomiarowych. Rozbija tekst na części, przelicza 
        /// surowe wartości przetwornika ADC na jednostki fizyczne (Niutony) i rysuje słupki.
        /// </summary>
        /// <param name="rawData">Paczka danych z mikrokontrolera (oczekiwany format: "D:val1,val2,...,val10")</param>
        private void ProcessData(string rawData)
        {
            try
            {
                // Weryfikacja, czy linia danych jest poprawna (zaczyna się od prefiksu "D:")
                if (rawData.StartsWith("D:"))
                {
                    LogMessage($"Received: {rawData}");

                    // Odcinamy prefiks "D:" i dzielimy resztę tekstu po przecinkach do tablicy
                    string dataPart = rawData.Substring(2);
                    string[] values = dataPart.Split(',');

                    // Upewniamy się, że otrzymaliśmy dokładnie 10 kanałów
                    if (values.Length >= 10)
                    {
                        double sum = 0;
                        double loadRange = 50.0; // Maksymalny zakres czujnika w Funtach

                        // Iterujemy przez wszystkie 10 kanałów pomiarowych
                        for (int i = 0; i < 10; i++)
                        {
                            // Konwersja tekstu na liczbę całkowitą (wartość z 14-bitowego przetwornika ADC)
                            if (int.TryParse(values[i], out int adcValue))
                            {
                                // Obliczenie siły bezwzględnej z proporcji (0-4095 dla 12-bit ADC)
                                double absoluteForceN = ((adcValue - 1000) * loadRange / 14000) * 4.44822;
                                _currentAbsoluteForces[i] = absoluteForceN; // Zapisanie do późniejszego tarowania

                                // Automatyczne tarowanie na pierwszej paczce po starcie pomiaru
                                if (_autoTarePending)
                                {
                                    _tareOffsets[i] = absoluteForceN;
                                }

                                // Obliczenie siły netto (odjęcie wartości tary dla danego kanału)
                                double netForce = absoluteForceN - _tareOffsets[i];

                                // Obsługa błędu pomiaru
                                if (netForce < 0)
                                {
                                    netForce = 0;
                                }

                                sum += netForce; // Dodanie do sumy w celu późniejszego wyliczenia średniej

                                // Aktualizacja wykresu
                                chartWykres.Series["Sila"].Points[i].YValues[0] = netForce; // Wysokość słupka
                                //labele na slupkach przyklejone do slupkow
                                chartWykres.Series["Sila"].Points[i].Label = $"{netForce:F2}"; // Napis na słupku z dokładnością do 2 miejsc
                                //labele nad wykresem nie przyklejone do slupkow
                                //chartWykres.Series["WartosciSily"].Points[i].Label = $"{netForce:F2} N";
                            }
                        }

                        if (_autoTarePending)
                        {
                            _autoTarePending = false;
                            LogMessage("Auto-tare done on first data packet.");
                        }
                        // Obliczenie i narysowanie średniej ze wszystkich czujników
                        double average = sum / 10.0;
                        UpdateAverageLine(average);
                    }
                }
            }
            catch (Exception ex)
            {
                // Przechwycenie błędu (np. uszkodzonego pakietu) i wrzucenie go do konsoli
                LogMessage($"Data error: {ex.Message}");
            }
        }

        /// <summary>
        /// Zdarzenie wywoływane automatycznie przez system, gdy port COM odbierze nowe dane.
        /// Działa w osobnym wątku pracującym w tle.
        /// </summary>
        private void MyPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                // Odczytanie całej linii danych (aż do znaku nowej linii)
                string incomingData = myPort.ReadLine().Trim();

                // Jeśli użytkownik nie włączył pomiaru, ignorujemy dane i uciekamy z funkcji
                if (!isMeasuring)
                {
                    return;
                }

                // Bezpieczne przekazanie odebranych danych z wątku w tle do głównego wątku okna (UI Thread).
                // Jest to absolutnie konieczne, by móc modyfikować kontrolki (np. rysować wykres).
                this.Invoke(new Action(() =>
                {
                    ProcessData(incomingData);
                }));
            }
            catch (Exception ex)
            {
                // Wyjątek przy odczycie oznacza zwykle martwy port (np. wyrwany kabel),
                // więc zamykamy go w tle, żeby Close() nie zawiesiło wątku UI.
                HandleDisconnection(ex.Message, deviceGone: true);
            }
        }

        /// <summary>
        /// Rysuje poziomą, czerwoną linię na wykresie oznaczającą średnią wartość pomiarową.
        /// </summary>
        /// <param name="averageValue">Obliczona z 10 kanałów średnia siła w Niutonach</param>
        private void UpdateAverageLine(double averageValue)
        {
            // Czyszczenie starych adnotacji (np. poprzedniej linii średniej)
            chartWykres.Annotations.Clear();

            // Tworzenie nowej poziomej linii
            HorizontalLineAnnotation linia = new HorizontalLineAnnotation();
            linia.AxisX = chartWykres.ChartAreas[0].AxisX;
            linia.AxisY = chartWykres.ChartAreas[0].AxisY;
            linia.Y = averageValue; // Wysokość umieszczenia linii
            linia.LineColor = Color.Red;
            linia.LineWidth = 2;
            linia.ClipToChartArea = chartWykres.ChartAreas[0].Name;
            linia.IsInfinitive = true; // Linia ma ciągnąć się przez cały wykres od lewej do prawej

            // Dodanie linii do wykresu i aktualizacja napisu tekstowego na samej górze
            chartWykres.Annotations.Add(linia);
            chartWykres.Titles[1].Text = $"Average: {averageValue:F2} N";
        }

        /// <summary>
        /// Centralny punkt sprzątania po utracie połączenia z urządzeniem — niezależnie od tego, 
        /// czy zerwanie było zamierzone (użytkownik kliknął "Disconnect"), czy nagłe (fizyczne 
        /// wypięcie kabla wykryte przez WM_DEVICECHANGE albo przez błąd odczytu). Bezpiecznie
        /// zamyka i zwalnia port, resetuje wygląd przycisków i przerywa trwający pomiar, jeśli był aktywny.
        /// </summary>
        /// <param name="reason">Krótki opis przyczyny rozłączenia (np. treść wyjątku), trafiający do logu.</param>
        /// <param name="deviceGone">True, jeśli urządzenie zniknęło fizycznie - wtedy port jest zamykany
        /// w tle, bo Close() na usuniętym urządzeniu potrafi zawiesić wątek UI.</param>
        private void HandleDisconnection(string reason, bool deviceGone = false)
        {
            // Guard: wątek tła nie może bezpośrednio modyfikować kontrolek WinForms.
            // Jeśli jesteśmy poza wątkiem UI, przekazujemy wykonanie na właściwy wątek
            // i kończymy działanie bieżącej instancji metody.
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action(() => HandleDisconnection(reason, deviceGone)));
                return;
            }

            // Guard: unikamy powtórnego wykonania procedury sprzątającej, jeśli już wiemy,
            // że jesteśmy rozłączeni (np. przy kilku błędach/zdarzeniach z rzędu). Sprawdzamy
            // TYLKO _isConnected - to jedyne wiarygodne źródło prawdy w całej aplikacji.
            if (!_isConnected)
                return;

            _isConnected = false;
            _connectedPortName = null;

            // Odpięcie handlera, zamknięcie i Dispose() portu.
            SafeClosePort(deviceGone);

            // Reset stanu wizualnego przycisku połączenia do wartości domyślnych.
            btnConnect.Text = "Connect";
            btnConnect.BackColor = Color.LightGreen;

            // Jeżeli pomiar był aktywny w chwili rozłączenia, przerywamy go i synchronizujemy
            // stan wizualny przycisku pomiaru z rzeczywistym stanem logicznym aplikacji.
            if (isMeasuring)
            {
                btnMeasure.PerformClick();
            }

            LogMessage($"Disconnected: {reason}");
        }

        /// <summary>
        /// Bezpiecznie zamyka port szeregowy i ZWALNIA go (Dispose), ignorując wyjątki, które mogą
        /// wystąpić, gdy urządzenie zniknęło fizycznie w trakcie zamykania. Brak Dispose() powoduje,
        /// że sterownik trzyma uchwyt, a w systemie zostają martwe wpisy portu (duplikaty COM na liście).
        /// </summary>
        /// <param name="deviceGone">True - zamknięcie w tle (Task.Run), bo Close() na usuniętym
        /// urządzeniu potrafi zablokować wątek UI.</param>
        private void SafeClosePort(bool deviceGone = false)
        {
            var port = myPort;

            try { port.DataReceived -= MyPort_DataReceived; } catch { }

            void CloseAndDispose()
            {
                try
                {
                    if (port.IsOpen) port.Close();
                }
                catch (Exception ex)
                {
                    LogMessage($"Ignored error while closing port: {ex.Message}");
                }
                finally
                {
                    try { port.Dispose(); } catch { }
                }
            }

            if (deviceGone)
                Task.Run(CloseAndDispose);
            else
                CloseAndDispose();
        }


        /// <summary>
        /// Uniwersalna funkcja do wyświetlania wiadomości systemowych w oknie logów.
        /// Automatycznie dodaje godzinę i przewija widok na sam dół.
        /// </summary>
        /// <param name="message">Treść komunikatu</param>
        private void LogMessage(string message)
        {
            if (this.IsDisposed || !this.IsHandleCreated)
                return;

            if (rtbConsole.InvokeRequired)
            {
                rtbConsole.Invoke(new Action(() => LogMessage(message)));
                return;
            }

            rtbConsole.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
            rtbConsole.ScrollToCaret(); // Wymuszenie zjechania suwakiem do najnowszego loga
        }

        /// <summary>
        /// Obsługuje logikę nawiązywania i zrywania połączenia z mikrokontrolerem.
        /// Działa jako przycisk dwustanowy (Connect / Disconnect).
        /// </summary>
        private void btnConnect_Click(object sender, EventArgs e)
        {
            // --- SEKCJA ROZŁĄCZANIA ---
            // Ręczne rozłączenie deleguje do tego samego miejsca, które obsługuje nagłą utratę
            // połączenia - dzięki temu nie ma dwóch niezależnych kopii logiki resetowania UI.
            if (_isConnected)
            {
                HandleDisconnection("User request.");
                return; // Uciekamy z funkcji, żeby nie wykonać kodu łączenia znajdującego się poniżej
            }

            // --- SEKCJA ŁĄCZENIA I HANDSHAKE'U ---
            try
            {
                // Zabezpieczenie przed próbą połączenia bez wybranego portu
                if (cmbPorts.SelectedItem == null)
                {
                    MessageBox.Show("Proszę najpierw wybrać port COM z listy!", "Błąd połączenia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LogMessage("Connection aborted: No COM port selected.");
                    return;
                }

                // Zwolnienie poprzedniej instancji (jeśli jeszcze żyje) przed utworzeniem nowej.
                // Bez tego stary uchwyt wisi do finalizera i blokuje/dubluje port w systemie.
                try { myPort.DataReceived -= MyPort_DataReceived; } catch { }
                try { myPort.Dispose(); } catch { }

                // Nowa instancja SerialPort przy każdej próbie połączenia - jeśli poprzednie
                // połączenie zakończyło się awaryjnie (fizyczne wypięcie kabla w trakcie Close()),
                // stary obiekt mógł zostać w niespójnym wewnętrznym stanie. Zaczynamy od zera.
                myPort = new SerialPort();
                myPort.PortName = cmbPorts.SelectedItem.ToString();
                myPort.BaudRate = 115200; // Prędkość musi być identyczna jak w STM32
                myPort.ReadTimeout = 2000; // Czas oczekiwania na odpowiedź (2 sekundy)
                myPort.WriteTimeout = 1000;
                myPort.DtrEnable = true;
                myPort.RtsEnable = true;

                myPort.Open();

                // Czyszczenie starych śmieci z bufora (bardzo ważne przy restarcie połączenia!)
                myPort.DiscardInBuffer();
                myPort.DiscardOutBuffer();

                LogMessage($"Port {myPort.PortName} opened. Sending handshake...");

                // Inicjacja Handshake'u - wysyłamy literę "I" do mikrokontrolera
                myPort.WriteLine("I");

                // Czekamy na odpowiedź z tożsamością urządzenia
                string response = myPort.ReadLine().Trim();

                // Weryfikacja tożsamości - sprawdzamy, czy po drugiej stronie na pewno jest nasz sprzęt
                if (response == "ID:FORCE_MEASUREMENT_DEVICE_V1")
                {
                    LogMessage($"Handshake successful! STM32 is connected.");

                    // Zmiana stanu interfejsu na "połączony"
                    btnConnect.Text = "Disconnect";
                    btnConnect.BackColor = Color.Salmon;

                    _isConnected = true;
                    _connectedPortName = myPort.PortName;

                    // Podpięcie funkcji nasłuchującej nowe dane z kabla
                    myPort.DataReceived += MyPort_DataReceived;
                }
                else
                {
                    // Urządzenie odpowiedziało, ale nie zna poprawnego hasła/identyfikatora
                    LogMessage($"Handshake failed. Unexpected response: {response}");
                    SafeClosePort();
                }

            }
            catch (TimeoutException)
            {
                // Urządzenie nie odpowiedziało w ciągu 2 sekund (ReadTimeout)
                LogMessage("Handshake timeout. STM32 did not respond.");
                SafeClosePort();
            }
            catch (Exception ex)
            {
                // Łapanie innych błędów (np. port jest aktualnie używany przez inny program,
                // albo port o tej nazwie już fizycznie nie istnieje w systemie)
                LogMessage($"Error: {ex.Message}");
                SafeClosePort();
            }
        }

        /// <summary>
        /// Włącza i wyłącza nasłuchiwanie na przychodzące z urządzenia dane pomiarowe.
        /// Chroni przed rozpoczęciem pomiaru bez aktywnego połączenia.
        /// </summary>
        private void btnMeasure_Click(object sender, EventArgs e)
        {
            // Zabezpieczenie: Sprawdzamy stan połączenia TYLKO przy próbie startu pomiaru.
            // Pytamy o _isConnected, a nie o myPort.IsOpen - to samo źródło prawdy, którego
            // używa też btnConnect i HandleDisconnection, więc te trzy miejsca nie mogą
            // już się ze sobą "kłócić" tak jak na screenach.
            if (!isMeasuring && !_isConnected)
            {
                MessageBox.Show("Najpierw połącz się z urządzeniem!", "Brak połączenia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Odwrócenie flagi stanu (włącz -> wyłącz / wyłącz -> włącz)
            isMeasuring = !isMeasuring;

            // Aktualizacja wyglądu przycisku
            if (isMeasuring)
            {
                _autoTarePending = true;
                btnMeasure.Text = "Stop Measure";
                btnMeasure.BackColor = Color.LightCoral; // Czerwony ostrzega, że pomiar właśnie trwa
            }
            else
            {
                btnMeasure.Text = "Start Measure";
                btnMeasure.BackColor = SystemColors.ActiveCaption;
            }
        }

        /// <summary>
        /// Zdarzenie wywoływane w momencie rozwinięcia listy (ComboBox) z portami.
        /// Dynamicznie odświeża listę dostępnych portów COM w systemie Windows, 
        /// pozwalając na podpięcie kabla USB w trakcie działania programu.
        /// Lista jest pozbawiona duplikatów i posortowana naturalnie.
        /// </summary>
        private void cmbPorts_DropDown(object sender, EventArgs e)
        {
            // Zapamiętanie aktualnie wybranego portu
            string? selectedPorts = cmbPorts.SelectedItem?.ToString();

            cmbPorts.Items.Clear();

            // Pobranie z systemu aktualnej listy fizycznych i wirtualnych kabli COM (bez duplikatów)
            string[] availablePorts = GetUniquePortNames();
            cmbPorts.Items.AddRange(availablePorts);

            // Próba przywrócenia wcześniej wybranego portu, jeśli nadal widnieje na liście
            if (selectedPorts != null && cmbPorts.Items.Contains(selectedPorts))
            {
                cmbPorts.SelectedItem = selectedPorts;
            }
            else if (cmbPorts.Items.Count > 0)
            {
                // Jeśli stary port zniknął (ktoś odpiął kabel), wybieramy pierwszy dostępny z góry
                cmbPorts.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Wykonuje operację tarowania sprzętu. Aktualne fizyczne wartości (bezwzględne) 
        /// stają się nowym punktem odniesienia (zerem) dla wykresu.
        /// </summary>
        private void btnTare_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                // Kopiujemy bieżące surowe siły do tablicy offsetów
                _tareOffsets[i] = _currentAbsoluteForces[i];
            }

            LogMessage("System Tared. Offsets updated.");

            // Wymuszenie natychmiastowego odrysowania wykresu z uwzględnieniem nowej tary
            chartWykres.Invalidate();
        }

        /// <summary>
        /// Usuwa wprowadzony wcześniej punkt odniesienia (Tare). 
        /// Wykres wraca do pokazywania rzeczywistych, surowych wartości nacisku.
        /// </summary>
        private void btnClearTare_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                _tareOffsets[i] = 0; // Wyzerowanie offsetów
            }

            LogMessage("Tare reset. Offsets cleared.");
            chartWykres.Invalidate();
        }

        /// <summary>
        /// Przełącza widoczność konsoli tekstowej z logami systemowymi na dole ekranu.
        /// Pozwala zwolnić miejsce na ekranie dla samego wykresu.
        /// </summary>
        private void btnToggleConsole_Click(object sender, EventArgs e)
        {
            // Odwrócenie właściwości Visible (widoczny -> ukryty, ukryty -> widoczny)
            rtbConsole.Visible = !rtbConsole.Visible;

            // Dostosowanie napisu na przycisku do aktualnego stanu
            if (rtbConsole.Visible)
            {
                btnToggleConsole.Text = "Hide Console";
            }
            else
            {
                btnToggleConsole.Text = "Show Console";
            }
        }

        /// <summary>
        /// Zapewnia bezpieczne zamknięcie i zwolnienie portu szeregowego oraz odpięcie nasłuchiwaczy 
        /// przed zniszczeniem formularza. Zapobiega próbom dostępu do już zniszczonych 
        /// kontrolek z wątku tła SerialPort, jeśli dane przyjdą dokładnie w momencie 
        /// zamykania okna programu.
        /// </summary>
        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            SafeClosePort();
        }

        // =====================================================================
        // Puste zdarzenia wygenerowane automatycznie przez kliknięcia w projektancie. 
        // Nie mają wpływu na działanie programu, można je zignorować.
        // =====================================================================
        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void cmbPorts_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void chartWykres_Click(object sender, EventArgs e)
        {

        }
    }
}