# Canica Volàtil · Preparació de l'Alpha de PC

Direcció acordada a partir de l'entrevista amb Ferran del 18/11/2026. Document de proposta i treball: les funcions següents encara no estan implementades. Plataformes d'aquesta etapa: Windows i Linux. Android continua en la seua branca.

## 1. La promesa del joc

«Una expedició amb els amics on sempre pots descobrir una sala, una combinació o una peça per a la teua col·lecció.»

Tres prioritats: sorpreses en cada expedició, combat amb identitat de canica i col·lecció que perdure entre partides. El cooperatiu per LAN i per codi és un requisit per arribar a l'Alpha.

El motiu per tornar serà dominar una arma, descobrir un secret, superar el rècord amb els amics i completar la col·lecció. Les recompenses es guanyen jugant, amb objectius visibles i un catàleg consultable.

## 2. Bucle principal

Refugi compartit → formar grup → triar expedició → explorar, combinar equip i assumir riscos → derrotar un boss o caure → cobrar recompenses → ampliar la col·lecció i tornar a preparar-se.

La masmorra conservarà l'objectiu d'arribar cada vegada més lluny. Al refugi hi haurà resum de l'expedició, rècord de pis, descobriments, equip dels companys i accés ràpid a una altra partida.

## 3. Masmorres amb decisions

La forma i la funció de les sales han de variar. Canviar columnes dins d'un rectangle no serà suficient. Proposta de repertori:

| Sala | Què fas | Decisió o sorpresa |
|---|---|---|
| Combat | Superar un encontre amb geometria pròpia | Prioritzar enemics i aprofitar cobertures |
| Mercader | Gastar monedes de l'expedició | Cura, arma o reservar diners |
| Forja | Triar una modificació d'una arma | Especialitzar-la amb un avantatge i un cost |
| Santuari | Triar una relíquia entre alternatives | Canviar la manera de jugar |
| Desafiament | Fer una prova opcional anunciada | Arriscar recursos per una recompensa millor |
| Esdeveniment | Resoldre una situació amb alternatives | Rescatar un personatge, activar una màquina o negociar |
| Secreta | Descobrir una pista i obrir un accés | Trobar objectes i aspectes especials |
| Persecució | Travessar una sala amb una amenaça mòbil | Moure's i coordinar-se, amb eixida reconeixible |
| Tresor | Recompensa amb variants recognoscibles | Triar entre premis o afrontar un guardià |
| Boss | Aprendre patrons i fases | Preparar l'equip i repartir funcions |

Primer lot: combat amb diverses geometries, mercader, santuari i desafiament. Les sales especials tindran pressupost per pis, requisits d'accés i límits de repetició. Es conservaran una ruta practicable, portes accessibles i espai segur en entrar. El pis 1 manté la dificultat aprovada; la varietat es provarà abans de canviar-ne l'equilibri.

La sorpresa tindrà pistes: un cofre sospitós, una marca en una paret o un so especial permetran aprendre. El generador combinarà peces preparades a mà, regles d'encontres i variants, amb una llavor guardada per reproduir errors i compartir reptes.

## 4. Una identitat de canica

Les mecàniques distintives seran trajectòries, impulsos i interaccions amb l'escenari. El moviment base continuarà sent precís; els efectes de gel, rebot o magnetisme seran zones identificables.

| Bloc o superfície | Funció | Ús en mapes |
|---|---|---|
| Rebotador | Impuls en una direcció visible | Rutes ràpides, flanqueig i sales de punteria |
| Mirall | Reflexió de projectils compatibles | Atacs indirectes i trencaclosques |
| Cobertura destructible | Protecció temporal | Obrir una ruta durant el combat |
| Gel | Menys adherència dins de la zona | Corbes i risc controlable |
| Placa i porta | Activar una connexió temporal | Rutes alternatives i cooperació |
| Imant | Atraure o desviar objectes compatibles | Armes i obstacles amb polaritat |
| Cinta de corrent | Desplaçament ambiental anunciat | Canviar línies de foc i trajectes |

Primers blocs: rebotador, mirall i cobertura destructible. Cada interacció haurà de tindre senyal visual, regla de col·lisió clara i la mateixa resolució en tots els clients.

## 5. Armes, rareses i relíquies

