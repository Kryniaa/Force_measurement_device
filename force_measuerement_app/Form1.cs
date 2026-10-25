using System.Diagnostics;
using System.IO.Ports;
using System.Text;
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
        /// Volatile, bo czytana z wątku tła SerialPort (MyPort_DataReceived).
        /// </summary>
        private volatile bool isMeasuring = false;

        /// <summary>
        /// Bufor na odebrane, jeszcze niekompletne fragmenty linii. Dane z portu są czytane
        /// przez ReadExisting() (bez blokowania), a pełne linie wycinane po znaku '\n'.
        /// Dostęp tylko pod lock(_rxBuffer).
        /// </summary>
        private readonly StringBuilder _rxBuffer = new StringBuilder();

        /// <summary>
        /// Zadanie zamykające port w tle po fizycznym odłączeniu urządzenia. Przy ponownym
        /// łączeniu czekamy na jego zakończenie, żeby nie otwierać portu, którego stary uchwyt
        /// jeszcze nie został zwolniony ("Access to the port is denied").
        /// </summary>
        private Task _pendingPortClose = Task.CompletedTask;

        /// <summary>
        /// Linia średniej na wykresie - tworzona raz i tylko przesuwana (zamiast tworzyć
        /// nowy obiekt adnotacji przy każdej paczce danych).
        /// </summary>
        private HorizontalLineAnnotation? _averageLine;

        /// <summary>
        /// Czy dany kanał przysłał ostatnio poprawny odczyt (służy do logowania zmian stanu
        /// czujnika bez zalewania konsoli tym samym błędem przy każdej paczce).
        /// </summary>
        private readonly bool[] _channelValid = Enumerable.Repeat(true, 10).ToArray();

        /// <summary>
        /// Firmware zgłasza błąd czujnika/I2C wartością 0xFFFF. Poprawny odczyt czujnika
        /// jest 14-bitowy (0..16383), więc wszystko powyżej traktujemy jako błąd kanału.
        /// </summary>
        private const int AdcMaxValid = 0x3FFF;

        /// <summary>
        /// Maksymalna liczba linii w konsoli. Logi "Received" przychodzą ~10 razy na sekundę,
        /// więc bez limitu RichTextBox rośnie w nieskończoność i po czasie spowalnia UI.
        /// </summary>
        private const int MaxConsoleLines = 1000;

        /// <summary>
        /// Ustawiana przy starcie pomiaru, osobno dla każdego kanału. Pierwszy POPRAWNY odczyt
        /// kanału zostaje użyty jako jego tara, po czym flaga kanału jest kasowana. Dzięki temu
        /// kanał, który w pierwszej paczce zgłosił błąd (0xFFFF), nie zostaje wytarowany śmieciem.
        /// </summary>
        private readonly bool[] _autoTarePending = new bool[10];

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
            // Paczka mogła czekać w kolejce BeginInvoke i dotrzeć już po kliknięciu "Stop Measure".
            if (!isMeasuring)
                return;

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
                        int validCount = 0;
                        bool autoTaredNow = false;
                        double loadRange = 50.0; // Maksymalny zakres czujnika w Funtach
                        var points = chartWykres.Series["Sila"].Points;

                        // Iterujemy przez wszystkie 10 kanałów pomiarowych
                        for (int i = 0; i < 10; i++)
                        {
                            // Konwersja tekstu na liczbę (14-bitowy odczyt czujnika: 0..16383).
                            // 0xFFFF (65535) to kod błędu czujnika/I2C wysyłany przez firmware -
                            // bez tej kontroli dawał ~1025 N i psuł tarę oraz średnią.
                            bool valid = int.TryParse(values[i], out int adcValue)
                                         && adcValue >= 0 && adcValue <= AdcMaxValid;

                            if (!valid)
                            {
                                if (_channelValid[i])
                                    LogMessage($"CH {i + 1}: sensor error (raw value: {values[i]}).");
                                _channelValid[i] = false;

                                points[i].YValues[0] = 0;
                                points[i].Label = "ERR";
                                continue;
                            }

                            if (!_channelValid[i])
                                LogMessage($"CH {i + 1}: sensor OK again.");
                            _channelValid[i] = true;

                            {
                                // Przeliczenie liczników na siłę: 1000 liczników = 0, span 14000 liczników = pełny zakres
                                double absoluteForceN = ((adcValue - 1000) * loadRange / 14000) * 4.44822;
                                _currentAbsoluteForces[i] = absoluteForceN; // Zapisanie do późniejszego tarowania

                                // Automatyczne tarowanie na pierwszym poprawnym odczycie kanału po starcie pomiaru
                                if (_autoTarePending[i])
                                {
                                    _tareOffsets[i] = absoluteForceN;
                                    _autoTarePending[i] = false;
                                    autoTaredNow = true;
                                }

                                // Obliczenie siły netto (odjęcie wartości tary dla danego kanału)
                                double netForce = absoluteForceN - _tareOffsets[i];

                                // Obsługa błędu pomiaru
                                if (netForce < 0)
                                {
                                    netForce = 0;
                                }

                                sum += netForce; // Dodanie do sumy w celu późniejszego wyliczenia średniej
                                validCount++;

                                // Aktualizacja wykresu
                                points[i].YValues[0] = netForce; // Wysokość słupka
                                //labele na slupkach przyklejone do slupkow
                                points[i].Label = $"{netForce:F2}"; // Napis na słupku z dokładnością do 2 miejsc
                                //labele nad wykresem nie przyklejone do slupkow
                                //chartWykres.Series["WartosciSily"].Points[i].Label = $"{netForce:F2} N";
                            }
                        }

                        if (autoTaredNow)
                        {
                            LogMessage(_autoTarePending.Any(p => p)
                                ? "Auto-tare done (some channels still waiting for a valid reading)."
                                : "Auto-tare done on first data packet.");
                        }

                        // Średnia tylko z kanałów z poprawnym odczytem (kanał z błędem nie zaniża wyniku)
                        double average = validCount > 0 ? sum / validCount : 0;
                        UpdateAverageLine(average);

                        // Zmiana YValues[0] nie zawsze sama odświeża kontrolkę - wymuszamy przerysowanie
                        chartWykres.Invalidate();
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
            var port = (SerialPort)sender;

            // Zdarzenie od starej (już zamykanej) instancji portu - ignorujemy.
            if (!ReferenceEquals(port, myPort))
                return;

            try
            {
                // ReadExisting() nie blokuje. Poprzednie ReadLine() czytało tylko JEDNĄ linię na
                // zdarzenie - gdy w buforze były 2+ linie, reszta czekała i opóźnienie wykresu
                // rosło z czasem. Do tego TimeoutException z ReadLine był traktowany jak wyrwany kabel.
                string chunk = port.ReadExisting();
                string? latestDataLine = null;

                lock (_rxBuffer)
                {
                    _rxBuffer.Append(chunk);
                    string all = _rxBuffer.ToString();
                    int lastNewLine = all.LastIndexOf('\n');

                    if (lastNewLine < 0)
                    {
                        // Zabezpieczenie przed śmieciami bez końca linii
                        if (_rxBuffer.Length > 4096)
                            _rxBuffer.Clear();
                    }
                    else
                    {
                        _rxBuffer.Remove(0, lastNewLine + 1);

                        // Rysujemy tylko najnowszą kompletną paczkę - starsze i tak zostałyby
                        // natychmiast nadpisane, a ich przetwarzanie tylko zapycha wątek UI.
                        string[] lines = all.Substring(0, lastNewLine).Split('\n');
                        for (int i = lines.Length - 1; i >= 0; i--)
                        {
                            string line = lines[i].Trim();
                            if (line.StartsWith("D:"))
                            {
                                latestDataLine = line;
                                break;
                            }
                        }
                    }
                }

                // Jeśli użytkownik nie włączył pomiaru, ignorujemy dane i uciekamy z funkcji
                if (!isMeasuring || latestDataLine == null)
                    return;

                if (this.IsDisposed || !this.IsHandleCreated)
                    return;

                // BeginInvoke (asynchronicznie), NIE Invoke. Invoke blokował wątek portu do czasu
                // obsłużenia przez UI - a gdy UI w tym samym momencie wołał port.Close()
                // (Disconnect / zamknięcie okna), Close() czekał na wątek portu i aplikacja się zawieszała.
                this.BeginInvoke(new Action(() => ProcessData(latestDataLine)));
            }
            catch (Exception ex)
            {
                // Wyjątek przy odczycie oznacza zwykle martwy port (np. wyrwany kabel),
                // więc zamykamy go w tle, żeby Close() nie zawiesiło wątku UI.
                HandleDisconnection(ex.Message, deviceGone: true, source: port);
            }
        }

        /// <summary>
        /// Rysuje poziomą, czerwoną linię na wykresie oznaczającą średnią wartość pomiarową.
        /// </summary>
        /// <param name="averageValue">Obliczona z 10 kanałów średnia siła w Niutonach</param>
        private void UpdateAverageLine(double averageValue)
        {
            // Linia tworzona tylko raz - potem jedynie zmieniamy jej wysokość
            if (_averageLine == null)
            {
                _averageLine = new HorizontalLineAnnotation();
                _averageLine.AxisX = chartWykres.ChartAreas[0].AxisX;
                _averageLine.AxisY = chartWykres.ChartAreas[0].AxisY;
                _averageLine.LineColor = Color.Red;
                _averageLine.LineWidth = 2;
                _averageLine.ClipToChartArea = chartWykres.ChartAreas[0].Name;
                _averageLine.IsInfinitive = true; // Linia ma ciągnąć się przez cały wykres od lewej do prawej
                chartWykres.Annotations.Add(_averageLine);
            }

            _averageLine.Y = averageValue; // Wysokość umieszczenia linii

            // Aktualizacja napisu tekstowego na samej górze
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
        private void HandleDisconnection(string reason, bool deviceGone = false, SerialPort? source = null)
        {
            // Okno jest już zamknięte - nie ma czego sprzątać w UI, a BeginInvoke rzuciłby
            // wyjątek na wątku tła (nieobsłużony = crash aplikacji).
            if (this.IsDisposed || this.Disposing)
                return;

            // Guard: wątek tła nie może bezpośrednio modyfikować kontrolek WinForms.
            // Jeśli jesteśmy poza wątkiem UI, przekazujemy wykonanie na właściwy wątek
            // i kończymy działanie bieżącej instancji metody.
            if (this.InvokeRequired)
            {
                try
                {
                    this.BeginInvoke(new Action(() => HandleDisconnection(reason, deviceGone, source)));
                }
                catch (Exception) { /* uchwyt okna właśnie zniszczony */ }
                return;
            }

            // Błąd zgłoszony przez starą instancję portu (np. po szybkim Disconnect -> Connect)
            // nie może rozłączyć nowego, poprawnie działającego połączenia.
            if (source != null && !ReferenceEquals(source, myPort))
                return;

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
            // Bezpośrednio, a nie przez btnMeasure.PerformClick() - PerformClick nic nie robi,
            // gdy przycisk nie może przyjąć fokusu (np. okno nieaktywne/zminimalizowane albo
            // walidacja innej kontrolki), i pomiar zostawał logicznie "włączony".
            if (isMeasuring)
            {
                SetMeasuring(false);
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
                _pendingPortClose = Task.Run(CloseAndDispose);
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
            if (this.IsDisposed || this.Disposing || !this.IsHandleCreated)
                return;

            if (rtbConsole.InvokeRequired)
            {
                // BeginInvoke - wątek tła nie czeka na UI (Invoke mógł się zakleszczyć np. gdy
                // UI czekał na zamknięcie portu, a zamykanie w tle chciało coś zalogować).
                try
                {
                    rtbConsole.BeginInvoke(new Action(() => LogMessage(message)));
                }
                catch (Exception) { /* okno właśnie zamykane */ }
                return;
            }

            // Ograniczenie rozmiaru konsoli - usuwamy najstarsze linie
            int lineCount = rtbConsole.GetLineFromCharIndex(rtbConsole.TextLength);
            if (lineCount > MaxConsoleLines + 100)
            {
                int cutIndex = rtbConsole.GetFirstCharIndexFromLine(lineCount - MaxConsoleLines);
                if (cutIndex > 0)
                {
                    bool wasReadOnly = rtbConsole.ReadOnly;
                    rtbConsole.ReadOnly = false;
                    rtbConsole.Select(0, cutIndex);
                    rtbConsole.SelectedText = string.Empty;
                    rtbConsole.ReadOnly = wasReadOnly;
                }
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

                // Jeśli poprzedni port jest jeszcze zamykany w tle (po wypięciu kabla), czekamy chwilę,
                // inaczej Open() tego samego COM potrafi rzucić "Access to the port is denied".
                if (!_pendingPortClose.IsCompleted)
                {
                    LogMessage("Waiting for the previous port to close...");
                    if (!_pendingPortClose.Wait(1500))
                        LogMessage("Previous port is still closing - if connecting fails, try again in a moment.");
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

                // Czekamy na odpowiedź z tożsamością urządzenia. Pomijamy ewentualne linie z danymi
                // ("D:...") - mogą jeszcze przyjść resztki z poprzedniej sesji.
                string response = ReadHandshakeResponse(myPort, 2000);

                // Weryfikacja tożsamości - sprawdzamy, czy po drugiej stronie na pewno jest nasz sprzęt
                if (response == "ID:FORCE_MEASUREMENT_DEVICE_V1")
                {
                    LogMessage($"Handshake successful! STM32 is connected.");

                    // Zmiana stanu interfejsu na "połączony"
                    btnConnect.Text = "Disconnect";
                    btnConnect.BackColor = Color.Salmon;

                    _isConnected = true;
                    _connectedPortName = myPort.PortName;

                    lock (_rxBuffer) _rxBuffer.Clear();
                    for (int i = 0; i < 10; i++) _channelValid[i] = true;

                    // Podpięcie funkcji nasłuchującej nowe dane z kabla
                    myPort.DataReceived += MyPort_DataReceived;
                }
                else
                {
                    // Urządzenie odpowiedziało, ale nie zna poprawnego hasła/identyfikatora
                    LogMessage($"Handshake failed. Unexpected response: {response}");
                    if (response.StartsWith("D:"))
                        LogMessage("Device is already streaming data (previous session was not closed properly). Replug the USB cable and connect again.");
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

            // Odwrócenie stanu (włącz -> wyłącz / wyłącz -> włącz)
            SetMeasuring(!isMeasuring);
        }

        /// <summary>
        /// Jedno miejsce zmiany stanu pomiaru: flaga, auto-tara i wygląd przycisku.
        /// Używane przez btnMeasure oraz przez HandleDisconnection.
        /// </summary>
        private void SetMeasuring(bool measuring)
        {
            isMeasuring = measuring;

            // Aktualizacja wyglądu przycisku
            if (measuring)
            {
                for (int i = 0; i < 10; i++)
                    _autoTarePending[i] = true;
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
                // Kopiujemy bieżące surowe siły do tablicy offsetów (pomijamy kanały z błędem)
                if (_channelValid[i])
                    _tareOffsets[i] = _currentAbsoluteForces[i];
            }

            LogMessage(isMeasuring
                ? "System Tared. Offsets updated."
                : "System Tared using the last received values (measurement is stopped).");

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
            isMeasuring = false;
            _isConnected = false;
            SafeClosePort();
        }

        /// <summary>
        /// Czyta linie z portu aż do odpowiedzi "ID:..." albo upływu czasu. Linie z danymi
        /// pomiarowymi są pomijane. Gdy port milczy, ReadLine() rzuca TimeoutException
        /// (obsługiwany w btnConnect_Click).
        /// </summary>
        private static string ReadHandshakeResponse(SerialPort port, int timeoutMs)
        {
            var sw = Stopwatch.StartNew();
            string lastLine = "";

            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                string line = port.ReadLine().Trim();
                if (line.StartsWith("ID:"))
                    return line;
                if (line.Length > 0)
                    lastLine = line;
            }

            return lastLine;
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