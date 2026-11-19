# Consell de Canica · Grup A (balanç i tècnic) · Alpha v1.1.1 · 2026-11-20

Fonts: dossier de dades (stats de les 23 armes i 15 enemics), conclusions de Ferran, i el codi de `Builds/BugfixWorkspace` (PlayerAttack.cs, WeaponData.cs, Projectile.cs, EnemyHealth.cs, Fx.cs, EnemyBoss.cs, EnemySplitter.cs, EnemyBullet.cs, CameraFollow.cs). Tot el que no ix d'ací va marcat.

Convencions dels càlculs:
- DPS = dany × perdigons / cooldown, comptant que peguen tots (el millor cas).
- TTK = (trets necessaris − 1) × CD, perquè el primer tret ix a l'instant.
- Comprovat al codi: `EnemyHealth.TakeHit` no té invulnerabilitat després d'un colp, així que el DPS teòric s'aplica tal qual (el Llançaflames fa de veritat els seus 33 DPS).
- Comprovat al codi: l'explosió (`Fx.Explode`) també pega a l'enemic que rep el tret directe. Per tant, el Llançagranades fa 1 + 4 = 5 per tret.
- Referència: el puny fa 1 / 0,35 = **2,9 DPS**.

---

## 📊 Marta Ferrer · Desenvolupadora balancejadora
> "Done'm un full de càlcul i et dic quina arma sobra. Ací en sobren unes quantes, i en falten d'altres."

### Primera impressió
Hi ha bona varietat de mecàniques (pierce, retorn, homing, dash, explosió, spin-up), però la corba de raresa no vol dir res. El **Shuriken comú (10 DPS)** guanya a totes les rares. El **Sniper llegendari (2,9 DPS)** empata amb la Pistola comuna. El **Llançaflames (33,3 DPS)** fa 1,5× la Minigun i 4× l'AK47. Per raresa, el DPS mitjà de cada grup és: comú 5,0 · poc comú 8,3 · rar 4,0 · èpic 6,5 (comptant el Llançagranades a 5) · llegendari 16,1. La raresa rara està per davall de la comuna.

**TTK de referència** contra Golem (12 HP, el comú amb més vida) i GolemGegant (90 HP, el boss amb més vida):

