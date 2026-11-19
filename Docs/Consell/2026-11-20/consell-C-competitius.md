# Consell de Canica · Grup C · Perfils competitius · Alpha v1.1.1 · 20/11/2026

Dades del dossier (DPS teòric = dany × perdigons / cooldown) i comprovades al codi de BugfixWorkspace:
- `PlayerAttack.TickAttack`: un sol `nextAttackTime`; després de disparar es posa a `Time.time + weapon.cooldown` **de l'arma que ha disparat**. `spinCharge` (arrancada de la Minigun) es reinicia si canvies d'arma.
- `DungeonGenerator.SpawnEnemy`: vida d'enemic normal × (1 + 0,3 × (pis − 1)), elit × 1,5 extra. **Boss: (vida base + 45 × (pis − 1)) × (1 + 0,55 × (jugadors − 1))**.
- `EnemyBoss`: a mitja vida es posa furiós (velocitat × 1,3, temps entre atacs × 0,7, i desbloqueja tots els atacs). Cada colp al jugador fa 1; el jugador té 5 cors i 1 s d'invulnerabilitat després d'un colp.
- Online: ping estimat ~300 ms pel túnel `bore.pub` (ONLINE.md). Mòbil: dos joysticks (moure / apuntar+disparar), 60 FPS.

---

## 🏆 Àlex "Vortex" Gómez · Jugador pro
> "He llegit `TickAttack` abans que el tutorial. Ja sé on està el meta: dins de dos números."

### Primera impressió
El joc té bon feel de base, però el meta és una línia recta: **Llançaflames 33,3 DPS**, 1,5× la Minigun (22,2) i 4× la següent decent. Dos llegendaris (Sniper 2,9 i Làser 6,0) fan menys que el Shuriken comú (10,0). I els bosses escalen només en vida: al pis 8 en solitari el Golem Gegant té 90 + 315 = **405 HP**, amb Pistola són 140 s apretant. Esponja de bales de manual.

