using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TracebackAbility : MonoBehaviour
{
    private Rigidbody2D rb;
    private FireAbility fireAbility;
    private PlayerWeapon currentWeapon;
    public Transform firePoint;
    
    // store the necessary information to trace back
    private bool recordFireAction = false;
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
        GetComponent<FireAbility>().OnFire += OnFire;
        GetComponent<PlayerControllerTest>().OnResetCalled += ResetStates;
    }
    private void FixedUpdate()
    {
        if (isRecording)
        {
            recordedStates.Add(new ObjectState(rb.linearVelocity, rb.position, firePoint.rotation, recordFireAction, currentWeapon));    
        }
        recordFireAction = false;
    }
    #region listen call back
    private void OnFire()
    {
        recordFireAction = true;
    }
    public void OnWeaponChanged(PlayerWeapon newWeapon)
    {
        currentWeapon= newWeapon;
    }
    #endregion
    public void ActivateTrackback(float duration, PlayerGhost GhostType,PlayerWeapon initialWeapon)
    {
        if (isRecording) return;
        StartCoroutine(TrackbackCoroutine(duration, GhostType, initialWeapon));
    }
    private IEnumerator TrackbackCoroutine(float duration, PlayerGhost GhostType, PlayerWeapon initialWeapon)
    {
        isRecording = true;
        recordedStates.Clear();
        currentWeapon = initialWeapon;
        yield return new WaitForSeconds(duration);
        ghostInstance = Instantiate(GhostType);
        ghostInstance.InitializeGhost(recordedStates[0].currentPosition, recordedStates);
        isRecording = false;
        ResetStates();
    }
    public void ResetStates()
    {
        if (isRecording)
        {
            StopAllCoroutines();
            if (ghostInstance != null)
                Destroy(ghostInstance);
            ghostInstance= null;
            currentWeapon = null;
            isRecording = false;
        }
        
    }
}