Conservar les quatre rareses existents —comuna, rara, èpica i llegendària— i donar més identitat a cada arma. Separar la probabilitat d'aparició del nivell de dany. Les armes excepcionals necessitaran una contrapartida i una descripció comprensible.

Primer paquet proposat de sis mecàniques, amb valors pendents de prova:

- **Carambola:** boles que reboten; els miralls permeten preparar tirs indirectes.
- **Espasa de retorn:** avanç curt en atacar i segon tall en recuperar posició.
- **Imant bipolar:** primer tret marca; el següent atrau o espenta la marca.
- **Escopeta de ferralla:** fragments travessen cobertures que acabes de destruir.
- **Llança d'ancoratge:** fixa una zona breument, amb avís abans d'activar-se.
- **Nucli inestable:** explosió retardada i trajectòria recuperable per un jugador hàbil.

Relíquies d'expedició: modificacions temporals que es poden combinar, per exemple un rebot extra, una recompensa per encertar sense fallar o una explosió en tornar un projectil. Limitar acumulacions i provar interaccions amb explosions, invulnerabilitat i atordiments abans d'ampliar el catàleg.

Els botins tindran pesos per font i pis. Els descobriments i els cofres importants evitaran duplicats inútils quan siga possible. Els objectes únics s'obtindran també amb reptes concrets que el jugador puga consultar.

## 6. Col·lecció, inventari i tenda

Dos saldos amb noms i icones diferents:

- **Monedes d'expedició:** mercaders, forja i decisions durant la partida. Formen part de la partida en curs.
- **Fitxes de col·lecció:** recompenses per progressar, descobrir i completar reptes. Es guarden en el perfil i serveixen per a aspectes, barrets, esteles i decoració del refugi.

La tenda del refugi tindrà previsualització, preu, saldo, confirmació de compra i indicació clara del que ja tens. El catàleg permanent facilitarà perseguir un aspecte concret. Els cosmètics actuals es conservaran per als perfils existents; caldrà definir una migració abans de posar-los darrere de desbloquejos.

Inventari amb nom, raresa, mecànica, estadístiques comparables i descripció de les relíquies actives. La col·lecció mostrarà què has descobert, què tens equipat i pistes dels desbloquejos.

Persistència prevista: identificadors estables d'objecte, versió del perfil, escriptura atòmica i còpia de recuperació. Cada client conservarà el seu perfil. Validar costos i recompenses del grup a l'amfitrió; una partida LAN amb perfils locals encara no ofereix una economia protegida contra trampes.

## 7. Jugar amb amics

Objectiu de l'Alpha: expedicions d'1 a 4 jugadors. La primera prova es farà amb dos. Accés des del menú a crear partida, unir-se per LAN i unir-se amb codi per Internet.

LAN i codi són dues vies diferents: LAN usa connexió directa a l'amfitrió; el codi d'Internet necessita un servei de connexió/relay i configuració del projecte. Cal validar l'SDK compatible, l'autenticació i les condicions del servei abans de tancar la implementació. La proposta tècnica anterior de Netcode for GameObjects + Transport + Relay continua com a candidata.

L'amfitrió resoldrà generació, enemics, impactes, compres i botins. Els clients enviaran intencions de moviment i atac. La llavor ajudarà a reproduir el mapa; els objectes i els resultats es sincronitzaran explícitament.

Primera experiència cooperativa: sala d'espera amb estat preparat, entrada conjunta, botí amb recollida inequívoca i companys abatuts que es poden reviure. Les transicions de pis conservaran les dades de cada jugador. Definir i provar què passa si falta un jugador o marxa l'amfitrió; primera versió amb avís i retorn segur al menú si cau l'amfitrió.

El menú social començarà pel grup actual, copiar codi i connexions recents. Llista d'amics amb presència i invitacions persistents es podrà ampliar quan hi haja identitat de jugador i servei social real.

## 8. Vida i modes competitius

Conservar els cors de masmorra, amb valors interns compatibles amb dany fraccionat i modificadors si la implementació ho necessita. Per al PvP es proposen **100 HP** visibles i un perfil d'equilibri propi. No convertir automàticament els valors actuals: provar temps per derrotar un rival, curació, defensa i combos.

Primer mode extra proposat: **Ruptura de nucli**, 2v2 per a les primeres proves. Cada equip protegeix un nucli; activar nodes del mapa obri finestres per atacar-lo. Cobertures destructibles i rebotadors creen rutes de flanqueig. Equip inicial equilibrat i millores de partida accessibles als dos equips.

