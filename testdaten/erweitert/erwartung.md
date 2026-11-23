# Erweiterte Testdaten – erwartete Ergebnisse

Automatisch erzeugt von `erzeuge_testdaten.py`. Datenbank vor jedem Szenario löschen.

## Dateien

| Datei | Mitarbeiter | Zeilen | Prüft |
|---|---|---:|---|
| `homeoffice_Becker_Lena_1.xlsx` | Becker, Lena | 1 | 1 Zeile, kürzester Eintrag (0:15) |
| `homeoffice_Nowak_Piotr_1.xlsx` | Nowak, Piotr | 20 | 20 Zeilen Okt./Nov., gemischte Schreibweisen |
| `homeoffice_Nowak_Piotr_2.xlsx` | Nowak, Piotr | 15 | 15 Zeilen, die ersten 5 identisch mit Datei 1 -> 5 Doubletten |
| `homeoffice_Hoffmann_Klaus-Dieter_1.xlsx` | Hoffmann, Klaus-Dieter | 8 | 8 lange Tage (10:00-12:00 h), Bindestrich im Vornamen |
| `homeoffice_Weber_Sophie_1.xlsx` | Weber, Sophie | 24 | Viele kurze Einträge (0:15-1:00), mehrere pro Tag |
| `homeoffice_Krüger_Tim_1.xlsx` | Krüger, Tim | 4 | Längster Eintrag 14:30 (/max), '18-8' vertauscht ohne Minuten, '7.45-9' |
| `homeoffice_Özdemir_Ayşe_1.xlsx` | Özdemir, Ayşe | 4 | ş im Vornamen, Datum als Text, Spalte D falsch, vertauscht + normal = 1 Doublette |
| `homeoffice-van der Berg_Jan_1.xlsx` | van der Berg, Jan | 3 | Leerzeichen im Namen und um Werte, Zeile 1/2 nach Trim identisch -> 1 Doublette |
| `homeoffice_Fischer_Paul_2024-09.xlsx` | Fischer, Paul | 12 | September |
| `homeoffice_Fischer_Paul_2024-10.xlsx` | Fischer, Paul | 15 | Oktober |
| `homeoffice_Fischer_Paul_2024-11.xlsx` | Fischer, Paul | 12 | November, letzte 2 Zeilen nur Vorgesetzter verschieden -> keine Doublette |
| `homeoffice_Meier_Jonas_1.xlsx` | Meier, Jonas | 2 | Gleiche Zeilen wie Meier, Anna -> anderer Mitarbeiter, keine Doublette |
| `homeoffice_Schulz_Tom_1.xlsx` | Schulz, Tom | 0 | Leere Datei (nur Kopf) -> 0 Zeilen, kein Fehler |
| `homeoffice_Wagner_Eva_1.xlsx` | Wagner, Eva | 2 | Fehlerfall: 'ganztags' nicht lesbar -> Datei übersprungen, nichts importiert |
| `homeofficeLange_Ute_1.xlsx` | Lange, Ute | 1 | Kein Trennzeichen nach 'homeoffice' -> wird ignoriert |
| `stunden_homeoffice_Lange_Ute_2.xlsx` | Lange, Ute | 1 | Beginnt nicht mit 'homeoffice' -> wird ignoriert |

## Szenario A: nur dieser Ordner (`/imp testdaten\erweitert`)

| Prüfpunkt | Erwartet |
|---|---|
| `/imp` Dateien gefunden | 14 |
| `/imp` Zeilen gelesen | 120 |
| `/imp` Neu in Datenbank | 113 |
| `/imp` Doubletten | 7 |
| `/imp` Fehlerhafte Dateien | 1 |
| `/imp` erneut | 120 gelesen, 0 neu, 120 Doubletten |
| `/count` | 113 |
| `/max` | 14:30 h am 23.09.2024 (Krüger, 06:00 - 20:30) |

`/time`:

| Mitarbeiter | Einträge | Gesamtzeit |
|---|---:|---:|
| Becker, Lena | 1 | 0:15 |
| Fischer, Paul | 39 | 197:00 |
| Hoffmann, Klaus-Dieter | 8 | 88:15 |
| Krüger, Tim | 4 | 26:15 |
| Meier, Jonas | 2 | 9:30 |
| Nowak, Piotr | 30 | 153:30 |
| Weber, Sophie | 24 | 14:30 |
| van der Berg, Jan | 2 | 7:00 |
| Özdemir, Ayşe | 3 | 13:30 |

## Szenario B: alle Dateien aus `testdaten` und `testdaten\erweitert` in einem Ordner

| Prüfpunkt | Erwartet |
|---|---|
| `/imp` Dateien gefunden | 21 |
| `/imp` Zeilen gelesen | 145 |
| `/imp` Neu in Datenbank | 126 |
| `/imp` Doubletten | 19 |
| `/imp` Fehlerhafte Dateien | 1 |
| `/imp` erneut | 145 gelesen, 0 neu, 145 Doubletten |
| `/count` | 126 |
| `/max` | 14:30 h am 23.09.2024 (Krüger, 06:00 - 20:30) |

`/time`:

| Mitarbeiter | Einträge | Gesamtzeit |
|---|---:|---:|
| Becker, Lena | 1 | 0:15 |
| Fischer, Paul | 39 | 197:00 |
| Hoffmann, Klaus-Dieter | 8 | 88:15 |
| Krüger, Tim | 4 | 26:15 |
| Meier, Anna | 2 | 9:30 |
| Meier, Jonas | 2 | 9:30 |
| Mustermann, Max | 8 | 29:45 |
| Nowak, Piotr | 30 | 153:30 |
| Schröder, Jörg | 3 | 13:30 |
| Weber, Sophie | 24 | 14:30 |
| van der Berg, Jan | 2 | 7:00 |
| Özdemir, Ayşe | 3 | 13:30 |
