using System.Data.SQLite;

namespace Homeoffice
{
    /// <summary>
    /// Alles, was mit der SQLite-Datenbank zu tun hat:
    /// Tabellen anlegen, Daten schreiben (Load-Schritt des ETL-Prozesses) und Auswertungen.
    /// </summary>
    public static class Datenbank
    {
        public const string Dateiname = "homeoffice.db";

        // Tabellen werden nur angelegt, wenn sie noch nicht existieren.
        // Die Datenbank wird also NIE gelöscht oder neu erzeugt, sondern weiter gefüllt.
        const string SqlTabellen = @"
            CREATE TABLE IF NOT EXISTS Mitarbeiter (
                MitarbeiterID   INTEGER PRIMARY KEY AUTOINCREMENT,
                Nachname        TEXT NOT NULL,
                Vorname         TEXT NOT NULL,
                UNIQUE (Nachname, Vorname)
            );

            CREATE TABLE IF NOT EXISTS Vorgesetzter (
                VorgesetzterID  INTEGER PRIMARY KEY AUTOINCREMENT,
                Nachname        TEXT NOT NULL UNIQUE
            );

            CREATE TABLE IF NOT EXISTS Taetigkeit (
                TaetigkeitID    INTEGER PRIMARY KEY AUTOINCREMENT,
                Bezeichnung     TEXT NOT NULL UNIQUE
            );

            CREATE TABLE IF NOT EXISTS Arbeitseintrag (
                EintragID       INTEGER PRIMARY KEY AUTOINCREMENT,
                MitarbeiterID   INTEGER NOT NULL REFERENCES Mitarbeiter(MitarbeiterID),
                Datum           TEXT    NOT NULL,   -- Format JJJJ-MM-TT
                Beginn          TEXT    NOT NULL,   -- Format HH:MM
                Ende            TEXT    NOT NULL,   -- Format HH:MM
                DauerMinuten    INTEGER NOT NULL,
                TaetigkeitID    INTEGER NOT NULL REFERENCES Taetigkeit(TaetigkeitID),
                VorgesetzterID  INTEGER NOT NULL REFERENCES Vorgesetzter(VorgesetzterID),
                -- Fehlerbehandlung 1: Ende darf nie vor Beginn liegen
                CHECK (Ende >= Beginn),
                -- Fehlerbehandlung 2: ein identischer Datensatz darf nur einmal vorkommen
                UNIQUE (MitarbeiterID, Datum, Beginn, Ende, TaetigkeitID, VorgesetzterID)
            );";

        /// <summary>
        /// Öffnet die Datenbank im angegebenen Ordner. Existiert sie noch nicht,
        /// legt SQLite die Datei an und die Tabellen werden erstellt.
        /// </summary>
        public static SQLiteConnection Oeffnen(string ordner)
        {
            string pfad = Path.Combine(ordner, Dateiname);
            SQLiteConnection verbindung = new SQLiteConnection("Data Source=" + pfad);
            verbindung.Open();

            using (SQLiteCommand befehl = new SQLiteCommand("PRAGMA foreign_keys = ON;" + SqlTabellen, verbindung))
            {
                befehl.ExecuteNonQuery();
            }
            return verbindung;
        }

        // ------------------------------------------------------------------
        // Import
        // ------------------------------------------------------------------

