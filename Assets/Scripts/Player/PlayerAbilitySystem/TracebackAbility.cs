using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TracebackAbility : MonoBehaviour
{
    private Rigidbody2D rb;
    //private FireAbility fireAbility;
    //private PlayerWeapon currentWeapon;
    public Transform firePoint;
    
    // store the necessary information to trace back
    //private bool recordFireAction = false;
    private bool isRecording = false;
    private List<ObjectState> recordedStates = new List<ObjectState>();
    private PlayerGhost ghostInstance;
    //declare event for 
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        //GameManager.instance.onReset += ResetStates;
    }
    private void FixedUpdate()
    {
        if (isRecording)
        {
            recordedStates.Add(new ObjectState(rb.linearVelocity, rb.position, firePoint.rotation));    
        }
        //recordFireAction = false;
    }
    #region listen call back
    #endregion
    public void ActivateTrackback(float duration, PlayerGhost GhostType)
    {
        if (isRecording) return;
        StartCoroutine(TrackbackCoroutine(duration, GhostType));
    }
    private IEnumerator TrackbackCoroutine(float duration, PlayerGhost GhostType)
    {
        isRecording = true;
        recordedStates.Clear();
        yield return new WaitForSeconds(duration);
        ghostInstance = Instantiate(GhostType);
        ghostInstance.InitializeGhost(recordedStates[0].currentPosition, recordedStates);
        isRecording = false;
        //ResetStates();
    }
    //public void ResetStates()
    //{
    //    if (isRecording)
    //    {
    //        StopAllCoroutines();
    //        if (ghostInstance != null)
    //            Destroy(ghostInstance);
    //        ghostInstance= null;
    //        //currentWeapon = null;
    //        isRecording = false;
    //    }
        
    //}
}
