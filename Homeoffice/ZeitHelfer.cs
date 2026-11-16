using System.Globalization;

namespace Homeoffice
{
    /// <summary>
    /// Hilfsmethoden rund um Uhrzeiten.
    /// </summary>
    public static class ZeitHelfer
    {
        /// <summary>
        /// Zerlegt einen Text wie "13:00 - 16:30", "8-12:00" oder "15:00-13:00"
        /// in Beginn und Ende. Sind die Zeiten vertauscht (Ende vor Beginn),
        /// werden sie automatisch getauscht (Fehlerbehandlung 1).
        /// </summary>
        public static void ZeitspanneLesen(string text, out TimeSpan beginn, out TimeSpan ende)
        {
            string[] teile = text.Split('-');
            if (teile.Length != 2)
            {
                throw new FormatException("Arbeitszeit nicht lesbar: '" + text + "'");
            }

            beginn = UhrzeitLesen(teile[0]);
            ende = UhrzeitLesen(teile[1]);

            // Fehlerbehandlung 1: Anfangs- und Endzeit vertauscht -> tauschen
            if (ende < beginn)
            {
                TimeSpan merker = beginn;
                beginn = ende;
                ende = merker;
            }
        }

        /// <summary>
        /// Wandelt "8", "8:00", "08:00" oder "8.30" in eine Uhrzeit um.
        /// </summary>
        public static TimeSpan UhrzeitLesen(string text)
        {
            text = text.Trim().Replace('.', ':');

            // Nur Stunde angegeben, z. B. "8" -> "8:00"
            if (!text.Contains(':'))
            {
                text = text + ":00";
            }

            string[] teile = text.Split(':');
            int stunden = int.Parse(teile[0], CultureInfo.InvariantCulture);
            int minuten = int.Parse(teile[1], CultureInfo.InvariantCulture);
            return new TimeSpan(stunden, minuten, 0);
        }

        /// <summary>
        /// Gibt Minuten als "h:mm" aus, z. B. 255 -> "4:15".
        /// </summary>
        public static string MinutenAlsText(long minuten)
        {
            return (minuten / 60) + ":" + (minuten % 60).ToString("00");
        }
    }
}
