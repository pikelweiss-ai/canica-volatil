using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Novetats de cada versió. S'obrin amb el botó NOVETATS del menú principal.
// Per a una versió nova: afig un bloc a dalt de tot de Entries (el més nou primer) i canvia GameVersion.
public static class Changelog
{
    public class Entry { public string version; public string title; public string[] lines; }

    public static readonly Entry[] Entries =
    {
        new Entry
        {
            version = "Alpha v1.4.1", title = "Més enemics, botiga i Almanac",
            lines = new[]
            {
                "ENEMICS",
                "Sis enemics nous: Salamandra, Llimac àcid, Escorpí, Llop de neu (en manada), Bombardier i Torreta antiga.",
                "Quatre microbosses amb barra de vida: Escarabat Daurat, Ull Flotant, Granota Gegant i Mag de Cendra (des del pis 3).",
                "BOTIGA",
                "A la sala de tresor, un mercader amb taulell i una màquina expenedora. Gasta les monedes de la partida (E per a obrir).",
                "PERSONALITZAR I ALMANAC",
                "Pantalla Personalitzar nova: pista per a veure l'estela en moviment i quadrícula de canigues, barrets i esteles.",
                "Almanac nou al menú: totes les armes i els enemics que vas trobant.",
                "ARMES",
                "SMG: -10 % de cadència, -10 % d'abast i V un 10 % més oberta. Projector de polsos: +10 % de cadència.",
                "Martell orbital: recarrega un 15 % més lent que abans. Ganxo d'abordatge: ara és èpic i amplia el camp de vista un 20 %.",
                "Arc de llum: mateixa càmera que el Làser. Bàcul de torreta: carrega també guardat, amb barra a la hotbar i l'aim acaba on cau.",
                "Llançamines: 3 càrregues que es recuperen en 1,5 s. Vareta curativa: només ix en cooperatiu, i recarrega més lenta.",
                "ARREGLOS I OPCIONS",
                "La mira queda sempre davall de la canica, el barret i l'arma. Els noms llargs de la hotbar ja no tapen l'arma.",
                "Opció «Seguiment suau de càmera». Sales amb cofre ixen marrons al mapa. Tirs fantasma del Talp arreglats.",
                "Gel: ara és més rar, des del pis 3 i amb rajoles de gel. Mode Creatiu: botó per a treure cofres i la roda ja no canvia l'arma.",
            }
        },
        new Entry
        {
            version = "Alpha v1.4.0", title = "Menú nou i masmorres més variades",
            lines = new[]
            {
                "MENÚ",
                "Fons del portal, logo i botons nous. L'historial s'obri amb NOVETATS.",
                "MASMORRES",
                "Sales amb formes noves, rutes alternatives, emboscades i sales secretes.",
                "Arenes amb tres onades, cobertura i un cofre rar o millor.",
                "Foscor, gel i flames segons la sala. Les portes s'animen en acostar-t'hi.",
                "COMBAT",
                "Sis enemics nous: Mímic, Sanador, Escut-guerrer, Talp, Franctirador i Banshee.",
                "Sons nous per als enemics. El Llançaflames es queda a la barra quan es buida.",
            }
        },
        new Entry
        {
            version = "Alpha v1.3.2", title = "Sprites propis i menú amb novetats",
            lines = new[]
            {
                "MENÚ",
                "El menú principal mostra les novetats al costat dels botons.",
                "ARMES",
                "La torreta té cúpula i un canó que gira cap a l'enemic.",
                "Les mines tenen sprite propi i una llum que parpelleja quan s'armen.",
                "El Ganxo d'abordatge llança un harpó que arriba fins a l'objectiu.",
                "La Vareta curativa dispara una bola verda amb una creu, diferent de la Vareta normal.",
                "ARREGLOS",
                "Barret de Cuiner: ja no falta un píxel a la vora.",
            }
        },
        new Entry
        {
            version = "Alpha v1.3.1", title = "Sis armes noves, bosses nous i cofres que milloren",
            lines = new[]
            {
                "ARMES NOVES",
                "Bàcul de torreta (rar): deixa una torreta que dispara sola durant 9 s.",
                "Llançamines (rar): deixa fins a 3 mines que exploten quan s'hi acosta un enemic.",
                "Arc de llum (èpic): mantín el clic per carregar; més càrrega, més mal, i travessa enemics.",
                "Escut de mirall (èpic): mantín el clic per parar i reflectir les bales; també copeja.",
                "Vareta curativa (rar): en cooperatiu, cura el company ferit que apuntes.",
                "Ganxo d'abordatge (comú): estira un enemic cap a tu, o et llança cap a una paret.",
                "MASMORRA",
                "Enemics nous: Gàrgola (tira en ventall), Cranc (resistent) i Fantasma de gel (es teletransporta).",
                "Bosses nous: Reina Aranya (Bosc i Cripta) i Rei de Gel (Neu).",
                "Els cofres milloren amb els pisos: al pis 1 no hi ha armes èpiques ni llegendàries.",
                "En cooperatiu, la sala inicial té un cofre comú per a cada jugador.",
                "ESTÈTICA",
                "Barrets nous: Mag, Cuiner, Orelles de gat, Cowboy i Antenes. El barret de Pirata és nou.",
                "Esteles noves: Cors, Flamarada, Floc de neu, Diamants, Fulles i Estrelles de colors.",
                "OPCIONS",
                "Nova pestanya OPCIONS als Ajustos: cura intel·ligent (sí/no), números de dany, aim amb rebots i tremolor de càmera.",
                "ONLINE",
                "Protocol 16: cal la v1.3.1 per a jugar amb altres jugadors.",
            }
        },
        new Entry
        {
            version = "Alpha v1.3.0", title = "Cooldown de sempre i més personalitat a les armes",
            lines = new[]
            {
                "COOLDOWN",
                "Torna el cooldown de sempre: les armes guardades NO es recuperen, només les de cos a cos (i l'escalfament de la Minigun).",
                "Canviar d'arma ja no fa esperar.",
                "ARMES",
                "Ballesta de cristall: alenteix un 20 % durant 1,5 s.",
                "Disc de cometa: no allunya la càmera, l'aim ensenya tots els rebots (com el Rico) i cada rebot allarga el recorregut un 15 %.",
                "Espasa de vidre: ara és de qualitat verda; si colpeges, +15 % de velocitat durant 1 s (es renova, no s'acumula).",
                "Làser: torna a travessar tots els enemics, amb menys zoom i menys cadència.",
                "Llançaflames: un 10 % menys de cadència.",
                "Llança de tempesta: el rang i el colp són una línia recta, no una V.",
                "Minigun: menys abast, un poc menys de cadència i no pots tornar a disparar fins que l'escalfament torna al principi.",
                "Projector de polsos: un poc més d'abast, sense tocar la càmera.",
                "Sniper: un poc menys de zoom.",
                "Vareta màgica: un 15 % menys de cadència.",
                "Martell orbital: 5 de dany.",
                "ONLINE",
                "Protocol 15: cal la v1.3.0 per a jugar amb altres jugadors.",
            }
        },
        new Entry
        {
            version = "Alpha v1.2.1", title = "Balanç, arreglos i cares noves",
            lines = new[]
            {
                "ARMES",
                "Sniper: 8 de dany, recarrega més lent i la càmera s'allunya menys.",
                "Làser: 3 de dany i només travessa 2 enemics.",
                "Vareta d'espores: ja no és teledirigida; tira 3 boles més ràpides.",
                "Espasa de vidre: 4 de dany, colp en V més estret i l'aim ensenya la V.",
                "Llança de tempesta: menys abast, dash més curt i només pega a 3 enemics.",
                "Ballesta de cristall: dispara més ràpid. Vareta: ja no allunya la càmera.",
                "Disc de cometa: arriba més lluny i fa +1 de dany a cada rebot.",
                "Projector de polsos: 3 de dany i cada salt de la cadena fa 2.",
                "Martell orbital: pega per damunt de les parets.",
                "Cooldown: les armes guardades també es recuperen, però canviar d'arma fa esperar un poc.",
                "ARREGLOS",
                "Les armes cos a cos ja trenquen caixes i barrils.",
                "Les caixes ja no es posen una damunt de l'altra.",
                "Els enemics et veuen i et disparen per damunt de l'aigua.",
                "Els ítems d'un cofre cauen al voltant de qui l'obri, també en cooperatiu.",
                "Una sala s'activa també si algú està als raconets.",
                "El cofre del boss no apareix damunt d'un jugador.",
                "Dues cures iguals s'ajunten en arrossegar-ne una damunt de l'altra.",
                "Clic dret: cura intel·ligent, tria sola la cura que millor encaixa amb la vida que et falta.",
                "Les armes amb trets limitats (Llançaflames) només es trenquen a la masmorra.",
                "MÉS",
                "Lava i àcid amb bombolles i crosta, connectats.",
                "El xat mostra una \"_\" parpellejant quan escrius.",
                "Aquest panell de novetats: botó NOVETATS del menú principal.",
                "ONLINE",
                "Protocol 14: cal la v1.2.1 per a jugar amb altres jugadors.",
                "La llista de sales s'obri a INTERNET i el formulari avisa si una sala pública només serà per a la teua wifi.",
            }
        },
        new Entry
        {
            version = "Alpha v1.2.0", title = "Resum, rècords i skins",
            lines = new[]
            {
                "Resum de partida en morir: temps, enemics, bosses, monedes, sales, cofres i arma favorita.",
                "Rècords locals, separats en solitari i cooperatiu.",
                "28 skins, amb el Llop, el Cíclop i la Postal.",
                "Botons TORNAR A JUGAR i MENÚ PRINCIPAL; l'amfitrió decideix en cooperatiu.",
            }
        },
    };