### Tier list ara → amb els canvis de Ferran
| Tier | Ara | Amb els canvis |
|---|---|---|
| **S** | Llançaflames (33,3, no hi ha res igual) · Minigun (22,2 a 7,5 d'abast; els 0,6 s d'arrancada són l'únic preu) | Escopeta (14 dany a boca de canó → **16,5 DPS**, s'ha de controlar) [cal provar] |
| **A** | SMG (14,3) · Shuriken (10, comú amb travessa) · Ballesta de cristall (8 + travessa 3) · AK47 (8,3) · Escopeta (8,2) | Llançaflames (segueix sent el millor DPS, ara és "arma de boss" per la durabilitat) · Minigun (si el sobreescalfament deixa ~3 s de foc) · Ballesta de cristall (+ alentiment 0,5 s) · Vareta d'espores (fins a 10,9 DPS, vegeu baix) · Shuriken · SMG |
| **B** | Martell orbital (6,4 radial) · Sabre de corall (6,2 + dash) · Làser (6,0, travessa 5) · Vareta (5,7, segueix) · Ballesta (5,0) · Llançagranades (1 + 4 en àrea) | AK47 · Sniper (si arriba a 9–10 de dany) · Projector de polsos (cadena) · Làser · Sabre · Vareta · Ballesta (sense FOV perd informació) |
| **C** | Espasa de vidre (5,5 a 1,7 d'abast, massa risc) · Revolver (4,4) · Llança de tempesta (3,5) · Projector de polsos (~2,4 amb explosió) | Martell (5 → 4,5 DPS) · Revolver · Espasa de vidre (depén del rework) · Llança (depén del rework) · Llançagranades |
| **D** | Sniper (llegendari amb DPS de pistola) · Bumerang (2,9) · Vareta d'espores (2,7) · Pistola (2,9) · Disc cometa (2,4) · Pedra (2,0) | Pistola · Pedra (més lenta encara: correcte, és la de començar) · Bumerang/Disc fins que es faça el rework |

### Exploits i el cooldown compartit
- **Ara no hi ha exploit de cancel·lar**: el temporitzador guarda el cooldown de l'última arma, així que canviar d'arma no t'estalvia res. És un **càstig**: dispares el Sniper (1,4 s) i la Minigun queda bloquejada 1,4 s. L'únic truc: tir ràpid (Shuriken, 0,3 s) i canvi immediat al Sniper o al Martell, que surten "lliures" després de 0,3 s.
- **El perill ve amb l'arreglo**: si cada arma té el seu cooldown i baixa en segon pla, roteges Sniper → Martell → Làser → Ballesta i sumes DPS (doble pump). Ferran ja ho talla ("només baixa a la mà"). Però vigila: **soltar i tornar a agafar** una arma, o **intercanviar caselles** (`SwapSlots`), no pot reiniciar-la. Proposta: el cooldown viu dins de l'arma (també al terra), i en agafar-la es queda com estava.
- **Durabilitat del Llançaflames**: si soltar-lo i agafar-lo no conserva la barra, el reomplis sense límit. La durabilitat ha d'anar amb l'objecte, també online.
- **Sobreescalfament**: que la calor no es reinicie canviant d'arma (si no, dispares 2,9 s, canvies i tornes freda).

### Sobre els canvis de Ferran
| Canvi | Veredicte | Per què | Número |
|---|---|---|---|
| Llançaflames durabilitat + menys abast | 👍 | Trau-lo del "sempre" sense matar-lo | ~400 tirs (24 s de foc, ~800 dany) i abast 3,2 → 2,8 [cal provar] |
| Minigun sobreescalfament | 👍 | És la contrapartida que falta | Plena en 3 s, s'enfria en 4,5 s, bloqueig de 2 s; con de 9° → 12° |
| Escopeta 2 de dany a prop | 🤔 | 16,5 DPS en una Rar supera la SMG | Només a < 1,5 unitats, o 2 dany als 3 perdigons centrals |
| Sniper més dany | 👍 | Ara és D | 4 → 10 (7,1 DPS) + travessa 1; un mag de 4 HP mor al pis 3 amb un tir |
| Vareta d'espores verí | 🤔 | Si és 1 + 3 de verí: 3 × 4 / 1,1 = 10,9 DPS amb seguiment; massa | Que el total per bola siga 3 (1 d'impacte + 2 de verí) → 8,2 DPS; el verí refresca, no s'acumula |
| Martell 7 → 5 + aim circular | 🤔 | L'aim sí; 5 l'enfonsa a C | Deixar-lo en 6 (5,5 DPS) |
| SMG més V, menys abast | 👍 | Nerf just | 12° → 15°, abast 5 → 4,5 |
| Projector: cadena elèctrica | 👍 | Identitat nova | Màxim 3 salts, radi 3, CD 1,25 → 1,0 |

### Les meues propostes
- **Bosses**: canviar el +45 HP per pis per **+25 HP/pis i +1 atac per cada 3 pisos** (el pool de calma passa de 2 a 3 al pis 4) i bales del boss +8 %/pis fins a +40 %. Dificultat per patró, no per vida.
- **Cooperatiu**: ×1,55 per jugador extra està bé, però amb 4 Llançaflames el boss es fon igual [cal provar].
- **RNG**: el pis 3+ dona 40 % de llegendari a la sala de cofres; amb el Sniper fluix, aquesta tirada és la que decideix la run. Proposta: garantir una arma A o més abans del primer boss.
- Speedrun: un temporitzador de run i de pis visible (encara que siga en local).

### El que més em preocupa
1. Arreglar el cooldown sense obrir el reinici per soltar/agafar o intercanviar caselles.
2. Bosses = vida inflada. Al pis 8 el boss té 4,5× la vida del pis 1 i els mateixos atacs.
3. La durabilitat online: si la porta cada client, desincronització [cal provar].