        /// <summary>
        /// Schreibt einen Eintrag in die Datenbank.
        /// Rückgabe: true = neu eingefügt, false = Doublette (war schon vorhanden).
        /// </summary>
        public static bool EintragSpeichern(SQLiteConnection verbindung, SQLiteTransaction transaktion, Eintrag e)
        {
            long mitarbeiterId = IdHolen(verbindung, transaktion,
                "INSERT OR IGNORE INTO Mitarbeiter (Nachname, Vorname) VALUES ($a, $b);",
                "SELECT MitarbeiterID FROM Mitarbeiter WHERE Nachname = $a AND Vorname = $b;",
                e.Nachname, e.Vorname);

            long taetigkeitId = IdHolen(verbindung, transaktion,
                "INSERT OR IGNORE INTO Taetigkeit (Bezeichnung) VALUES ($a);",
                "SELECT TaetigkeitID FROM Taetigkeit WHERE Bezeichnung = $a;",
                e.Taetigkeit, null);

            long vorgesetzterId = IdHolen(verbindung, transaktion,
                "INSERT OR IGNORE INTO Vorgesetzter (Nachname) VALUES ($a);",
                "SELECT VorgesetzterID FROM Vorgesetzter WHERE Nachname = $a;",
                e.Vorgesetzter, null);

            // INSERT OR IGNORE: verletzt der Datensatz die UNIQUE-Regel (Doublette),
            // wird er einfach nicht eingefügt.
            string sql = @"
                INSERT OR IGNORE INTO Arbeitseintrag
                    (MitarbeiterID, Datum, Beginn, Ende, DauerMinuten, TaetigkeitID, VorgesetzterID)
                VALUES ($ma, $datum, $beginn, $ende, $dauer, $ta, $vg);";

            using (SQLiteCommand befehl = new SQLiteCommand(sql, verbindung, transaktion))
            {
                befehl.Parameters.AddWithValue("$ma", mitarbeiterId);
                befehl.Parameters.AddWithValue("$datum", e.Datum.ToString("yyyy-MM-dd"));
                befehl.Parameters.AddWithValue("$beginn", e.Beginn.ToString(@"hh\:mm"));
                befehl.Parameters.AddWithValue("$ende", e.Ende.ToString(@"hh\:mm"));
                befehl.Parameters.AddWithValue("$dauer", e.DauerMinuten);
                befehl.Parameters.AddWithValue("$ta", taetigkeitId);
                befehl.Parameters.AddWithValue("$vg", vorgesetzterId);

                int geaenderteZeilen = befehl.ExecuteNonQuery();
                return geaenderteZeilen == 1;
            }
        }

        /// <summary>
        /// Legt einen Stammdatensatz (Mitarbeiter, Tätigkeit, Vorgesetzter) an,
        /// falls es ihn noch nicht gibt, und liefert dessen ID zurück.
        /// </summary>
        static long IdHolen(SQLiteConnection verbindung, SQLiteTransaction transaktion,
                            string sqlEinfuegen, string sqlSuchen, string wertA, string wertB)
        {
            using (SQLiteCommand befehl = new SQLiteCommand(sqlEinfuegen, verbindung, transaktion))
            {
                befehl.Parameters.AddWithValue("$a", wertA);
                if (wertB != null) befehl.Parameters.AddWithValue("$b", wertB);
                befehl.ExecuteNonQuery();

                // Gleicher Befehl, gleiche Parameter – nur das SQL wechselt
                befehl.CommandText = sqlSuchen;
                return Convert.ToInt64(befehl.ExecuteScalar());
            }
        }

        // ------------------------------------------------------------------
        // Auswertungen
        // ------------------------------------------------------------------

        /// <summary>/show – komplette Tabelle ausgeben.</summary>
        public static void AllesAnzeigen(SQLiteConnection verbindung)
        {
            string sql = @"
                SELECT m.Nachname, m.Vorname, strftime('%d.%m.%Y', a.Datum), t.Bezeichnung,
                       a.Beginn, a.Ende, a.DauerMinuten, v.Nachname
                FROM Arbeitseintrag a
                JOIN Mitarbeiter  m ON m.MitarbeiterID  = a.MitarbeiterID
                JOIN Taetigkeit   t ON t.TaetigkeitID   = a.TaetigkeitID
                JOIN Vorgesetzter v ON v.VorgesetzterID = a.VorgesetzterID
                ORDER BY m.Nachname, m.Vorname, a.Datum, a.Beginn;";

            string format = "{0,-15} {1,-12} {2,-10} {3,-25} {4,-13} {5,7} {6,-15}";
            string linie = new string('-', 103);
            Console.WriteLine(format, "Nachname", "Vorname", "Datum", "Tätigkeit",
                              "Arbeitszeit", "Zeit(h)", "Vorgesetzter");
            Console.WriteLine(linie);

            int anzahl = 0;
            using (SQLiteCommand befehl = new SQLiteCommand(sql, verbindung))
            using (SQLiteDataReader leser = befehl.ExecuteReader())
            {
                while (leser.Read())
                {
                    Console.WriteLine(format,
                        leser.GetString(0),
                        leser.GetString(1),
                        leser.GetString(2),
                        leser.GetString(3),
                        leser.GetString(4) + " - " + leser.GetString(5),
                        ZeitHelfer.MinutenAlsText(leser.GetInt64(6)),
                        leser.GetString(7));
                    anzahl++;
                }
            }
            Console.WriteLine(linie);
            Console.WriteLine(anzahl + " Einträge");
        }

