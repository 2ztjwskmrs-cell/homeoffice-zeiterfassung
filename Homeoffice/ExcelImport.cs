using System.Globalization;
using ClosedXML.Excel;

namespace Homeoffice
{
    /// <summary>
    /// Liest die Homeoffice-Excel-Dateien ein (Extract-Schritt des ETL-Prozesses).
    /// </summary>
    public static class ExcelImport
    {
        // Feste Positionen in jeder Excel-Datei (alle Dateien sind gleich aufgebaut)
        const string ZelleNachname = "B3";
        const string ZelleVorname = "B4";
        const int ErsteDatenzeile = 7;

        const int SpalteDatum = 1;        // A
        const int SpalteTaetigkeit = 2;   // B
        const int SpalteArbeitszeit = 3;  // C
        // Spalte D "Zeit (h)" wird bewusst NICHT gelesen: sie ist in den Dateien
        // teilweise falsch und lässt sich aus Beginn/Ende selbst berechnen.
        const int SpalteVorgesetzter = 5; // E

        /// <summary>
        /// Sucht alle Dateien homeoffice_*.xlsx im angegebenen Ordner.
        /// </summary>
        public static string[] DateienSuchen(string ordner)
        {
            // Excel-Sperrdateien heißen "~$homeoffice_..." und passen daher nicht auf das Muster
            return Directory.GetFiles(ordner, "homeoffice_*.xlsx");
        }

        /// <summary>
        /// Liest alle Einträge aus einer Excel-Datei.
        /// </summary>
        public static List<Eintrag> DateiLesen(string pfad)
        {
            List<Eintrag> eintraege = new List<Eintrag>();

            using (XLWorkbook mappe = new XLWorkbook(pfad))
            {
                IXLWorksheet blatt = mappe.Worksheet(1);

                // Name und Vorname stehen im Kopf der Datei (nicht aus dem Dateinamen nehmen!)
                string nachname = blatt.Cell(ZelleNachname).GetFormattedString().Trim();
                string vorname = blatt.Cell(ZelleVorname).GetFormattedString().Trim();

                int zeile = ErsteDatenzeile;
                // So lange lesen, bis in Spalte A (Datum) nichts mehr steht
                while (!blatt.Cell(zeile, SpalteDatum).IsEmpty())
                {
                    Eintrag e = new Eintrag();
                    e.Nachname = nachname;
                    e.Vorname = vorname;
                    e.Datum = DatumLesen(blatt.Cell(zeile, SpalteDatum));
                    e.Taetigkeit = blatt.Cell(zeile, SpalteTaetigkeit).GetFormattedString().Trim();
                    e.Vorgesetzter = blatt.Cell(zeile, SpalteVorgesetzter).GetFormattedString().Trim();

                    string arbeitszeit = blatt.Cell(zeile, SpalteArbeitszeit).GetFormattedString();
                    ZeitHelfer.ZeitspanneLesen(arbeitszeit, out e.Beginn, out e.Ende);

                    eintraege.Add(e);
                    zeile++;
                }
            }

            return eintraege;
        }

        /// <summary>
        /// Das Datum steht meist als echtes Excel-Datum in der Zelle,
        /// zur Sicherheit wird auch Text wie "01.09.2024" verstanden.
        /// </summary>
        static DateTime DatumLesen(IXLCell zelle)
        {
            if (zelle.DataType == XLDataType.DateTime)
            {
                return zelle.GetDateTime().Date;
            }
            if (zelle.DataType == XLDataType.Number)
            {
                // Excel speichert Datumswerte intern als Zahl (Tage seit 1900)
                return DateTime.FromOADate(zelle.GetDouble()).Date;
            }
            return DateTime.ParseExact(zelle.GetFormattedString().Trim(), "dd.MM.yyyy",
                                       CultureInfo.InvariantCulture);
        }
    }
}
