namespace Homeoffice
{
    /// <summary>
    /// Eine Zeile aus der Excel-Tabelle, schon bereinigt (Transform-Schritt des ETL-Prozesses).
    /// </summary>
    public class Eintrag
    {
        public string Nachname;
        public string Vorname;
        public DateTime Datum;
        public string Taetigkeit;
        public TimeSpan Beginn;
        public TimeSpan Ende;
        public string Vorgesetzter;

        /// <summary>Dauer in Minuten, aus Beginn und Ende berechnet.</summary>
        public int DauerMinuten
        {
            get { return (int)(Ende - Beginn).TotalMinutes; }
        }
    }
}
