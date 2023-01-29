using Pet.Utils;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace Pet.Modules;

internal class Shade
{
    internal static GameObject Create(Vector3 position)
    {
        var go = GoUtils.GetGameObjectFromResources("Hollow Shade");
        var shade = GameObject.Instantiate(go);
        GameObject.DontDestroyOnLoad(shade);
        //shade.name = go.name;
        shade.transform.position = position;
        shade.transform.SetScaleMatching(0.5f);

        //typeof(Shade).LogModDebug("Removing shade's fsm");
        //RemoveShadeFsm(shade);

        //typeof(Shade).LogModDebug("Getting grimmchild's fsm");
        //var fsm = GetGrimmchildFsm();
        //var fsm = GetWeaverlingFsm();

        //typeof(Shade).LogModDebug("Adding grimmchild's fsm into shade");
        //var newFsm = shade.AddComponent(fsm);
        //foreach (var s in newFsm.FsmStates)
        //{
        //    s.Fsm = newFsm.Fsm;
        //}
        //typeof(Shade).LogModDebug("Done");

        return shade;
    }

    private static void RemoveShadeFsm(GameObject shade)
    {
        //shade.RemoveComponent<PlayMakerFSM>(fsm => fsm.FsmName == "Shade Control");
        foreach (var c in shade.GetComponents<PlayMakerFSM>())
        {
            UObject.DestroyImmediate(c);
        }
    }

    private static PlayMakerFSM GetGrimmchildFsm()
    {
        var go = GoUtils.GetGameObjectFromResources("Grimmchild");
        var fsm = go.GetComponent<PlayMakerFSM>(fsm => fsm.FsmName == "Control");
        return fsm;
    }

    private static PlayMakerFSM GetWeaverlingFsm()
    {
        var go = GoUtils.GetGameObjectFromResources("Weaverling");
        var fsm = go.GetComponent<PlayMakerFSM>(fsm => fsm.FsmName == "Control");
        return fsm;
    }

    // "Hollow Shade"
    // "Grimmchild-Control"

    //GameObject Create(Vector2 position)
    //{
    //    foreach (var pfsm in aspid.GetComponentsInChildren<PlayMakerFSM>())
    //    {
    //        pfsm.SetState(pfsm.Fsm.StartState);
    //    }
    //    GameObject NewAspid = GameObject.Instantiate(aspid);
    //    NewAspid.SetActive(true);
    //    NewAspid.SetActiveChildren(true);
    //    List<GameObject> AspidChildren = GetChildren(NewAspid);
    //    foreach (var child in AspidChildren)
    //    {
    //        if (child.name == "Alert Range New")
    //            child.GetComponent<CircleCollider2D>().radius = 15;
    //        if (child.name == "Unalert Range")
    //            child.GetComponent<CircleCollider2D>().radius = 25;
    //    }
    //    HealthManager AspidHP = NewAspid.GetComponent<HealthManager>();
    //    if (!stngs.enemysoul)
    //        SetEnemyType(AspidHP, 3); // setting it to 3 or 6 disables soul gain
    //    AspidHP.hp = int.MaxValue;

    //    var aspidZ = NewAspid.transform.position.z;
    //    NewAspid.transform.position = new Vector3(position.x, position.y, aspidZ);

    //    var xscale = NewAspid.transform.GetScaleX();
    //    var yscale = NewAspid.transform.GetScaleY();
    //    NewAspid.transform.SetScaleX(xscale * stngs.scaler);
    //    NewAspid.transform.SetScaleY(yscale * stngs.scaler);
    //    return NewAspid;
    //}
}
