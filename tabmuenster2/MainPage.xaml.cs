//using Java.Net;
using Microsoft.Maui.ApplicationModel;     // PhoneDialer, Sms, Email
using Microsoft.Maui.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace tabmuenster
{
    // Modell (ursprünglich "Task" — umbenannt auf "TabTask" um Konflikte zu vermeiden)
    public class TabTask
    {
        public string UNID { get; set; } = string.Empty;
        public string F14 { get; set; } = string.Empty;
        public string F7 { get; set; } = string.Empty;
        public string F1 { get; set; } = string.Empty;
        public string F3 { get; set; } = string.Empty;
        public string F15 { get; set; } = string.Empty;
        public string CDate { get; set; } = string.Empty;
        public string LCMDate { get; set; } = string.Empty;
        public string Score { get; set; } = string.Empty;
        public string HasCommunity { get; set; } = string.Empty;
        public string HasAttachment { get; set; } = string.Empty;
        public string RA { get; set; } = string.Empty;
        public string Attachments { get; set; } = string.Empty;
        public string Distance { get; set; } = string.Empty;
        public string Geo_LatLng { get; set; } = string.Empty;
        public string PathDia { get; set; } = string.Empty;
        public string CustViewField1 { get; set; } = string.Empty;
        public string CustViewField2 { get; set; } = string.Empty;
    }

    public class Aufgabe
    {
        public string? Quartier { get; set; }
        public string? Kategorie { get; set; }
        public string? Aufgabenbeschreibung { get; set; }
        public string? Kategoriebild { get; set; }
        public string? MyID { get; set; }
    }

    public partial class MainPage : ContentPage
    {
        // non-nullable property initialisiert sofort
        public ObservableCollection<Aufgabe> aufgaben { get; set; } = new ObservableCollection<Aufgabe>();

        // darf null sein, bis eine Auswahl existiert
        public Aufgabe? selektierteAufgabe;

        public MainPage()
        {
            InitializeComponent();

            // Damit XAML-Bindings wie ItemsSource funktionieren
            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Daten laden, wenn noch nichts drin ist
            if (aufgaben.Count == 0)
            {
                _ = LoadDataAsync(); // Feuer-und-Vergessen; Fehler werden intern behandelt
            }
        }

        private async Task LoadDataAsync()
        {
            string url = "https://cloud-11.datenbanken24.de/apps/tab/public.nsf/mobileRequest?openagent&callback=db24&FN=F3";

            try
            {
              
                using var http = new HttpClient();
                var responseText = await http.GetStringAsync(url).ConfigureAwait(false);

                // parse wie im Original (rudimentär)
                int anzahl = CountChar(responseText, '{');
                if (anzahl <= 1) return;

                int endPosition = 1;
                int startPosition = 0;
                var tasks = new TabTask[anzahl - 1];
                var items = new Aufgabe[anzahl - 1];
                int y = 0;

                while (y < anzahl - 1)
                {
                    startPosition = responseText.IndexOf("{", endPosition) + 1;
                    endPosition = responseText.IndexOf("}", startPosition);
                    if (startPosition <= 0 || endPosition <= startPosition) break;

                    string word22 = responseText.Substring(startPosition, endPosition - startPosition);
                    string word32 = word22.Replace("\"", "\'");
                    string word42 = "{ " + word32 + "}";
                    var t = JsonConvert.DeserializeObject<TabTask>(word42);
                    if (t != null)
                    {
                        tasks[y] = t;
                        items[y] = new Aufgabe
                        {
                            Quartier = ConvertF7_to_Quartier(t.F7),
                            Kategorie = ConvertF1_to_Kategorie(t.F1),
                            Aufgabenbeschreibung = t.F3,
                            Kategoriebild = ConvertF1_to_Kategoriebild(t.F1),
                            MyID=ConvertF14_to_MyID(t.F14)
                        };
                        y++;
                    }
                    else
                    {
                        break;
                    }
                }

                // UI-Thread aktualisieren
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    aufgaben.Clear();
                    for (int i = 0; i < y; i++)
                        aufgaben.Add(items[i]);
                });
            }
            catch (Exception ex)
            {
                // Kompletten Exception-Text ins Debug-Log schreiben
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                // Fehler sichtbar machen (Debug/Release beachten)
                await MainThread.InvokeOnMainThreadAsync(() =>
                    DisplayAlert("Fehler beim Laden", ex.ToString(), "OK"));

                // Test: kurz versuchen, die Anfrage mit deaktivierter Zertifikatsprüfung (nur zu Diagnose!)
                bool insecureOk = await TryRequestWithInsecureHandlerAsync(url);
                if (insecureOk)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                        DisplayAlert("Hinweis", "Unsichere Anfrage (Zertifikatsprüfung aus) hat funktioniert — vermutlich Problem mit Zertifikat/Kette oder TLS-Kompatibilität.", "OK"));
                }
            }
        }

        // Test-Methode: Anfrage mit deaktivierter Zertifikatsprüfung (NUR ZU DIAGNOSE, NICHT PRODUKTIV)
        private async Task<bool> TryRequestWithInsecureHandlerAsync(string url)
        {
            try
            {
                var handler = new HttpClientHandler
                {
                    // Danger: nur für Test! Nicht in Produktion verwenden.
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                };
                using var http = new HttpClient(handler);
                var text = await http.GetStringAsync(url).ConfigureAwait(false);
                System.Diagnostics.Debug.WriteLine("Insecure request succeeded, length=" + (text?.Length ?? 0));
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Insecure request failed: " + ex.ToString());
                return false;
            }
        }
        
        #region Hilfsmethoden & Konverter
        public string ConvertF1_to_Kategorie(string f1)
        {
            switch (f1)
            {
                case "1": return "Gartenarbeit";
                case "2": return "Einkaufsdienste / Botengänge";
                case "3": return "Kleine Hilfen im Haushalt";
                case "4": return "Technik (Smartphone/PC)";
                case "5": return "Tierpflege";
                case "0": return "Sonstiges";
            }
            return "keine Kategorie gefunden";
        }

        public string ConvertF1_to_Kategoriebild(string f1)
        {
            switch (f1)
            {
                case "1": return "ic_spa_active.png";
                case "2": return "ic_shopping_cart_active.png";
                case "3": return "ic_home.png";
                case "4": return "ic_desktop_mac_active.png";
                case "5": return "ic_pets_active.png";
                case "0": return "ic_timer.png";
            }
            return "ic_add.png";
        }

        public string ConvertF14_to_MyID(string f14)
        {
            return f14;
        }

        public string ConvertF7_to_Quartier(string f7)
        {
            return f7;
            /*switch
            {
                "1" => "Albachten",
                "2" => "Angelmodde",
                "3" => "Amelsbüren",
                "4" => "Berg Fidel",
                "5" => "Coerde",
                "6" => "Gievenbeck",
                "7" => "Gremmendorf",
                "8" => "Handorf",
                "9" => "Hiltrup",
                "10" => "Innenstadt",
                "11" => "Kinderhaus",
                "12" => "Mauritz",
                "13" => "Mecklenbeck",
                "14" => "Nienberge",
                "15" => "Roxel",
                "16" => "Rumphorst",
                "17" => "Sentruper Höhe",
                "18" => "Südviertel",
                "19" => "Wolbeck",
                _ => "keine Quartier gefunden (Mappingliste unvollständig)"
            };*/
        }

        public int CountChar(string s, char search)
        {
            if (string.IsNullOrEmpty(s)) return 0;
            int tmp = 0;
            for (int i = 0; i < s.Length; i++)
                if (s[i] == search) tmp++;
            return tmp;
        }
        #endregion

        #region Event-Handler (Signaturen MAUI-konform)
        void OnTap(object sender, ItemTappedEventArgs e)
        {
            // optional: kurz hervorheben oder Details anzeigen
            if (selektierteAufgabe == null)
            {
                DisplayAlert("Keine Aufgabe ausgewählt", "Bitte wähle eine Aufgabe aus, die du ganz angezeigt bekommen möchstest.", "Ok");
            }
            else
            {
                DisplayAlert("Aufgabe:", selektierteAufgabe.Aufgabenbeschreibung +" ("+selektierteAufgabe.MyID+")", "Ok");

            }
        }

        void OnSelection(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null) return;
            selektierteAufgabe = (Aufgabe)e.SelectedItem;
        }

        private void ButtonCallClicked(object? sender, EventArgs e)
        {
            string phoneNumber = "025114917752";

            if (string.IsNullOrEmpty(phoneNumber)) return;

            try
            {
                PhoneDialer.Open(phoneNumber);
            }
            catch (Exception ex) when (ex is ArgumentNullException || ex is FeatureNotSupportedException)
            {
                DisplayAlert("Fehlgeschlagen", ex.Message, "OK");
            }
            catch (Exception ex)
            {
                DisplayAlert("Fehlgeschlagen", ex.Message, "OK");
            }
        }

       /* private async void ButtonSMSClicked(object? sender, EventArgs e)
        {
            if (selektierteAufgabe == null)
            {
                await DisplayAlert("Keine Aufgabe ausgewählt", "Bitte wähle eine Aufgabe aus, die du übernehmen möchtest.", "Ok");
                return;
            }

            string smsPhoneNumber = "017620985995";
            string smsText = $"Ich habe Interesse, die Aufgabe '{selektierteAufgabe.Aufgabenbeschreibung}' aus der Kategorie '{selektierteAufgabe.Kategorie}' zu übernehmen."+selektierteAufgabe.MyID+")"+ \nMein Name:\nMeine Telefonnummer:\nMeine Email-Adresse:";

            try
            {
                var message = new SmsMessage(smsText, new[] { smsPhoneNumber });
                await Sms.ComposeAsync(message);
            }
            catch (Exception ex) when (ex is FeatureNotSupportedException)
            {
                await DisplayAlert("Fehlgeschlagen", ex.Message, "OK");
            }
        }
       */
        private async void ButtonMailClicked(object? sender, EventArgs e)
        {
            if (selektierteAufgabe == null)
            {
                await DisplayAlert("Keine Aufgabe ausgewählt", "Bitte wähle eine Aufgabe aus, die du übernehmen möchtest.", "Ok");
                return;
            }

            string toEmail = "info@taschengeldboerse-muenster.de";
            string emailSubject = "Interesse an: " + selektierteAufgabe.Aufgabenbeschreibung;
            string emailBody = "Ich habe Interesse, die Aufgabe '" + selektierteAufgabe.Aufgabenbeschreibung +
                       "' aus der Kategorie '" + selektierteAufgabe.Kategorie + "' zu übernehmen.('" +selektierteAufgabe.MyID+"')."+
                       Environment.NewLine + "Mein Name: " +
                       Environment.NewLine + "Meine Telefonnummer: " +
                       Environment.NewLine + "Meine Email-Adresse: ";

            try
            {
                var message = new EmailMessage
                {
                    Subject = emailSubject,
                    Body = emailBody,
                    To = { toEmail },
                };
                await Email.ComposeAsync(message);
            }
            catch (Exception ex) when (ex is FeatureNotSupportedException)
            {
                await DisplayAlert("Fehlgeschlagen", ex.Message, "OK");
            }
        }

        private async void ButtonInfoClicked(object? sender, EventArgs e)
        {
            string ein_text = "Die Taschengeldbörse Münster – ein Gewinn für Jung und Alt" + Environment.NewLine +
                  Environment.NewLine +
                 "Bevor du eine Aufgabe übernehmen kannst, melde dich bei der Taschengeldbörse an unter:" + Environment.NewLine +
 "https://www.taschengeldboerse-muenster.de/de/jugendliche/anmeldung/" + Environment.NewLine +
 "Wenn du Fragen hast, rufe uns unter der Telefonnummer 02 51 14 91 77 52 (immer die ganze Nummer wählen und außerhalb der Sprechstunde gerne auf den AB sprechen.)"
 + Environment.NewLine + Environment.NewLine +

 "Ältere Menschen benötigen bei einfachen, ungefährlichen, haushaltsnahen Tätigkeiten gelegentlich Unterstützung zu kleinem Preis." + Environment.NewLine +
 "Jugendliche suchen Möglichkeiten unkompliziert und ohne dauerhafte Verpflichtung ihr Taschengeld aufzubessern, um sich den einen oder anderen Wunsch erfüllen zu können." + Environment.NewLine +
 "Die Taschengeldbörse Münster bringt Jung und Alt zusammen und bietet Jugendlichen und Seniorinnen und Senioren eine gemeinsame Plattform und Vermittlungsstelle." + Environment.NewLine +
  Environment.NewLine +
 "Träger und Kooperationspartner: " + Environment.NewLine +
  "Die Taschengeldbörse ist ein Projekt der Stiftung Magdalenenhospital - Stadtteilinitiativen 'Von Mensch zu Mensch', Kooperationspartner ist die Kommunale Seniorenvertretung Münster." + Environment.NewLine +
  Environment.NewLine +
 "Offene Aufgaben - Du möchtest einen der offenen Aufgaben übernehmen?" + Environment.NewLine +
 "Dann" + Environment.NewLine +
 "    ruf uns an 0251 / 14917752 (immer mit Vorwahl wählen und außerhalb der Sprechstunde gerne auf den AB sprechen)" + Environment.NewLine +
 "    oder" + Environment.NewLine +
 "    schreib uns eine E - Mail an info@taschengeldboerse-muenster.de" + Environment.NewLine +
 "Wir melden uns daraufhin schnellstmöglich bei dir." + Environment.NewLine +
 Environment.NewLine +
 "https://www.taschengeldboerse-muenster.de" + Environment.NewLine +

 "Datenschutzerklärung siehe: https://www.taschengeldboerse-muenster.de/de/datenschutzerklaerung/"
             ;

            await DisplayAlert("Information zur Taschengeldbörse Münster", ein_text, "Ok");
        }

        private async void ButtonImpressumClicked(object? sender, EventArgs e)
        {
            string ein_text = "Die Taschengeldbörse Münster – ein Gewinn für Jung und Alt" + Environment.NewLine +
                 Environment.NewLine +
                 "Träger und Kooperationspartner: " + Environment.NewLine +
            "Die Taschengeldbörse ist ein Projekt der Stiftung Magdalenenhospital - Stadtteilinitiativen 'Von Mensch zu Mensch', Kooperationspartner ist die Kommunale Seniorenvertretung Münster." + Environment.NewLine +
            Environment.NewLine +
            "https://www.taschengeldboerse-muenster.de" +
            Environment.NewLine +
            "Einige Icons der App sind von: https://icons8.de" +
             Environment.NewLine +
                 "Die Grundlagen zu dieser App stammen vom  Münsterhack 2018: https://www.muensterhack.de/" + Environment.NewLine +

"Datenschutzerklärung siehe: https://www.taschengeldboerse-muenster.de/de/datenschutzerklaerung/";
            await DisplayAlert("Information zur Taschengeldbörse Münster", ein_text, "Ok");
        }
        #endregion
    }
}