| Arma | DPS | TTK Golem | TTK Gegant |
|---|---|---|---|
| Llançaflames | 33,3 | 0,30 s | 2,6 s |
| Minigun (+0,6 s d'arrancada) | 22,2 | 1,1 s | 4,6 s |
| SMG | 14,3 | 0,77 s | 6,2 s |
| Shuriken (comú!) | 10,0 | 0,9 s | 8,7 s |
| AK47 | 8,3 | 1,3 s | 10,7 s |
| Laser | 6,0 | 1,5 s | 14,5 s |
| Sniper | 2,9 | 2,8 s | 30,8 s |

### Sobre els canvis de Ferran
| Canvi | Veredicte | Per què | Número o matís proposat |
|---|---|---|---|
| Llançaflames: durabilitat + menys rang | 👍 | Ara mateix 33 DPS sense cap cost. La durabilitat el converteix en "burst consumible" | Durabilitat **300 trets** (18 s de foc, 600 de dany màxim, uns 6 Gegants). Rang 3,2 → **2,8**. El DPS el deixaria igual |
| Minigun: sobreescalfament + con més obert | 👍 | Li dona contrapartida al 22 DPS | +1 de calor per tret, topall 60 (2,7 s de foc). Refreda 15/s (escalfa 22/s). Si arriba al topall, bloqueig de **2 s**. Con 9° → **12°** |
| Ballesta: llevar el zoom de FOV | 👍 | Té rang 9, just al llindar `>= 9` de CameraFollow | Rang 9 → **8,5**, o pujar el llindar a `>= 10` (així només Sniper i Laser, que tenen 12) |
| Ballesta de cristall: menys FOV + alentiment 0,5 s | 👍 | Fa 8 DPS, té pierce 3 i és èpica: correcte. L'alentiment li dona identitat | Rang **8,5**. Alentiment del **40 %** durant 0,5 s |
| Bumerang / Disc: duplicats | 👍 | Tots dos són rars, de 2 de dany, amb retorn, i fan 2,9 i 2,4 DPS | Cal rework del Disc (vegeu els rols). Si peguen a l'anada i a la tornada, fan 5,7 i 4,7 de DPS efectiu |
| Escopeta: 2 de dany de prop | 🤔 | Amb 7 perdigons × 2 / 0,85 = **16,5 DPS**, una rara passaria la SMG èpica | **6 perdigons**, 2 de dany per perdigó a menys d'1,5 u i 1 més lluny: **14,1 de prop / 7,1 de lluny**. Velocitat 15 → 12 |
| Espasa de vidre: V més gran o rework | 🤔 | No és "inservible" pels números (5,5 DPS, més que el Revolver). El problema és l'abast: 1,7 u contra enemics que fan dany per contacte [cal provar] | Rang 1,7 → **2,0**, arc 100° → **120°** abans de fer cap rework |
| Projector de polsos: cadena elèctrica | 👍 | Ara fa **0,8 DPS** directe (2,4 comptant l'explosió): és la pitjor arma del joc. A més, és mig Llançagranades | Bala 2, salt 1, **màxim 3 salts**, radi de salt 2,5 u, rang 5 → **5,5**. CD 1,25 → **0,8**: fa 2,5 DPS contra un sol enemic i 6,25 contra grups |
| Llança / Sable: quasi iguals | 👍 | A més, la Llança èpica (3,5 DPS) fa menys que el Sable rar (6,2) | Canviar la Llança (vegeu els rols) |
| Martell: aim circular + dany 5 | 🤔 | Amb 7 mata l'Escarabat (7 HP) d'un colp i el Golem en 2. Amb 5, l'Escarabat necessita 2 colps i el Golem 3 | Millor **6** (5,5 DPS): el Golem continua caient en 2. L'aim circular, sí o sí |
| Pedra: encara més lenta | 👍 | Ja fa 2,0 DPS, menys que el puny (2,9) | CD 0,5 → **0,65** (1,5 DPS). Que siga una arma de broma, i que es diga |
| SMG: V més oberta, menys rang | 👍 | El nerf xicotet és correcte | V 12° → **15°**, rang 5 → **4,5**. El DPS teòric no canvia, el que baixa és la precisió [cal provar] |
| Sniper: més dany, menys FOV, so | 👍 | Llegendari amb 2,9 DPS i 31 s per matar el Gegant | Dany 4 → **12**, CD **1,5** (8 DPS). Mata d'un tret qualsevol enemic normal, inclòs el Golem. El FOV, mantindre'l: amb rang 12 és el que el fa sniper |
| Vareta d'espores: verí | 🤔 | Si el verí s'acumula, fa 3 × 3 / 1,1 = **8,2 DPS** (és rara). Si només es renova, fa ~1 DPS | Verí acumulable fins a 3, 1 de dany directe + 2 de verí en 2 s. V 20° → **14°**, velocitat 6 → **9** |

### Les meues propostes
- **Balanç (que Ferran no ha dit):**
  - **Shuriken** (comú): CD 0,3 → **0,45** (6,7 DPS), o passar-lo a PocComú.
  - **Laser** (llegendari, 6 DPS): dany 3 → **4** (8 DPS). Amb pierce 5 ja fa de "rompe-files".
  - **AK47**: el deixaria. Està en 8,3, just per damunt de les rares, i és l'única PocComú.
  - **Llançagranades**: 5 DPS amb àrea, correcte.
- **Rol de cada arma (n'hi ha 23)** · ⚠ = duplicat
  - Pedra: arma de broma.
  - Pistola: inicial, de precisió.
  - Revolver: tret pesat inicial. ⚠ amb la Pistola: separar-les per cadència i dany.
  - Shuriken: ventall que travessa.
  - Espasa de vidre: melee ràpid.
  - AK47: ràfega precisa.
  - SMG: spray de prop.
  - Minigun: torreta amb calor. ⚠ Les tres (AK47, SMG, Minigun) són "mànegues de bales". Es diferencien per l'angle (4°, 12°, 9°) i per la calor.
  - Escopeta: burst de prop.
  - Ballesta: travessa en línia.
  - Ballesta de cristall: control (alentiment).
  - Bumerang: va i torna.
  - Disc de cometa ⚠: **proposta de rebot a les parets** (3 rebots, sense retorn).
  - Sable de corall: duelista amb dash curt.
  - Llança de tempesta ⚠: **estocada llarga en línia** (rang 3,5, arc 25°, travessa, dash 1,8).
  - Martell orbital: àrea al voltant (radial).
  - Projector de polsos: cadena elèctrica. ⚠ Avui és mig Llançagranades.
  - Llançagranades: àrea a distància.
  - Vareta: homing segur.
  - Vareta d'espores: verí (DoT).
  - Sniper: un tret, una mort.
  - Laser: travessa en línia a llarga distància.
  - Llançaflames: burst consumible.
- **Enemics:** amb 5 cors i bosses de 65–90 HP, les armes de 2–3 DPS no poden acabar un boss (més de 30 s). Proposaria un mínim de ~4 DPS per a qualsevol arma rara o millor.
- **Tècnic:** la durabilitat **no es pot guardar al `WeaponData`**. És un ScriptableObject compartit, i dins de l'editor el canvi es grava a l'asset. Cal un estat per instància que viatge amb `RunState.Weapons` i amb `ItemPickup`.

### El que més em preocupa
1. Mentre el cooldown siga compartit, cap número d'aquesta taula es pot validar, perquè canviar d'arma arrossega el CD de l'anterior.
2. L'Escopeta i la Vareta d'espores poden passar-se de DPS si no s'acoten (perdigons i acumulació del verí). **[cal provar]**
3. La durabilitat pot frustrar si el jugador no veu la barra. **[cal provar]**

### Nota del joc ara mateix: 6/10 · i amb els canvis: 7,5/10

---

## 🧪 Dani Vidal · Tester especialista (Brawl Stars / Soul Knight)
> "Si es pot trencar en tres passos, ho trencaré en dos. Primer l'estabilitat, després el contingut."

### Primera impressió
El combat és sòlid, però hi ha dos softlocks (els fills dels bosses i el loot damunt del portal) i desincronitzacions online amb diners pel mig. A més, el cooldown compartit **bloqueja qualsevol prova de balanç**.

### Sobre els canvis de Ferran
| Canvi | Veredicte | Per què | Número o matís proposat |
|---|---|---|---|
| Cooldown per arma | 👍 | És la base de tot | Vegeu "Com arreglar-lo" |
| Durabilitat del Llançaflames | 🤔 | Afegeix estat per instància: mort, drop, online, pas de pis | Definir-ho abans de programar-ho |
| Sobreescalfament de la Minigun | 🤔 | La calor baixa també amb l'arma amagada? | Que la calor es refrede sempre. El bloqueig, que també compte només a la mà |
| Cadena del Projector | 👍 | Cal un "sense repetir enemic" per tret | Un HashSet per tret, com el `alreadyHit` del Projectile |

### Bugs
| # | Gravetat | Bug | Passos per reproduir | Esperat |
|---|---|---|---|---|
| 1 | 🟠 (bloqueja el balanç) | **Cooldown compartit**. `PlayerAttack` té un sol `nextAttackTime` (línies 45 i 137–151). L'aim mostra el `lastCooldownDuration` de l'última arma | 1) Sniper a la casella 1 i Pistola a la 2. 2) Dispara el Sniper. 3) Prem 2 i dispara: la Pistola no tira fins que passen 1,4 s | Cada casella té el seu cooldown i la barra de l'aim mostra el de l'arma triada |
| 2 | 🔴 | **Fills del boss fora de la sala → softlock** | 1) Porta el boss a una paret o cantó. 2) Espera el `Summon`. 3) Algun fill apareix darrere de la paret | Tots els fills naixen dins de la sala. Si un queda fora més de 3 s, es teleporta dins o compta com a mort |
| 3 | 🔴 | **Loot damunt del portal post-boss** | 1) Mata el boss. 2) Mor o solta una arma (Z) al costat del portal. 3) No es pot agafar | Els drops s'aparten de les caselles del portal. Afegir el portal de tornada |
| 4 | 🔴 (economia) | **Moneda duplicada online** | 1) Dos jugadors amb ping alt. 2) Agafen la mateixa moneda alhora. 3) La compten tots dos | Pickup autoritari: el client ho demana, l'amfitrió confirma i dona l'ordre de despawn |
| 5 | 🟠 | **Cofre fantasma** (el va veure Herrera) | 1) Un cofre s'obri mentre un client té lag o acaba d'entrar. 2) El client el veu tancat i "obert" alhora | L'amfitrió envia l'estat dels cofres als clients que entren tard o es resincronitzen |
| 6 | 🟠 | **Les bales enemigues xoquen amb l'aigua** | Un Tirador dispara per damunt d'un estany | La bala passa. Ignorar la capa/tag d'aigua a `EnemyBullet` |
| 7 | 🟢 | El barret d'astronauta queda per damunt d'altres jugadors | 2 jugadors, un davant de l'altre en Y | El barret fa servir el mateix sorting que el seu jugador + 1 |
| 8 | 🟢 | El xat queda darrere dels arbres | Parla al costat d'un arbre | Decisió de disseny. Jo el posaria en una capa d'UI |

