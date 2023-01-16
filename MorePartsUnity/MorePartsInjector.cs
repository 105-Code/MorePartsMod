using System.Collections.Generic;
using UnityEngine;

public class MorePartsInjector : MonoBehaviour
{

    public string MinMorePartsVersion = "3.0.0";

    public List<Modules> modules;
}

public enum Modules{
    BallonModule,
    RotorModule,
    TelecomunicationModule,
    ContinuosTrackModule,
    ScannerModule,
    ExcavatorModule,
    HingeModule
}
