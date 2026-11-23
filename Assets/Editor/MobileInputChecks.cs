using System;
using UnityEngine;
using UnityEngine.EventSystems;

// Prova els dos punters alhora, la propietat de cada dit i la cancel·lacio.
public static class MobileInputChecks
{
    public static void Validate()
    {
        float timeScale = Time.timeScale; bool editing = GameControls.Editing;
        var root = new GameObject("Touch checks",typeof(Canvas)); root.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var events = new GameObject("Events de prova",typeof(EventSystem));
        try
        {
            Time.timeScale = 1; GameControls.Editing = false; MobileInput.Clear();
            var left = Create(root.transform,false); var right = Create(root.transform,true); Canvas.ForceUpdateCanvases();
            var a = Point(events.GetComponent<EventSystem>(),left,11,new Vector2(82,0));
            var b = Point(events.GetComponent<EventSystem>(),right,22,new Vector2(0,82));
            left.OnPointerDown(a); right.OnPointerDown(b);
            Require(MobileInput.Movement.x > .99f && !MobileInput.Firing && MobileInput.Aim.y > .99f,"Dos dits simultanis: moure i apuntar sense disparar");
            var attack = new GameObject("Botó d'atac",typeof(RectTransform)).AddComponent<MobileAction>(); attack.action = 4;
            var attackPointer = new PointerEventData(events.GetComponent<EventSystem>()) { pointerId = 33 };
            attack.OnPointerDown(attackPointer); Require(MobileInput.Firing,"El botó d'atac inicia el foc");
            left.OnPointerUp(b); Require(MobileInput.Movement.x > .99f,"Un altre dit no pot soltar el joystick");
            left.OnPointerUp(a); Require(MobileInput.Movement == Vector2.zero && MobileInput.Firing,"Soltar moviment conserva atac");
            right.OnPointerUp(b); Require(MobileInput.Firing,"Soltar l'aim no para el foc");
            attack.OnPointerUp(attackPointer); Require(!MobileInput.Firing,"Soltar atac para el foc");
            a.position = Position(left,new Vector2(41,0)); left.OnPointerDown(a);
            Require(MobileInput.Movement.magnitude > .35f && MobileInput.Movement.magnitude < .5f,"Velocitat analogica");
            a.position = Position(left,Vector2.one * 200); left.OnDrag(a); Require(Mathf.Abs(MobileInput.Movement.magnitude - 1) < .001f,"Diagonal limitada");
            left.Cancel(); right.Cancel(); Require(MobileInput.Movement == Vector2.zero && !MobileInput.Firing,"Cancel·lacio sense accions enganxades");
            Time.timeScale = 0; right.OnPointerDown(b); Require(!MobileInput.Firing,"Pausa bloqueja disparar");
            for (int action = 0; action < GameControls.Actions.Length; action++)
            {
                MobileInput.Press(action);
                MobileInput.Release(action);
            }
            Debug.Log("MOBILE_INPUT_CHECKS: PASS (joysticks, pausa i totes les accions)");
        }
        finally { UnityEngine.Object.DestroyImmediate(root); UnityEngine.Object.DestroyImmediate(events); MobileInput.Clear(); Time.timeScale = timeScale; GameControls.Editing = editing; }
    }
    static MobileStick Create(Transform parent,bool aim)
    {
        var r = new GameObject("Joystick",typeof(RectTransform)).GetComponent<RectTransform>(); r.SetParent(parent,false); r.sizeDelta = Vector2.one * 230;
        var knob = new GameObject("Palanca",typeof(RectTransform)).GetComponent<RectTransform>(); knob.SetParent(r,false);
        var stick = r.gameObject.AddComponent<MobileStick>(); stick.aim = aim; stick.knob = knob; return stick;
    }
    static Vector2 Position(MobileStick stick,Vector2 local) => RectTransformUtility.WorldToScreenPoint(null,stick.transform.TransformPoint(local));
    static PointerEventData Point(EventSystem events,MobileStick stick,int id,Vector2 local) => new PointerEventData(events) { pointerId = id, position = Position(stick,local) };
    static void Require(bool ok,string detail) { if(!ok) throw new Exception("MOBILE_INPUT_CHECKS: "+detail); }
}
