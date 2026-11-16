# homeoffice-zeiterfassung

LF08-Projekt (ITS25-2): Automatisiertes Importieren von Excel-Dateien in eine SQLite-Datenbank (ETL-Prozess) mit dem Kommandozeilen-Tool `homeoffice.exe`.

Abgabe: 14.12.2026 vor Beginn der LF08-Stunde (Moodle).

## Befehle

| Aufruf | Funktion |
|---|---|
| `homeoffice.exe /n` | Namen der Projektbeteiligten |
| `homeoffice.exe /imp` | Alle `homeoffice_*.xlsx` im Ordner importieren |
| `homeoffice.exe /show` | Komplette Tabelle |
| `homeoffice.exe /count` | Anzahl der Einträge |
| `homeoffice.exe /max` | Längste Arbeitszeit mit Datum |
| `homeoffice.exe /time` | Gesamtarbeitszeit je Mitarbeiter |

Gearbeitet wird im **aktuellen Verzeichnis**: dort liegen die Excel-Dateien und dort entsteht `homeoffice.db`. Optional als zweiter Parameter ein anderer Ordner:

```bat
homeoffice.exe /imp C:\Daten\Homeoffice
```

## ETL-Ablauf

| Schritt | Umsetzung | Datei |
|---|---|---|
| **Extract** | Excel-Dateien mit ClosedXML lesen, Name/Vorname aus B3/B4, Daten ab Zeile 7 | `ExcelImport.cs` |
| **Transform** | Arbeitszeit in Beginn/Ende zerlegen, vertauschte Zeiten tauschen, Dauer berechnen | `ZeitHelfer.cs` |
| **Load** | In SQLite schreiben, Doubletten per `UNIQUE` + `INSERT OR IGNORE` verwerfen | `Datenbank.cs` |

Fehlerbehandlung 1 (vertauschte Zeiten): `ZeitHelfer.ZeitspanneLesen` – ist Ende < Beginn, werden beide getauscht.

Fehlerbehandlung 2 (Doubletten): `UNIQUE`-Regel in Tabelle `Arbeitseintrag`. Greift innerhalb einer Datei, zwischen Dateien und bei wiederholtem `/imp`.

Die Datenbank wird nie gelöscht. `CREATE TABLE IF NOT EXISTS` legt Tabellen nur beim allerersten Lauf an, danach wird nur weiter befüllt.

Details: [Datenbankmodell](doku/Datenbankmodell.md) · [Testprotokoll](doku/Testprotokoll.md) · [SQL-Skript](datenbank/erstellen.sql)

## Projektstruktur

```
Homeoffice.sln                     Visual-Studio-Projektmappe
Homeoffice/
  Program.cs                       Befehle auswerten (/n, /imp, ...)
  ExcelImport.cs                   Extract
  ZeitHelfer.cs                    Transform (Zeiten)
  Eintrag.cs                       Datenklasse für eine Zeile
  Datenbank.cs                     Load + Auswertungen
  Properties/PublishProfiles/
    Schulrechner.pubxml            Profil für die abzugebende exe
datenbank/erstellen.sql            Schema zum Nachlesen / für DB Browser
doku/                              Datenbankmodell, Testprotokoll
testdaten/                         Excel-Testdateien inkl. Fehlerfälle
```

## Voraussetzungen

- Visual Studio 2022 Community mit Workload „.NET-Desktopentwicklung" (.NET 8)
- NuGet-Pakete (werden beim Erstellen automatisch geladen):
  - `ClosedXML` – Excel lesen, ohne dass Excel installiert sein muss (wie Unterrichtsbeispiel `ShowExcelAutomationClosedXML`)
  - `System.Data.SQLite.Core` – SQLite-Zugriff mit `SQLiteConnection`/`SQLiteCommand` (wie Unterrichtsbeispiel `ShowDBAutomation`)

Abweichung zu den Unterrichtsbeispielen: Die Beispiele nutzen .NET Framework 4.7.2, dieses Projekt .NET 8. Klassen und Methoden von ClosedXML und System.Data.SQLite sind identisch.

## Bauen und testen

In Visual Studio: `Homeoffice.sln` öffnen → Projekteigenschaften → Debuggen → Befehlszeilenargumente z. B. `/imp ..\..\..\..\testdaten` → F5.

Oder in der Konsole:

```bat
dotnet build
cd testdaten
..\Homeoffice\bin\Debug\net8.0\homeoffice.exe /imp
..\Homeoffice\bin\Debug\net8.0\homeoffice.exe /show
```

## Abgabe-exe für die Schulrechner erzeugen

Visual Studio: Rechtsklick auf Projekt → **Veröffentlichen** → Profil **Schulrechner** → Veröffentlichen.

Konsole:

```bat
dotnet publish Homeoffice\Homeoffice.csproj -p:PublishProfile=Schulrechner
```

Ergebnis: `abgabe\homeoffice.exe` (ca. 12,7 MB). Eine einzige Datei inkl. ClosedXML und SQLite, benötigt die .NET-8-Runtime (auf den Schulrechnern vorhanden).

Falls ein Rechner ohne .NET 8 die exe ausführen soll (ca. 37 MB, Runtime eingebaut):

```bat
dotnet publish Homeoffice\Homeoffice.csproj -p:PublishProfile=Schulrechner --self-contained true -p:EnableCompressionInSingleFile=true
```

## Vor der Abgabe

- [ ] Namen in `Program.cs` (`Projektbeteiligte`) eintragen
- [ ] In VS 2022 Community öffnen, erstellen, einmal mit F5 testen
- [ ] exe auf einem Schulrechner testen
- [ ] VS-Projekt zippen (ohne `bin`, `obj`, `abgabe`) + `homeoffice.exe` in Moodle hochladen
