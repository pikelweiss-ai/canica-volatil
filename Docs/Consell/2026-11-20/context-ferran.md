# Conclusions i idees de Ferran (literal, 20/11/2026)

## Canvis de balanç (les armes no incloses no es toquen)
- Llançaflames: barra de durabilitat (estil Minecraft); quan s'acabe es trenca per sempre i desapareix de l'inventari. Escurçar un poc el rang. Fer sprite del foc.
- Minigun: sobreescalfament amb barra progressiva (s'enfria més lent que s'escalfa); si arriba al topall, queda bloquejada uns segons. Obrir un poc més el con.
- Ballesta: llevar l'amplificador de FOV (la càmera s'allunya amb abast >= 9) o reduir-lo.
- Ballesta de cristall: reduir l'amplificador de FOV, sprite de fletxes morades; si pega, alenteix l'enemic 0,5 s; sons únics.
- Bumerang i Disc de cometa: són la mateixa arma; rework, canviar el disc amb mecànica nova.
- Escopeta: molt a prop 2 de dany (per perdigó), perdigons un poc més lents, rojos i més grossos; so més d'escopeta.
- Espasa de vidre: rang en V i un poc més gran, o rework perquè és inservible.
- Projector de polsos: en lloc de mig llançagranades, cadena elèctrica: si pega a algú i hi ha un enemic prop, raig blau i l'electrocuta (1 volta), i d'aquest pot saltar a un altre, sense repetir enemic en el mateix tir. Bala base 2, electrocució 1. Augmentar lleugerament el rang i llevar el dany en àrea. Sprite i sons d'electricitat.
- Llança de tempesta / Sable de corall: canviar la funcionalitat d'un perquè són quasi iguals.
- Martell orbital: l'aim no representa l'àrea real (hauria de ser circular); baixar el dany a 5.
- Pedra: baixar més la cadència (que siga més roïna encara).
- SMG: obrir un poc més la V i baixar un poc el rang (petit nerf).
- Sniper: augmentar el dany, disminuir el FOV, so de sniper.
- Vareta d'espores: tancar un poc la V, projectils més ràpids i verds, verinosos: cada bola fa 3 de dany repartit en 3 s.

## Idees
- Enemic amb forma de virus que s'acobla a tu i et dona un nerf temporal.
- Etapes després de cert nombre de nivells amb temàtiques (desert, fantasmal, bosc maligne).
- Mapa de bosc maligne: arbres malvats que s'activen en matar tots els enemics i es converteixen en enemics extra.
- Augmentar la velocitat dels projectils d'alguns enemics.
- Amulets: escut, més dany o més cadència.
- Pseudobosses (minibosses dins de sales).

## Bugs que ha trobat
- MOLT VELL: el cooldown de les armes és compartit (un sol nextAttackTime a PlayerAttack). Hauria de ser per arma; només baixa mentre la tens a la mà (si l'amagues no compta, per evitar "doble pump"), però si agafes una arma que no estava carregada no ha de començar amb cooldown extra en agafar-la.
- Els fills dels bosses poden quedar fora de la sala i no pots avançar.
- Si mors o soltes loot, pot quedar damunt del portal post-boss i no es pot agafar (afegir també portal per a tornar).
- El barret d'astronauta no és de la mateixa capa que els jugadors: pot quedar per damunt d'altres jugadors.
- El missatge de xat damunt del jugador queda darrere dels arbres (no sap si deixar-ho).
- Els tirs dels enemics haurien de passar per damunt de l'aigua.
- Online, latència: una moneda la pot agafar un i continua per a l'altre; un amic (Herrera) veia un cofre fantasma que es podia obrir i ja estava obert.

## Context del joc
- Canica Volàtil, Alpha v1.1.1: roguelike 2D pixel art de caniques, masmorres procedurals, 8 temes, bosses (GolemGegant 90 HP, MagFosc 65, ReiLlim 70), sala d'elit (pis 3+), cambra de ressonància (pis 4+), cooperatiu fins a 4 online sense IP, Android amb joysticks, mode Creatiu al mapa lliure, 25 skins/barrets/esteles, monedes. Jugador amb 5 cors.
- Projecte: C:\Users\Usuario\Canica Alpha\Builds\BugfixWorkspace (llegir DISSENY.md, README.md; scripts a Assets/Scripts; WeaponData.cs; enemics a Assets/Prefabs/Enemics).
- Ferran és estudiant de 2n de SMX i fa el joc pràcticament sol (el seu amic Mario el prova); no hi ha pressupost ni monetització.
