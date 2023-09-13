using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IZoneInsertable
{
    void Insert(Grabbable grabbable);
    void Remove(Grabbable grabbable);
}