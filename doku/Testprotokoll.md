# Testprotokoll

Testdaten: Ordner [`testdaten/`](../testdaten). Datenbank vor Test 1 gelöscht.

## Testdateien

| Datei | Inhalt | Prüft |
|---|---|---|
| `homeoffice_Mustermann_Max_1.xlsx` | Original-Muster, 7 Zeilen | `8-12:00` (Stunde ohne Minuten), `15:00-13:00` (vertauscht), Zeile 12/13 identisch |
| `homeoffice_Mustermann_Max_2.xlsx` | 3 Zeilen | Zeile 1 identisch mit Datei 1 (Doublette über Dateigrenze), `11:00 - 9:30` vertauscht |
| `homeoffice_Schröder_Jörg_1.xlsx` | 4 Zeilen | Umlaute im Namen, doppelte Zeile, `17:00 - 9:00` vertauscht, `10-11:30` |
| `homeoffice_Meier_Anna_1.xlsx` | 2 Zeilen | Gleiche Tätigkeit/Zeit wie Mustermann → darf **keine** Doublette sein |
| `homeoffice_Meier_Anna_2.xlsx` | 1 Zeile | Wie Meier_1 Zeile 2, aber Zeiten vertauscht → **ist** Doublette |
| `Homeoffice-Stundenzettel-Muster.xlsx` | 7 Zeilen, erhaltene Musterdatei | Bindestriche + Großschreibung im Dateinamen werden erkannt; Inhalt wie Mustermann_1 → 7 Doubletten |
| `homeoffice-Meier_Anna_3.xlsx` | 1 Zeile, Kopie von Meier_2 | Mischform `-`/`_` im Dateinamen wird erkannt → 1 Doublette |
| `homeoffice_Weber_Lena_1.xlsx` | 5 Zeilen, **alle Zellen als Text formatiert** | Datum als Text (`01.09.2024`, `2.9.2024`, `04.09.24`), Arbeitszeit `13:00 – 15:30` (Gedankenstrich), `9-10`, ` 16:00 - 14:00 ` (vertauscht, Leerzeichen), `08:00:00-09:30:00` |

## Testfälle

| Nr. | Aufruf | Erwartet | Ergebnis |
|---|---|---|---|
| 1 | `homeoffice.exe /n` | Namen der Projektbeteiligten | ok |
| 2 | `homeoffice.exe /imp` | 8 Dateien gefunden, 30 Zeilen gelesen, 18 neu, 12 Doubletten | ok |
| 3 | `homeoffice.exe /imp` (erneut) | 30 gelesen, 0 neu, 30 Doubletten, Datenbank nicht neu erzeugt | ok |
| 4 | `homeoffice.exe /show` | 18 Einträge, alle Zeiten Beginn < Ende | ok |
| 5 | `homeoffice.exe /count` | `Anzahl Einträge: 18` | ok |
| 6 | `homeoffice.exe /max` | 8:00 h am 03.09.2024 (Schröder, 09:00 - 17:00) | ok |
| 7 | `homeoffice.exe /time` | Meier 9:30, Mustermann 29:45, Schröder 13:30, Weber 11:00 | ok |
| 8 | `homeoffice.exe /xyz` | Meldung „Unbekannter Befehl" + Hilfe | ok |
| 9 | `homeoffice.exe /count` ohne Datenbank | Hinweis „zuerst /imp ausführen" | ok |
| 10 | `homeoffice.exe` ohne Parameter | Hilfe | ok |
| 11 | `/imp` mit `~$homeoffice-x.xlsx` und `homeofficex.xlsx` im Ordner | Beide ignoriert (Sperrdatei bzw. kein Trennzeichen), weiterhin 8 Dateien | ok |
| 12 | Musterdatei mit allen Zellen auf Format Text umgestellt, `/imp` in leerem Ordner | 7 gelesen, 6 neu, 1 Doublette, `/time` Mustermann 21:45 | ok |
| 13 | Datei mit Datum `31.02.2024` bzw. Arbeitszeit `9 bis 10` | Datei übersprungen mit Meldung „Zeile 9: Datum nicht lesbar" bzw. „Zeile 10: Arbeitszeit nicht lesbar" | ok |

## Nachrechnung Testfall 7

Mustermann: 4:00 + 2:00 + 4:15 + 5:00 + 3:00 + 3:30 (Datei 1, Doublette nur einmal) + 6:30 + 1:30 (Datei 2) = **29:45**

Meier: 4:00 + 5:30 = **9:30**

Schröder: 4:00 + 8:00 + 1:30 = **13:30**

Weber: 4:00 + 2:30 + 1:00 + 2:00 + 1:30 = **11:00**

## Ausgabe `/show`

```
Nachname        Vorname      Datum      Tätigkeit                 Arbeitszeit   Zeit(h) Vorgesetzter
-------------------------------------------------------------------------------------------------------
Meier           Anna         01.09.2024 Scripterstellung          08:00 - 12:00    4:00 Hansen
Meier           Anna         06.09.2024 Switchkonfiguration       07:30 - 13:00    5:30 Müller
Mustermann      Max          01.09.2024 Scripterstellung          08:00 - 12:00    4:00 Hansen
Mustermann      Max          01.09.2024 Online-Meeting            13:00 - 15:00    2:00 Hansen
Mustermann      Max          02.09.2024 Scripterstellung          08:00 - 12:15    4:15 Müller
Mustermann      Max          02.09.2024 Scripterstellung          13:00 - 18:00    5:00 Müller
Mustermann      Max          03.09.2024 Hotlineunterstützung      09:00 - 12:00    3:00 Petersen
Mustermann      Max          04.09.2024 Switchkonfiguration       13:00 - 16:30    3:30 Müller
Mustermann      Max          09.09.2024 Scripterstellung          08:00 - 14:30    6:30 Hansen
Mustermann      Max          10.09.2024 Online-Meeting            09:30 - 11:00    1:30 Hansen
Schröder        Jörg         02.09.2024 Hotlineunterstützung      08:00 - 12:00    4:00 Petersen
Schröder        Jörg         03.09.2024 Netzwerkdokumentation     09:00 - 17:00    8:00 Franzen
Schröder        Jörg         05.09.2024 Online-Meeting            10:00 - 11:30    1:30 Müller
Weber           Lena         01.09.2024 Scripterstellung          08:00 - 12:00    4:00 Hansen
Weber           Lena         02.09.2024 Online-Meeting            13:00 - 15:30    2:30 Hansen
Weber           Lena         03.09.2024 Switchkonfiguration       09:00 - 10:00    1:00 Müller
Weber           Lena         04.09.2024 Hotlineunterstützung      14:00 - 16:00    2:00 Petersen
Weber           Lena         05.09.2024 Netzwerkdokumentation     08:00 - 09:30    1:30 Franzen
-------------------------------------------------------------------------------------------------------
18 Einträge
```

## Offen

- Test in Visual Studio 2022 Community unter Windows.
- Test der veröffentlichten `homeoffice.exe` auf einem Schulrechner.
