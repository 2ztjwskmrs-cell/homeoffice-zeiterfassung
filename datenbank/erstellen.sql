-- Datenbankschema Homeoffice-Zeiterfassung (SQLite)
-- Identisch mit dem Schema im Programm (Datenbank.cs).
-- Das Programm legt die Tabellen selbst an, falls sie fehlen.
-- Dieses Skript dient zur Dokumentation bzw. zum manuellen Anlegen,
-- z. B. im "DB Browser for SQLite".

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
    Datum           TEXT    NOT NULL,   -- JJJJ-MM-TT
    Beginn          TEXT    NOT NULL,   -- HH:MM
    Ende            TEXT    NOT NULL,   -- HH:MM
    DauerMinuten    INTEGER NOT NULL,
    TaetigkeitID    INTEGER NOT NULL REFERENCES Taetigkeit(TaetigkeitID),
    VorgesetzterID  INTEGER NOT NULL REFERENCES Vorgesetzter(VorgesetzterID),
    UNIQUE (MitarbeiterID, Datum, Beginn, Ende, TaetigkeitID, VorgesetzterID)
);

-- Beispielabfragen (entsprechen den Programmbefehlen)

-- /count
-- SELECT COUNT(*) FROM Arbeitseintrag;

-- /max
-- SELECT DauerMinuten, Datum FROM Arbeitseintrag
-- WHERE DauerMinuten = (SELECT MAX(DauerMinuten) FROM Arbeitseintrag);

-- /time
-- SELECT m.Nachname, m.Vorname, SUM(a.DauerMinuten) AS Minuten
-- FROM Mitarbeiter m JOIN Arbeitseintrag a ON a.MitarbeiterID = m.MitarbeiterID
-- GROUP BY m.MitarbeiterID, m.Nachname, m.Vorname;
