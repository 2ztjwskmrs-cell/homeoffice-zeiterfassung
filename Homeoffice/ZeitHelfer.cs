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
            // Trennzeichen: Bindestrich "-" oder Gedankenstrich "–" (z. B. aus Word kopiert)
            string[] teile = text.Split('-', '–');
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

        // Erlaubte Schreibweisen einer Uhrzeit: "8:00" bzw. "08:00", "08:00:00", "8.30", nur Stunde "8"
        static readonly string[] Uhrzeitformate = { @"h\:mm", @"h\:mm\:ss", @"h\.mm", "%h" };

        /// <summary>
        /// Wandelt "8", "8:00", "08:00", "08:00:00" oder "8.30" in eine Uhrzeit um.
        /// </summary>
        public static TimeSpan UhrzeitLesen(string text)
        {
            TimeSpan zeit;
            if (TimeSpan.TryParseExact(text.Trim(), Uhrzeitformate, CultureInfo.InvariantCulture, out zeit))
            {
                return zeit;
            }
            throw new FormatException("Uhrzeit nicht lesbar: '" + text.Trim() + "'");
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
