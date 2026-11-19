using UnityEngine;

// Referencies explicites: disponibles encara que cap cofre les haja carregades.
// enemies i chests els usa el mode Creatiu del mapa lliure (i els clients per a crear les copies).
public class NetContentCatalog : ScriptableObject
{
    public WeaponData[] weapons;
    public GameObject[] enemies;
    public GameObject[] chests;
}
