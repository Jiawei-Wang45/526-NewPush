using UnityEngine;
//deprecated, pausable is implemented using delegate now
public interface IPausable
{
    public void Pause(float pauseDuration, float pauseStrength);
}