        /// <summary>/count – Anzahl der Einträge.</summary>
        public static void AnzahlAnzeigen(SQLiteConnection verbindung)
        {
            using (SQLiteCommand befehl = new SQLiteCommand("SELECT COUNT(*) FROM Arbeitseintrag;", verbindung))
            {
                long anzahl = Convert.ToInt64(befehl.ExecuteScalar());
                Console.WriteLine("Anzahl Einträge: " + anzahl);
            }
        }

        /// <summary>
        /// /max – längste Arbeitszeit mit Datum.
        /// Haben mehrere Einträge die gleiche (längste) Dauer, werden alle ausgegeben.
        /// </summary>
        public static void MaximumAnzeigen(SQLiteConnection verbindung)
        {
            string sql = @"
                SELECT a.DauerMinuten, strftime('%d.%m.%Y', a.Datum), a.Beginn, a.Ende,
                       m.Nachname, m.Vorname, t.Bezeichnung
                FROM Arbeitseintrag a
                JOIN Mitarbeiter m ON m.MitarbeiterID = a.MitarbeiterID
                JOIN Taetigkeit  t ON t.TaetigkeitID  = a.TaetigkeitID
                WHERE a.DauerMinuten = (SELECT MAX(DauerMinuten) FROM Arbeitseintrag)
                ORDER BY a.Datum;";

            bool gefunden = false;
            using (SQLiteCommand befehl = new SQLiteCommand(sql, verbindung))
            using (SQLiteDataReader leser = befehl.ExecuteReader())
            {
                while (leser.Read())
                {
                    if (!gefunden)
                    {
                        Console.WriteLine("Längste Arbeitszeit: " +
                            ZeitHelfer.MinutenAlsText(leser.GetInt64(0)) + " h");
                        gefunden = true;
                    }
                    Console.WriteLine("  am " + leser.GetString(1) +
                        " (" + leser.GetString(2) + " - " + leser.GetString(3) + ", " +
                        leser.GetString(4) + ", " + leser.GetString(5) + ", " +
                        leser.GetString(6) + ")");
                }
            }
            if (!gefunden)
            {
                Console.WriteLine("Keine Einträge vorhanden.");
            }
        }

        /// <summary>/time – Gesamtarbeitszeit je Mitarbeiter.</summary>
        public static void ZeitJeMitarbeiterAnzeigen(SQLiteConnection verbindung)
        {
            string sql = @"
                SELECT m.Nachname, m.Vorname, SUM(a.DauerMinuten)
                FROM Mitarbeiter m
                JOIN Arbeitseintrag a ON a.MitarbeiterID = m.MitarbeiterID
                GROUP BY m.MitarbeiterID, m.Nachname, m.Vorname
                ORDER BY m.Nachname, m.Vorname;";

            string format = "{0,-15} {1,-12} {2,12}";
            Console.WriteLine(format, "Nachname", "Vorname", "Gesamt (h)");
            Console.WriteLine(new string('-', 41));
            using (SQLiteCommand befehl = new SQLiteCommand(sql, verbindung))
            using (SQLiteDataReader leser = befehl.ExecuteReader())
            {
                while (leser.Read())
                {
                    Console.WriteLine(format, leser.GetString(0), leser.GetString(1),
                        ZeitHelfer.MinutenAlsText(leser.GetInt64(2)));
                }
            }
        }
    }
}
