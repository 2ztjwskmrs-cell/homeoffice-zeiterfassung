# homeoffice-zeiterfassung

LF08-Projekt (ITS25-2): Automatisiertes Importieren von Excel-Dateien in eine SQLite-Datenbank (ETL-Prozess) mit dem Kommandozeilen-Tool `homeoffice.exe`.

Abgabe: 14.12.2026 vor Beginn der LF08-Stunde (Moodle).

## Befehle

| Aufruf | Funktion |
|---|---|
| `homeoffice.exe /n` | Namen der Projektbeteiligten |
| `homeoffice.exe /imp` | Alle `homeoffice_*.xlsx` bzw. `homeoffice-*.xlsx` im Ordner importieren |
| `homeoffice.exe /show` | Komplette Tabelle |
| `homeoffice.exe /count` | Anzahl der Einträge |
| `homeoffice.exe /max` | Längste Arbeitszeit mit Datum |
| `homeoffice.exe /time` | Gesamtarbeitszeit je Mitarbeiter |

Dateinamen: Nach `homeoffice` muss `_` oder `-` folgen, danach sind beide Trennzeichen gemischt erlaubt (z. B. `Homeoffice-Stundenzettel-Muster.xlsx`, `homeoffice-Meier_Anna_3.xlsx`). Groß-/Kleinschreibung egal.

Gearbeitet wird im **aktuellen Verzeichnis**: dort liegen die Excel-Dateien und dort entsteht `homeoffice.db`. Optional als zweiter Parameter ein anderer Ordner:

```bat
homeoffice.exe /imp C:\Daten\Homeoffice
```

## ETL-Ablauf

| Schritt | Umsetzung | Datei |
|---|---|---|
| **Extract** | Excel-Dateien mit ClosedXML lesen, Name/Vorname aus B3/B4, Daten ab Zeile 7 | `ExcelImport.cs` |
| **Transform** | Arbeitszeit in Beginn/Ende zerlegen, vertauschte Zeiten tauschen, Dauer berechnen | `ZeitHelfer.cs` |

Zellformat egal: Arbeitszeit und Datum werden erkannt, ob die Zelle als Text, Uhrzeit oder Datum formatiert ist. Erlaubt sind z. B. `8-12:00`, `08:00 - 12:15`, `13:00 – 15:30`, `08:00:00-09:30:00` sowie Datum `01.09.2024`, `1.9.2024`, `01.09.24`. Nicht lesbare Zeilen nennen Datei und Zeilennummer, die Datei wird dann komplett übersprungen.
| **Load** | In SQLite schreiben, Doubletten per `UNIQUE` + `INSERT OR IGNORE` verwerfen | `Datenbank.cs` |

Fehlerbehandlung 1 (vertauschte Zeiten): `ZeitHelfer.ZeitspanneLesen` – ist Ende < Beginn, werden beide getauscht.

Fehlerbehandlung 2 (Doubletten): `UNIQUE`-Regel in Tabelle `Arbeitseintrag`. Greift innerhalb einer Datei, zwischen Dateien und bei wiederholtem `/imp`.

Die Datenbank wird nie gelöscht. `CREATE TABLE IF NOT EXISTS` legt Tabellen nur beim allerersten Lauf an, danach wird nur weiter befüllt.

Details: [Datenbankmodell](doku/Datenbankmodell.md) · [Testprotokoll](doku/Testprotokoll.md) · Schema: `SqlTabellen` in [Datenbank.cs](Homeoffice/Datenbank.cs)

## Projektstruktur

```
Homeoffice.sln                     Visual-Studio-Projektmappe
Homeoffice/
  Program.cs                       Befehle auswerten (/n, /imp, ...)
  ExcelImport.cs                   Extract
  ZeitHelfer.cs                    Transform (Zeiten)
  Eintrag.cs                       Datenklasse für eine Zeile
  Datenbank.cs                     Load + Auswertungen
  Properties/launchSettings.json   Startprofile für F5 (Hauptordner, testdaten)
  Properties/PublishProfiles/
    Schulrechner.pubxml            Profil für die abzugebende exe
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

In Visual Studio: `Homeoffice.sln` öffnen, oben neben dem grünen Start-Pfeil ein Startprofil wählen, F5.

| Startprofil | Arbeitet in |
|---|---|
| `imp (Hauptordner)`, `show (Hauptordner)`, `count …`, `max …`, `time …`, `n` | Hauptordner (dort, wo `Homeoffice.sln` liegt) |
| `imp (testdaten)`, `show (testdaten)` | Ordner `testdaten` |

Excel-Dateien also einfach in den Hauptordner legen, `homeoffice.db` entsteht ebenfalls dort. Kein Wechsel nach `bin\Debug\net8.0` nötig. Die Profile stehen in `Homeoffice/Properties/launchSettings.json`.

Die abgegebene `homeoffice.exe` arbeitet unverändert im aktuellen Verzeichnis (bzw. im Ordner aus dem zweiten Parameter).

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

- [x] Namen in `Program.cs` (`Projektbeteiligte`) eintragen
- [ ] In VS 2022 Community öffnen, erstellen, einmal mit F5 testen
- [ ] exe auf einem Schulrechner testen
- [ ] VS-Projekt zippen (ohne `bin`, `obj`, `abgabe`) + `homeoffice.exe` in Moodle hochladen
