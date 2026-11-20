# Grup I · Competitiu

## 🎯 Lucía Ferrándiz · Jugadora PvP competitiva
> Jo organitze tornejos amb els amics. Si una arma no té resposta, el torneig s'acaba en dos dies. Tu fes-me un 1v1 just i et jugue mil partides.

### Primera impressió
Si Ferran diu que "no té gancho", el PvP és el gancho més barat que té: la revenja és contingut infinit sense haver de dissenyar sales noves. Però **no** comence per Ruptura de nucli: nodes, nucli, objectius i reaparició sincronitzats són massa per a la primera prova. Primer un **Duel 1v1/2v2 per rondes** damunt del que ja hi ha (sales per codi + mapa lliure + Mode Creatiu per a muntar arenes).

### Per què no val un multiplicador global
He calculat el TTK (temps per matar 100 HP, des del primer colp) si féssem dany PvP = dany × 7 (el multiplicador que deixa una arma mitjana, ~5,5 DPS, en ~2,5 s):

| Arma | Dany ×7 | Colps | TTK |
|---|---|---|---|
| Llançaflames (2 pellets / 0,06 s) | 14/pellet | 4 tics | **0,18 s** |
| Minigun | 7 | 15 | 0,63 s + 0,6 arrancada = **1,23 s** |
| SMG | 7 | 15 | **0,98 s** |
| Escopeta de prop (+1) | 84/tret | 2 | **0,85 s** |
| Sniper | 84/tret | 2 | 1,5 s (i one-shot amb qualsevol dany previ) |
| Làser | 28 | 4 | 1,5 s, travessa 5 |
| AK47 | 7 | 15 | 1,68 s |
| Llança tempesta | 21 | 5 | 3,4 s |
| Pistola | 7 | 15 | 4,9 s |
| Pedra | 7 | 15 | **9,1 s** |

De 0,18 s a 9 s: impossible. I amb ~300 ms de túnel + ~250 ms de reacció humana, **qualsevol TTK per davall d'1,2 s vol dir morir sense haver-ho vist**. Proposta: un camp `pvpDamage` per arma a `WeaponData` (i `pvpHoming`, `pvpPierce`), amb una franja objectiu de **TTK 1,3–3 s**. La masmorra no es toca quan balancege el PvP, i al revés.

### Sobre els canvis de Ferran
| Canvi | Veredicte | Per què | Número proposat |
|---|---|---|---|
| Nerf Sniper | 👍 | 12 de dany i abast 12 + la càmera s'allunya (abast ≥ 9): veus el rival abans que ell a tu. Això en PvP és trampa. | Masmorra: 12 → 10 (DPS 8 → 6,7). PvP: 55/tret (2 trets, 1,5 s, mai one-shot), línia de mira visible 0,3 s abans de disparar i **zoom de càmera igual per a tots** al mode PvP. |
| Nerf Làser | 👍 | Vel. 35: arriba a 12 d'abast en 0,34 s, és quasi instantani. Travessa 5 = en 2v2 pega als dos rivals alineats. | Masmorra: travessa 5 → 3. PvP: 20/tret, travessa 1 → 5 trets, **2,0 s**. |
| Vareta d'espores sense seguiment | 👍👍 | Les teledirigides són el que més odie: no hi ha esquiva per habilitat. | `homing 2 → 0`. I ull: la **Vareta normal té seguiment 6** (6 rad/s = gira 344°/s, radi de gir ~1,5 a vel. 9). En PvP: `pvpHoming = 0`, 12/tret, 8 trets, 2,45 s. |
| Nerf Llança tempesta | 👍 però pel motiu correcte | No és el DPS (3,5 vs 6,2 del Sable): és l'**amenaça**. Abast 3,5 + dash 1,8 = 5,3 unitats des d'on pots pegar-me; el Sable fa 2 + 0,85 = 2,85. Entra, pega i ix, i a més travessa en línia. A les mans "sembla superior" perquè mai et toquen. | Dash 1,8 → 1,2 (amenaça 4,7) o +0,3 s de CD després del dash. Dany igual. PvP: 22/colp, 5 colps, 3,4 s. |
| Espasa de vidre a 4 | 👍 | En PvP passa de 5 colps (2,2 s) a 4 (1,65 s) amb 28/colp: dins de la franja. | 3 → 4. |

### Les meues propostes
- **Balanç PvP (100 HP):** Llançaflames 2/pellet → 4 per tic → 25 tics, **1,44 s** amb abast 2,8 (ha d'arriscar). Minigun 3/bala → 34 bales = 1,49 + 0,6 arrancada = **2,1 s**. SMG 4 → 25 bales, **1,68 s**. AK 6 → 17 bales, **1,92 s**. Escopeta 8/pellet de prop (+1 → 9) → 2 trets si encertes tots els pellets, si no 3 (**0,85–1,7 s**), i fora l'"abast aleatori 0,3": l'aleatorietat no ha de decidir un duel. Pistola 12, Revolver 25 (4 trets, 1,35 s): les comunes han de ser jugables en PvP. Llançagranades: explosió 25, no més.
- **Mode 1 (v1.3): Duel per rondes.** 1v1 o 2v2, al millor de 5 rondes, 60–90 s per ronda, partida de 3–5 min. Mapa simètric fet amb el Mode Creatiu (2–3 arenes). **Sense cofres ni botí aleatori**: cada jugador tria 2 armes d'una llista tancada de 8 aprovades. Botó de **revenja** al final (mateixa sala, mateix codi). Sense reaparició dins de la ronda: es resol la xarxa més fàcil. Reutilitza sales per codi, lobby i LLEST.
- **Mode 2 (v1.4):** Ruptura de nucli 2v2 quan el Duel funcione. Carambola després (bola física sincronitzada = el més delicat per a la xarxa). Bandera la deixaria per a la següent.
- **Excloses del PvP al principi:** Vareta (fins que `pvpHoming` estiga fet), Disc cometa (3 rebots: morts que no veus venir), Bumerang (2,9 DPS, inútil).

### El que més em preocupa
1. **La xarxa [cal provar].** Ara "cada jugador rep el dany al seu PC" i les bales dels altres són còpies cosmètiques. En PvP això vol dir que la bala del rival t'arriba ~150–300 ms tard (client → amfitrió → client): el que dispara veurà colps "que no compten". Prova: 2 PC per `bore.pub`, 20 duels, apunteu cada "jo li he pegat". Si passa més d'1 de cada 5, el PvP en Internet no aguanta i cal **LAN** (a classe, a Batoi) fins al rework de connexió. Les armes lentes (vel. ≤ 18) es llegeixen millor amb retard que Sniper i Làser.
2. **Trampes:** si el dany el decideix el PC de la víctima, un client modificat pot ignorar-lo. Entre amics, acceptable; en partides públiques, no. L'amfitrió hauria de validar els colps PvP.
3. **No oblidar la masmorra:** el nucli és el cooperatiu. Per això el balanç PvP ha d'anar en camps separats, no tocant el dany de masmorra.

### Nota del joc ara mateix: 4/10 · i amb els canvis: 7/10
(4 perquè per a mi encara no hi ha res a jugar contra persones; 7 si ix el Duel amb la taula `pvpDamage` i la xarxa passa la prova.)