**Notes del codi per als bugs:**
- **Bug 2:** `EnemyBoss.Summon` posa els fills a 1,6 u del boss, en una direcció a l'atzar, i després fa `TileNav.NearestFree`. No mira els límits de la sala, així que la "casella lliure més pròxima" pot quedar a l'altra banda d'una paret. `EnemySplitter` també pot deixar fills fora: només comprova `IsBlockedWorld`, i l'empenta del colp els pot moure. No he vist cap codi de límits de sala que ho evite.
- **Bug 4 i bug 5:** no he revisat la carpeta `Net`. Que no hi haja confirmació de l'amfitrió és una hipòtesi.
- **Bug 6:** `EnemyBullet` (línia 50) explota amb qualsevol collider que no siga trigger. Si l'aigua té un collider sòlid, la bala explota. Ho he deduït del codi, sense comprovar el prefab de l'aigua. `Projectile`, la bala del jugador, fa el mateix: cal decidir si les bales del jugador també passen per damunt de l'aigua.

**Com arreglar el bug 1 (cooldown per arma):**
- Afegir `float[] slotCooldown = new float[4]` i `float[] slotDuration = new float[4]`, per casella i **no** al `WeaponData`, que és compartit. Dos Snipers en dues caselles han de tindre CD independents.
- A `Update`, restar `Time.deltaTime` **només** a `slotCooldown[selectedSlot]`. Així baixa només a la mà i s'evita el "doble pump".
- A `TickAttack`, disparar si `slotCooldown[selectedSlot] <= 0` i després posar `slotCooldown[selectedSlot] = weapon.cooldown` (o `fistCooldown` si la casella està buida).
- A `AddWeapon(i)`, posar `slotCooldown[i] = 0`: una arma que agafes del terra comença carregada, com demana Ferran.
- A `SwapSlots`, intercanviar també els dos floats.
- A `DropSlot` i `DropAllOnDeath`, posar la casella a 0.
- A l'aim, calcular el `progress` amb `slotCooldown[selectedSlot] / slotDuration[selectedSlot]`.
- ⚠ **Exploit nou:** soltar l'arma amb Z i tornar-la a agafar reiniciaria el CD del Sniper. Cal guardar el CD que li queda dins de l'`ItemPickup`, o posar un retard de recollida d'almenys el CD de l'arma.
- **[cal provar]** que "només baixa a la mà" no faça pesat el Sniper: canviar a la Pistola i tornar deixa el Sniper igual de descarregat. L'alternativa és que baixe a totes les caselles i que canviar d'arma afegisca un retard de 0,15 s.

