using System.Text;
using System.Data.SQLite;

namespace Homeoffice
{
    /// <summary>
    /// LF08-Projekt: Homeoffice-Zeiterfassung
    /// Importiert Excel-Dateien in eine SQLite-Datenbank (ETL) und wertet sie aus.
    ///
    /// Aufruf:  homeoffice.exe /befehl [Ordner]
    /// Ohne Ordnerangabe wird das aktuelle Verzeichnis verwendet.
    /// Dort liegen die Excel-Dateien und die Datenbank homeoffice.db.
    /// </summary>
    class Program
    {
        // TODO: Namen der Projektbeteiligten eintragen
        static readonly string[] Projektbeteiligte =
        {
            "Vorname Nachname (Teammitglied 1)",
            "Vorname Nachname (Teammitglied 2)",
        };

        static int Main(string[] args)
        {
            // Damit Umlaute (ä, ö, ü) in der Windows-Konsole richtig erscheinen
            Console.OutputEncoding = Encoding.UTF8;

            if (args.Length == 0)
            {
                HilfeAnzeigen();
                return 1;
            }

            string befehl = args[0].ToLower();
            string ordner = Directory.GetCurrentDirectory();
            if (args.Length >= 2)
            {
                ordner = args[1];
            }

            if (!Directory.Exists(ordner))
            {
                Console.WriteLine("Ordner nicht gefunden: " + ordner);
                return 1;
            }

            try
            {
                switch (befehl)
                {
                    case "/n":
                        NamenAusgeben();
                        break;
                    case "/imp":
                        Importieren(ordner);
                        break;
                    case "/show":
                    case "/count":
                    case "/max":
                    case "/time":
                        Auswerten(befehl, ordner);
                        break;
                    default:
                        Console.WriteLine("Unbekannter Befehl: " + args[0]);
                        HilfeAnzeigen();
                        return 1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fehler: " + ex.Message);
                return 1;
            }

            return 0;
        }

        static void NamenAusgeben()
        {
            Console.WriteLine("Projektbeteiligte:");
            foreach (string name in Projektbeteiligte)
            {
                Console.WriteLine("  " + name);
            }
        }

        /// <summary>
        /// /imp – alle Excel-Dateien im Ordner einlesen und in die Datenbank schreiben.
        /// </summary>
        static void Importieren(string ordner)
        {
            List<string> dateien = ExcelImport.DateienSuchen(ordner);
            Console.WriteLine(dateien.Count + " Excel-Datei(en) gefunden in " + ordner);
            if (dateien.Count == 0)
            {
                return;
            }

            int gelesen = 0;
            int neu = 0;
            int doubletten = 0;
            int fehlerhafteDateien = 0;

            using (SQLiteConnection verbindung = Datenbank.Oeffnen(ordner))
            {
                // Eine Transaktion für den ganzen Import: bei vielen Dateien
                // um ein Vielfaches schneller als jeder INSERT einzeln.
                using (SQLiteTransaction transaktion = verbindung.BeginTransaction())
                {
                    foreach (string datei in dateien)
                    {
                        List<Eintrag> eintraege;
                        try
                        {
                            // Erst die ganze Datei lesen, dann schreiben. So landet von
                            // einer kaputten Datei nichts halb in der Datenbank.
                            eintraege = ExcelImport.DateiLesen(datei);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("  Übersprungen: " + Path.GetFileName(datei) + " (" + ex.Message + ")");
                            fehlerhafteDateien++;
                            continue;
                        }

                        foreach (Eintrag e in eintraege)
                        {
                            gelesen++;
                            if (Datenbank.EintragSpeichern(verbindung, transaktion, e))
                            {
                                neu++;
                            }
                            else
                            {
                                doubletten++;
                            }
                        }
                    }
                    transaktion.Commit();
                }
            }

            Console.WriteLine("Import abgeschlossen.");
            Console.WriteLine("  Zeilen gelesen:          " + gelesen);
            Console.WriteLine("  Neu in Datenbank:        " + neu);
            Console.WriteLine("  Doubletten übersprungen: " + doubletten);
            if (fehlerhafteDateien > 0)
            {
                Console.WriteLine("  Fehlerhafte Dateien:     " + fehlerhafteDateien);
            }
        }

        static void Auswerten(string befehl, string ordner)
        {
            if (!File.Exists(Path.Combine(ordner, Datenbank.Dateiname)))
            {
                Console.WriteLine("Keine Datenbank gefunden. Bitte zuerst /imp ausführen.");
                return;
            }

            using (SQLiteConnection verbindung = Datenbank.Oeffnen(ordner))
            {
                if (befehl == "/show") Datenbank.AllesAnzeigen(verbindung);
                if (befehl == "/count") Datenbank.AnzahlAnzeigen(verbindung);
                if (befehl == "/max") Datenbank.MaximumAnzeigen(verbindung);
                if (befehl == "/time") Datenbank.ZeitJeMitarbeiterAnzeigen(verbindung);
            }
        }

        static void HilfeAnzeigen()
        {
            Console.WriteLine("Aufruf: homeoffice.exe /befehl [Ordner]");
            Console.WriteLine();
            Console.WriteLine("  /n      Namen der Projektbeteiligten ausgeben");
            Console.WriteLine("  /imp    Alle Excel-Dateien (homeoffice_*.xlsx) importieren");
            Console.WriteLine("  /show   Komplette Tabelle ausgeben");
            Console.WriteLine("  /count  Anzahl der Einträge ausgeben");
            Console.WriteLine("  /max    Längste Arbeitszeit mit Datum ausgeben");
            Console.WriteLine("  /time   Gesamtarbeitszeit je Mitarbeiter ausgeben");
            Console.WriteLine();
            Console.WriteLine("Ohne Ordner wird das aktuelle Verzeichnis verwendet.");
        }
    }
}