    const string SeenKey = "Changelog_Seen";

    // Hi ha novetats que l'usuari encara no ha obert d'aquesta versió?
    public static bool HasNew
    {
        get { try { return PlayerPrefs.GetString(SeenKey, "") != GameVersion.Package; } catch { return false; } }
    }

    static void MarkSeen()
    {
        if (RunStats.DontSave) return;
        try { PlayerPrefs.SetString(SeenKey, GameVersion.Package); PlayerPrefs.Save(); } catch { }
    }

    static GameObject open;

    public static void Open()
    {
        if (open != null) return;
        MarkSeen();
        open = UIKit.Canvas("Novetats", 120);
        var ui = open.AddComponent<ChangelogUI>();
        var shade = UIKit.Box(open.transform, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, .82f), "Fons");
        shade.anchorMin = Vector2.zero; shade.anchorMax = Vector2.one; shade.offsetMin = shade.offsetMax = Vector2.zero;
        var panel = UIKit.Box(open.transform, Vector2.zero, new Vector2(1280, 800), UIKit.Panel, "Panell");
        UIKit.Text(panel, "NOVETATS", new Vector2(0, 355), new Vector2(900, 60), 40, TextAlignmentOptions.Center, UIKit.Gold);

        var content = UIKit.ScrollList(panel, new Vector2(0, -10), new Vector2(1200, 620), out var scroll, 2);
        foreach (var e in Entries)
        {
            var head = UIKit.ListRow(content, 58, UIKit.PanelLight);
            UIKit.Text(head, e.version + "  ·  " + e.title, Vector2.zero, new Vector2(1120, 54), 28, TextAlignmentOptions.MidlineLeft, UIKit.Gold);
            foreach (var line in e.lines)
            {
                bool section = line == line.ToUpperInvariant() && line.Length < 20; // "ARMES", "ARREGLOS"...
                var row = UIKit.ListRow(content, section ? 42 : 36, section ? UIKit.Row : UIKit.RowAlt);
                UIKit.Text(row, section ? line : "·  " + line, Vector2.zero, new Vector2(1120, section ? 40 : 34), section ? 22 : 21,
                    TextAlignmentOptions.MidlineLeft, section ? UIKit.Green : UIKit.TextColor);
            }
            UIKit.ListRow(content, 14, new Color(0, 0, 0, 0));
        }

        UIKit.Button(panel, "TANCAR", new Vector2(0, -368), new Vector2(300, 56), Close, UIKit.Style.Primary, 24);
    }

    public static void Close()
    {
        if (open != null) Object.Destroy(open);
        open = null;
    }
}

public class ChangelogUI : MonoBehaviour
{
    void Update() { if (Input.GetKeyDown(KeyCode.Escape)) Changelog.Close(); }
}