### Pla de proves per als canvis de balanç
Al Mode Creatiu, contra un Golem (12 HP) i un GolemGegant (90 HP), amb cronòmetre:
1. **Cooldown:**
   - Sniper → Pistola → Sniper: cada una amb el seu CD, i la barra de l'aim correcta.
   - Dos Snipers en les caselles 1 i 2.
   - Soltar i recollir una arma.
   - Passar de pis amb una arma a mitja càrrega.
   - Morir online amb una arma a mitja càrrega.
2. **Llançaflames:**
   - Comptar els trets fins que es trenca (esperat: 300).
   - Que la barra es veja a la hotbar.
   - Que mantinga la durabilitat en passar de pis, en morir, i quan el recull un company online.
   - Que no tinga durabilitat infinita en tornar a carregar la partida.
3. **Minigun:** temps de foc fins al bloqueig (esperat: 2,7 s), durada del bloqueig (2 s) i temps de refredament complet.
4. **TTK:** repetir la taula de la Marta per a Sniper, Escopeta (de prop i de lluny), Martell, Projector i Vareta d'espores. Si alguna es desvia més d'un ±20 % del càlcul, és un bug.
5. **Projector:**
   - 3 enemics en fila i 1 d'aïllat: que salte com a màxim 3 voltes i no repetisca enemic.
   - Online, que la cadena es veja igual en tots els jugadors.
6. **Càmera:** Ballesta i Ballesta de cristall sense zoom, Sniper i Laser amb zoom, també al mòbil.
7. **Online:** cada canvi, provat també com a client amb latència (bore.pub entre dues cases).

### El que més em preocupa
1. Els softlocks (bugs 2 i 3) maten una run bona. Són els primers que cal arreglar.
2. La durabilitat i la calor obrin estats nous que l'online ha de sincronitzar. **[cal provar]**
3. L'exploit de soltar i recollir quan el cooldown siga per arma.

### Nota del joc ara mateix: 6/10 · i amb els canvis: 7/10 (8 si es tanquen els 🔴)
