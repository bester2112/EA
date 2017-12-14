# EA

In diesem Projekt existiert eine Implementierung einer Kalibrierung von drei Signalen (kurz, mittel und lang) über einen Evolutionären Algorithmus.

Zu Beginn wird der Benutzer benutzer aufgefordert eine Reihe von Signalen zu bewerten ob diese kurz mittel oder lang sind. Dies wird benutzt, um die Intervallgrenzen für die jeweiligen Signaltypen (kurz, mittel und lang) zu erstellen. 

Nach dem die Intervallgrenzen ermittelt sind, wird eine zufällige Population erstellt die sich in diesen Intervallgrenzen für jeden Typ befinden. Jetzt beginnt der Evolutionäre Algorithmus

Nachdem so eine Population vorhanden ist, wird die zufällig dem Benutzer ein Signal dargestellt, es wird ihm gesagt was es für ein Signaltyp ist und er soll anhand einem Bewertungsskala diesen Signaltyp bewerten.

Nach der Bewertung wird der Fitnesswert berechnet. 

Anhand des Fitnesswerts wird die ein pool mit den Signalen gefüllt. Von N mal zwei Signale zufällig gezogen werden und diese werden gecrossovered und mutiert. Das daraus erzeugte Signal ist ein neues Signal für die nächste Generatio, die der Benutzer bewerten soll. 