Després: **Atrapa la bandera**, amb rutes de retorn i punts de tall; **Carambola**, una bola neutral que cal impulsar fins a una porteria. Battle royale queda en la següent ampliació: necessita més jugadors, mapa, ritme d'eliminació i proves de xarxa a una escala superior.

Els mapes s'han de provar amb cada habilitat: distàncies de reaparició, línies de tir, llocs on quedar atrapat i avantatges de cada costat. Els cosmètics mantindran una silueta i una lectura d'equip clares.

## 9. Bosses amb comportaments diferents

Objectiu orientatiu del primer catàleg: sis bosses amb mecàniques pròpies, comptant els actuals després de revisar-los. La quantitat final dependrà de les proves.

- **Ferreter de ferralla:** transforma cobertures en projectils; permet interrompre una màquina.
- **Aranya teixidora:** connecta punts amb fils; cal obrir passadissos i esquivar, amb objectius per a un jugador o diversos.
- **Canica mirall:** duplica trajectòries anunciades i obliga a llegir rebots.
- **Nucli magnètic:** alterna atracció i repulsió amb zones segures identificables.

Fases visibles, patrons anunciats, temps de resposta suficient i recompenses pròpies. Les versions cooperatives escalaran pressió i objectius a més de vida. Cada boss ha de poder superar-se en solitari.

## 10. Ordre de desenvolupament i proves d'eixida

| Etapa | Resultat jugable | Quan la donem per feta |
|---|---|---|
| A · Fonaments de xarxa | Dos clients en una sala LAN | Es veuen moure, ataquen un enemic, recullen un objecte una vegada i gestionen desconnexió |
| B · Primera expedició compartida | Dos jugadors entren i acaben un pis | Mapa i portes coherents, dany coherent, reviure, inventaris separats i transició conjunta |
| C · Varietat | Primer lot de sales, blocs i sis mecàniques d'arma | Rutes accessibles, sales amb funcions distintes, botí reproduïble i menys repeticions en llavors de prova |
| D · Col·lecció | Perfil, tenda i resum de partida | Compra única, saldo correcte, persistència després de reiniciar i migració dels perfils actuals |
| E · Internet i grup de quatre | Sala amb codi i cooperatiu d'1 a 4 | Sessions fora de LAN, errors de connexió comprensibles i proves mixtes Windows/Linux |
| F · Mode extra i bosses | Ruptura de nucli i primer catàleg revisat | Objectius, reaparició, victòria/derrota i regles d'equip sincronitzats |
| G · Candidata Alpha | Paquets de PC per revisar | Proves completes, guardats recuperables, controls verificats i límits documentats |

El primer objectiu concret és l'etapa A. Les primeres proves poden usar instàncies locals; per donar el multijugador per validat faran falta dos ordinadors i després una connexió externa. No presentar un botó de sala com una funció acabada sense haver completat la connexió.

La versió es publicarà com a Alpha quan passe aquestes proves, amb el número final triat en preparar la publicació. Durant el desenvolupament es faran compilacions de prova identificades per etapa i commit.

## 11. Estat tècnic comprovat en començar

- Base PC: `main`, commit `45013a5`; treball d'Alpha separat en `alpha/pc-foundation`.
- `DungeonGenerator.RoomType` només conté Start, Combat, Treasure i Boss. Hi ha geometria/decoració aleatòria, però pocs rols de sala.
- `RunState` és estàtic i representa un sol jugador. Cal convertir-lo en estat de sessió i dades per jugador.
- La generació i diverses mecàniques busquen un únic objecte amb etiqueta Player. Cal revisar objectius, càmera, UI i propietat de cada jugador.
- `WeaponData` ja inclou raresa, explosions, penetració, retorn, homing, arcs i dash. Aprofitar la base i separar definició d'arma, estat de l'arma i resolució de l'atac.
- El manifest inclou Multiplayer Center, però no declara Netcode/Transport/Relay. El lobby actual figura com a pròximament; no és una implementació de xarxa.
- Hi havia quatre assets de fonts modificats al projecte principal. Es preserven; aquesta preparació es fa en un worktree separat.

Aquest document actualitza la direcció de `DISSENY.md`, especialment la tenda, la col·lecció i la prioritat del cooperatiu. Les propostes de contingut i els objectius numèrics encara s'han de contrastar jugant.
