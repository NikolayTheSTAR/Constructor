using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Object can be controlled with arrow-buttons
public interface IArrowsControlable
{
    void SetForceUp(bool force);
    void SetForceDown(bool force);
    void SetForceRight(bool force);
    void SetForceLeft(bool force);
    void Stop();
    bool ControlableWithWASD {get;}
}