### Nota del joc ara mateix: 6/10 · i amb els canvis: 7,5/10

---

## ⭐ Iker Sanz · Ve de Brawl Stars
> "Cada arma hauria de tindre personalitat: que la reconegues per com sona abans de vore-la."

### Primera impressió
El control amb dos joysticks està ben pensat (moure a l'esquerra, apuntar i disparar a la dreta). Però moltes armes no tenen rol: el Bumerang i el Disc són la mateixa, la Llança i el Sabre també. I l'online amb ~300 ms de ping es nota molt en un joc de bales [cal provar].

### Rols (per tipus de personatge, no còpia de cap joc)
| Rol | Armes | Què li falta |
|---|---|---|
| Tanc / cos a cos | Martell orbital, Sabre de corall, Espasa de vidre | Que aguante més: menys dany rebut mentre ataca |
| Assassí de dash | Llança de tempesta | Separar-la del Sabre: Llança = estocada llarga que travessa; Sabre = dash |
| Tirador de llarg abast | Sniper, Làser, Ballesta | El Sniper ha de "fer mal de veres" |
| Control de zona | Llançaflames, Llançagranades, Vareta d'espores | Zona que es quede al terra |
| Suport / control | Ballesta de cristall (alentir), Projector (cadena) | Ja ho tenen amb els canvis |
| Metralladora | Minigun, SMG, AK47 | Diferenciar-les: AK = precisió, SMG = mobilitat, Minigun = parada i calor |
| Llançador que torna | Bumerang | El Disc ha de ser una altra cosa: p. ex. rebota a les parets 2 voltes |

### Sobre els canvis de Ferran
| Canvi | Veredicte | Per què | Matís |
|---|---|---|---|
| Sons únics (Sniper, Escopeta, electricitat) | 👍 | La identitat entra per l'orella | Un so de "llest" quan acaba el cooldown llarg |
| Colors de bala (morat, verd, roig, blau) | 👍 | Es llig en un segon | Que l'enemic tinga colors que no es confonguen |
| Rework Bumerang/Disc i Llança/Sabre | 👍 | Dos rols nous gratis | — |
| Durabilitat del Llançaflames | 🤔 | En mòbil cal vore la barra sense mirar baix | Barra damunt del jugador |

### Les meues propostes
- **Super carregada**: una barra que s'ompli fent dany (p. ex. 25 de dany = plena) i es llança amb un botó gran a la dreta. Una per rol: tanc = escut de 2 s; tirador = un tir que travessa tota la sala; zona = bassal de 3 s; suport = alentir tots els enemics a 3 unitats durant 2 s. Al pis 1, 25 de dany són ~6 enemics de 4 HP [cal provar].
- **Mòbil**: botó de disparar amb mode "toc = tir apuntat a l'enemic més pròxim" i "arrossegar = apuntar a mà". Fer la casella d'arma més gran (4 armes + 4 cures són 8 botons). La Minigun en mòbil necessita que mantindre el joystick no la talle.
- **Online**: el ping de 300 ms és el que més mata. Prioritzar la connexió directa (UPnP) i la predicció al client que ja diu ONLINE.md abans que contingut nou. Mostrar el ping a la HUD en partida, no només al lobby.
- **Esdeveniments sense pagament**: "repte de la setmana" amb llavor fixa (tots juguen la mateixa masmorra) i un barret com a premi; modes de cap de setmana (només armes cos a cos, bosses dobles). Res de botiga amb diners reals.
- Rànquing entre amics per pis màxim i temps.

### El que més em preocupa
1. Ping alt + cofres i monedes desincronitzats: en cooperatiu això trenca la confiança [cal provar].
2. Massa botons en pantalla al mòbil.
3. Entre partides només hi ha monedes; falta un objectiu (missions, desbloquejos).

### Nota del joc ara mateix: 6,5/10 · i amb els canvis: 7,5/10

---

## 🌿 Noa Ribes · Ve de Soul Knight
> "23 armes està bé per a una alpha, però el gènere viu de les combinacions, no de la llista."

### Primera impressió
Hi ha base (8 temes, sala d'elit, cambra de ressonància, cofres per raresa), però cada arma és una bala que fa dany i ja. No hi ha estats elementals, no hi ha res que guanyes entre pisos que no siga arma, ni mascota, ni personatges. Els canvis de Ferran (verí, electricitat, alentir) són el primer pas per a tindre **sinergies**.

### Què li falta respecte al gènere
1. **Estats elementals** amb regles comunes (no un efecte per arma).
2. **Buffs/amulets** triats entre pisos (1 de 3).
3. **Mascota** petita que ajude.
4. **Personatges** (caniques) amb una habilitat pròpia.
5. Pisos que es noten diferents: els temes de Ferran (desert, fantasmal, bosc maligne) van per ací.

### Sobre els canvis de Ferran
| Canvi | Veredicte | Per què | Número |
|---|---|---|---|
| Espores verinoses | 👍 | Primer DoT del joc | 3 de dany en 3 s, no s'acumula, es refresca |
| Cadena elèctrica | 👍 | Control de grups | 2 + 1 per salt, 3 salts |
| Alentir del cristall | 👍 | Primer control | 0,5 s és poc per notar-ho: 0,8 s al 40 % [cal provar] |
| Llançaflames | 🤔 | Té foc visual però no crema | Que deixe "cremat": 1 de dany als 2 s |

### Sinergies (idees originals)
- **Verí + electricitat = "descàrrega tòxica"**: si la cadena toca un enemic enverinat, el verí salta també a l'enemic següent.
- **Alentir + Llançaflames**: l'enemic alentit queda més temps dins del foc; fàcil i se sent bé.
- **Electricitat + aigua**: enemics damunt de l'aigua reben +1 salt de cadena (aprofita que ja hi ha aigua al mapa).
- **Verí + Bolet verinós (enemic)**: els enemics que ja són de verí (BoletVeri, Esporer) són immunes al verí; dona sentit a canviar d'arma.

### Amulets (1 de 3 en acabar cada pis)
| Amulet | Efecte | Per què aquest número |
|---|---|---|
| Escut | Bloqueja 1 colp cada 15 s | Amb 5 cors, és com +1 cor per sala llarga |
| Força | Cada 3r impacte fa +1 de dany | El dany és enter: +1 sempre seria +100 % en armes d'1 de dany |
| Cadència | −12 % de cooldown, mínim 0,08 s | Sense mínim, el Llançaflames passaria de 33 a 38 DPS |
| Tòxic | El verí dura +1 s | Sinergia amb les espores |
| Conductor | La cadena fa +1 salt | Sinergia amb el Projector |
| Gel | Els alentiments duren +50 % | Sinergia amb el cristall |
Màxim 1 de cada i 4 en total per run, perquè el Vortex no faça una build infinita [cal provar].

### Mascota i personatges
- **Mascota**: una canica xicoteta que dispara 1 de dany cada 1,5 s (0,7 DPS, menys que la Pedra), però recull monedes. Ajuda sense trencar res.
- **Caniques jugables** amb 1 passiva: vidre (+1 dany al primer colp a cada enemic, −1 cor), acer (+1 cor, −10 % de velocitat), ull de gat (+20 % d'abast). Encaixa amb les 25 skins que ja hi ha.

### El que més em preocupa
1. Que cada estat es programe dins de cada arma: millor un sistema comú d'estats (verí, cremat, alentit, electrocutat) a `EnemyHealth`.
2. Online: qui aplica el verí? Ha de ser l'amfitrió, si no, cada jugador veu una vida diferent [cal provar].
3. Les llegendàries encara no se senten llegendàries (Sniper, Làser).

### Nota del joc ara mateix: 6/10 · i amb els canvis: 7,5/10
