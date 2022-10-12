using System.Collections.Generic;
using UnityEngine;

public class MorePartsInjector : MonoBehaviour
{

    public string MinMorePartsVersion = "2.2.0";

    public List<Modules> modules;
}

public enum Modules{
    BallonModule,
    RotorModule,
    TelecomunicationModule,
    ContinuosTrackModule
}
