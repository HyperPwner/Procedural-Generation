using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimationGenerator : MonoBehaviour
{
    [Header("General Settings")]
    [Tooltip("Speed of the attack")]
    public float fAttackSpeed = 2f; //How fast should the attack be?
    [Range(0.1f, 1)]
    [Tooltip("Skill of the character, affects how easilt controlled the sword is when swung and how accurate stabs are")]
    public float fAttackSkill = 0.8f; //affects things like how easily the sword is controlled. At low values the sword swings around and is hard to control, at high values the sword is easily controlled. Also affects the spread of stab locations.
    [Range(0.1f, 3f)]
    [Tooltip("The weight of the sword")]
    public float fAttackWeight = 2f; //The weight behind the sword attack
    [Min(1)]
    [Tooltip("Number of attacks generated")]
    public int iNumOfAttacks = 10; //number of animations generated

    public enum ATTACK_TYPE //What type of attack will it be?
    {
        THRUST,
        SLASH
    }
    [Space]
    [Tooltip("Type of attack generated.")]
    public ATTACK_TYPE eAttackType;

    enum ANIMATION_STATE
    {
        NONE,
        WINDUP,
        ATTACK,
        RETURN
    }
    ANIMATION_STATE eAnimState;

    [Header("Rig")]
    //different parts of the rig
    [Tooltip("Main root of the character, containing the skeleton and rig")]
    [SerializeField] GameObject goCharacter; //main object containing the skeleton and rig
    [Tooltip("Effector of the Chest Rig")]
    [SerializeField] GameObject goChestEffector; //effector of the chest rig
    [Tooltip("Effector of the off hand of the rig")]
    [SerializeField] GameObject goOffHand; //effector of the off hand of the rig
    [Tooltip("Effector of the sword hand of the rig")]
    [SerializeField] GameObject goSwordHand; //effector of the sword hand of the rig
    [Tooltip("Effector of the left foot of the rig")]
    [SerializeField] GameObject goLeftFoot; //effector of the left foot of the rig
    [Tooltip("Effector of the right foot of the rig")]
    [SerializeField] GameObject goRightFoot; //effector of the right foot of the rig

    [Space]
    //off hand fingers
    [Tooltip("Effector of the index finger of the off hand of the rig")]
    [SerializeField] GameObject goOffIndex; //effector of the index finger of the off hand of the rig
    [Tooltip("Effector of the middle finger of the off hand of the rig")]
    [SerializeField] GameObject goOffMiddle; //effector of the middle finger of the off hand of the rig
    [Tooltip("Effector of the ring finger of the off hand of the rig")]
    [SerializeField] GameObject goOffRing; //effector of the ring finger of the off hand of the rig
    [Tooltip("Effector of the pinky finger of the off hand of the rig")]
    [SerializeField] GameObject goOffPinky; //effector of the pinky finger of the off hand of the rig
    [Tooltip("Effector of the thumb finger of the off hand of the rig")]
    [SerializeField] GameObject goOffThumb; //effector of the thumb finger of the off hand of the rig

    [Header("Stab Settings")]
    [Tooltip("Prefab for the stab markers")]
    [SerializeField] GameObject goStabMarkerPrefab; //prefab for the stab markers
    Vector3[] stabLocations; //locations the character will stab to
    float fStabMultiplier = 0.1f; //multiplier value to keep the stab locations in a reasonable range
    float fStabSpeedMultiplier = 2f; //speed multiplier to make sure stabs are fast enough

    [Space]
    [Tooltip("GameObject marking the starting position of the stab")]
    [SerializeField] GameObject goStabStart;
    [Tooltip("Distance the character will stab to. It's best to not make this longer than the length of the arm of the character to avoid unwanted behaviour")]
    [SerializeField] float fStabDistance = 0.5f; //distance the stab locations will be at from the character

    int iIndex;
    Vector3 currentAttack;

    [Space]
    //off hand positions during the stab
    [Tooltip("Position of the off hand during the stab")]
    [SerializeField] GameObject goOffHandStab;
    [Tooltip("Position of the off hand when idle after a stab")]
    [SerializeField] GameObject goOffHandStabIdle;

    [Space]
    //off hand finger positions
    [Tooltip("Index finger position during the stab")]
    [SerializeField] GameObject goOffStabIndex;
    [Tooltip("Middle finger position during the stab")]
    [SerializeField] GameObject goOffStabMiddle;
    [Tooltip("Ring finger position during the stab")]
    [SerializeField] GameObject goOffStabRing;
    [Tooltip("Pinky finger position during the stab")]
    [SerializeField] GameObject goOffStabPinky;
    [Tooltip("Thumb finger position during the stab")]
    [SerializeField] GameObject goOffStabThumb;

    [Header("Slash Settings")]
    [Tooltip("Height the character will slash at.")]
    [SerializeField] float fSlashHeight = 0; //height the slash should be at
    [Tooltip("Distance the character will slash at. It's best to not make this longer than the length of the arm of the character to avoid unwanted behaviour")]
    [SerializeField] float fSlashDistance = 0.5f; //distance from the character that the slash should be at
    [Tooltip("Distance the character will wind up the attack at. It's best to not make this too far behind the character to avoid unwanted behaviour")]
    [SerializeField] float fWindupDistance = 0.115f; //position that the windup goes to
    [Tooltip("Position the off hand will be at during the slash")]
    [SerializeField] GameObject goSlashOffHandPoint; //gameobject on the sword that tells the off hand where to be
    [Tooltip("Prefab for slash points")]
    [SerializeField] GameObject goSlashPointPrefab; //prefab for slash points
    [Range(0, 359)]
    [Tooltip("Direction the slash generated will go in. 0 - 359 with 0 being vertically down.")]
    public int iAttackDirection; //A direction that the attack(s) generated will go in
    [Tooltip("If true, the character will slash in a random direction for each slash generated")]
    public bool bRandomDirection; //use random directions for the attack(s)?

    [Space]
    GameObject[] slashPoints; //array of slash points
    //start and end points of each slash
    Transform[] tSlashStarts;
    Transform[] tSlashEnds;
    //windups of each slash
    Transform[] tSlashWindups;
    [Tooltip("Idle position of the sword after the attack.")]
    [SerializeField] GameObject goSlashIdle;     //idle sword position
    [Tooltip("Array of preset windup points")]
    [SerializeField] List<GameObject> presetWindups; //list of preset windup positions
    [Tooltip("Minimum angle to avoid while slashing (to prevent the sword from hitting the legs)")]
    [SerializeField] float fMinAvoidAngle = 225f; //minimum angle to avoid while slashing (e.g. where the legs are)
    [Tooltip("Maximum angle to avoid while slashing (to prevent the sword from hitting the legs)")]
    [SerializeField] float fMaxAvoidAngle = 315f; //maximum angle to avoid while slashing
    Vector3 v3SlashCentre;
    int iSlashStage; //which stage of the slash is the character in?

    Vector3 arcStart;
    Vector3 arcEnd;
    [Tooltip("Part of the character the slashes will work around (e.g. a section of the torso)")]
    [SerializeField] Transform tSlashCentre; //part of the character that the slashes arc around (e.g. section of the spine)

    [Space]
    //off hand finger positions
    [Tooltip("Index finger position during the slash")]
    [SerializeField] GameObject goOffSlashIndex;
    [Tooltip("Middle finger position during the slash")]
    [SerializeField] GameObject goOffSlashMiddle;
    [Tooltip("Ring finger position during the slash")]
    [SerializeField] GameObject goOffSlashRing;
    [Tooltip("Pinky finger position during the slash")]
    [SerializeField] GameObject goOffSlashPinky;
    [Tooltip("Thumb finger position during the slash")]
    [SerializeField] GameObject goOffSlashThumb;

    //timing values
    float startTime;
    float windupTime;
    float slashTime;
    float stabTime;
    float fracComplete;

    //foot placement
    [Range(-1, 1)]
    [SerializeField] float fCentreOfMassDistribution = 0; //Distribution ratio for the Centre Of Mass, Left Foot:Right Foot
    //target positions
    Vector3 v3LeftFootTarget;
    Vector3 v3RightFootTarget;
    //are the feet meant to be above the floor?
    bool bLeftLegMoving = false, bRightLegMoving = false;

    Vector3 v3TorsoTarget;    //target position for the torso
    float fStartHeight; //starting height of the character
    [Header("Foot/Torso Placement")]
    [Tooltip("Desired height of the foot effectors")]
    [SerializeField] float fFootHeight = 0.1049224f; //desired height of the feet effectors
    [Tooltip("Multiplier value that decides how much the character crouches as the height lowers")]
    [SerializeField] float fCrouchRate = 4.5f; //multiplier value that decides how much the character crouches as the height lowers

    [Space]
    //idle feet positions
    [Tooltip("Left foot idle position")]
    [SerializeField] GameObject goLeftFootStart;
    [Tooltip("Right foot idle position")]
    [SerializeField] GameObject goRightFootStart;
    //stabbing feet positions
    [Tooltip("Left foot stab position")]
    [SerializeField] GameObject goLeftFootStab;
    [Tooltip("Right foot stab position")]
    [SerializeField] GameObject goRightFootStab;
    //slashing starting feet positions
    [Tooltip("Left foot first slash position")]
    [SerializeField] GameObject goLeftFootSlashStart;
    [Tooltip("Right foot first slash position")]
    [SerializeField] GameObject goRightFootSlashStart;
    //slashing end feet positions
    [Tooltip("Left foot second slash position")]
    [SerializeField] GameObject goLeftFootSlashEnd;
    [Tooltip("Right foot second slash position")]
    [SerializeField] GameObject goRightFootSlashEnd;

    [Space]
    float fStabTorsoRotation; //rotation of the torso on the y axis to adjust the stab direction
    [Tooltip("Multiplier value that decides how much the character's torso rotates when stabbing")]
    [SerializeField] float fStabTorsoRotMulti = 40f; //multiplier value that decides how much the character's torso rotates


    

    // Start is called before the first frame update
    void Start()
    {
        eAnimState = ANIMATION_STATE.NONE;
        v3RightFootTarget = goRightFoot.transform.position;
        v3LeftFootTarget = goLeftFoot.transform.position;
        v3TorsoTarget = goChestEffector.transform.position;
        fStartHeight = goChestEffector.transform.position.y;

        Vector3 direction = goLeftFootStab.transform.position - goRightFootStab.transform.position;
        Vector3 left = Vector3.Cross(Vector3.up, direction);
        fStabTorsoRotation = left.x * fStabTorsoRotMulti;
    }

    // Update is called once per frame
    void Update()
    {
        //update left hand position while slashing
        if (eAttackType == ATTACK_TYPE.SLASH && eAnimState != ANIMATION_STATE.NONE)
        {
            goOffHand.transform.position = goSlashOffHandPoint.transform.position;
            goOffHand.transform.rotation = goSlashOffHandPoint.transform.rotation;
        }

        switch (eAnimState)
        {
            case ANIMATION_STATE.NONE:
                break;
            case ANIMATION_STATE.WINDUP:
                //move to start positions
                switch (eAttackType)
                {
                    case ATTACK_TYPE.THRUST:
                        fracComplete = (Time.time - startTime) / windupTime;
                        //rotate hand back
                        goSwordHand.transform.position = Vector3.Lerp(goSwordHand.transform.position, goStabStart.transform.position, fracComplete);

                        if (goSwordHand.transform.position == goStabStart.transform.position)
                        {
                            if (iIndex > stabLocations.Length - 1)
                            {
                                startTime = Time.time;
                                windupTime = 1f / (fAttackSpeed * fStabSpeedMultiplier);
                                eAnimState = ANIMATION_STATE.RETURN;
                            }
                            else
                            {
                                startTime = Time.time;
                                stabTime = 1f / (fAttackSpeed * fAttackWeight * fStabSpeedMultiplier);
                                eAnimState = ANIMATION_STATE.ATTACK;
                            }
                        }
                        break;
                    case ATTACK_TYPE.SLASH:

                        //plan arc
                        Vector3 arcCentre = tSlashCentre.position;

                        Vector3 arcStartRel = arcStart - arcCentre;
                        Vector3 arcEndRel = arcEnd - arcCentre;

                        fracComplete = (Time.time - startTime) / windupTime;

                        //rotate chest to angle towards start point (based on fAttackSkill)
                        Vector3 torsoCentre = new Vector3(v3SlashCentre.x, goChestEffector.transform.position.y, v3SlashCentre.z); //Vector3 for the centre of the torso's rotation (directly facing the direction of v3SlashCentre)
                        Vector3 lookPoint = Vector3.Lerp(tSlashStarts[iIndex].position, torsoCentre, fAttackSkill);
                        Vector3 direction = lookPoint - goChestEffector.transform.position;
                        Quaternion toRotation = Quaternion.FromToRotation(goChestEffector.transform.forward, direction);
                        goChestEffector.transform.rotation = Quaternion.Lerp(goChestEffector.transform.rotation, toRotation, fracComplete);

                        //move/rotate right hand to windup point
                        goSwordHand.transform.position = Vector3.Slerp(arcStartRel, arcEndRel, fracComplete);
                        goSwordHand.transform.position += arcCentre;
                        goSwordHand.transform.rotation = Quaternion.Lerp(goSwordHand.transform.rotation, tSlashWindups[iIndex].rotation, fracComplete);
                        //move feet
                        v3RightFootTarget = goRightFootStart.transform.position;
                        v3LeftFootTarget = goLeftFootStart.transform.position;
                        goRightFoot.transform.position = Vector3.Lerp(goRightFoot.transform.position, v3RightFootTarget, fracComplete);
                        goLeftFoot.transform.position = Vector3.Lerp(goLeftFoot.transform.position, v3LeftFootTarget, fracComplete);

                        if (goSwordHand.transform.position == tSlashWindups[iIndex].position &&
                            goRightFoot.transform.position == goRightFootStart.transform.position &&
                            goLeftFoot.transform.position == goLeftFootStart.transform.position)
                        {
                            iSlashStage = 1;
                            startTime = Time.time;
                            slashTime = 1f / (fAttackSpeed * fAttackWeight);
                            arcStart = tSlashWindups[iIndex].position;
                            arcEnd = tSlashStarts[iIndex].position;
                            eAnimState = ANIMATION_STATE.ATTACK;
                        }
                        break;
                    default:
                        break;
                }
                break;
            case ANIMATION_STATE.ATTACK:
                switch (eAttackType)
                {
                    case ATTACK_TYPE.SLASH:
                        //move arms and chest to starting position of the slash

                        if (iSlashStage == 1)
                        {
                            //plan arc
                            

                            Vector3 arcCentre = (arcStart + arcEnd) * 0.5f;
                            arcCentre -= new Vector3(0, 1, 0); //adjust as necessary

                            Vector3 arcStartRel = arcStart - arcCentre;
                            Vector3 arcEndRel = arcEnd - arcCentre;

                            fracComplete = (Time.time - startTime) / slashTime;

                            //move chest towards centre?
                            Vector3 torsoCentre = new Vector3(v3SlashCentre.x, goChestEffector.transform.position.y, v3SlashCentre.z); //Vector3 for the centre of the torso's rotation (directly facing the direction of v3SlashCentre)
                            Vector3 direction = torsoCentre - goChestEffector.transform.position;
                            Quaternion toRotation = Quaternion.FromToRotation(goChestEffector.transform.forward, direction);
                            goChestEffector.transform.rotation = Quaternion.Lerp(goChestEffector.transform.rotation, toRotation, fracComplete);

                            goSwordHand.transform.position = Vector3.Slerp(arcStartRel, arcEndRel, fracComplete);
                            goSwordHand.transform.position += arcCentre;
                            goSwordHand.transform.rotation = Quaternion.Lerp(goSwordHand.transform.rotation, tSlashStarts[iIndex].rotation, fracComplete);
                            //move feet
                            v3RightFootTarget = goRightFootSlashStart.transform.position;
                            v3LeftFootTarget = goLeftFootSlashStart.transform.position;
                            goRightFoot.transform.position = Vector3.Lerp(goRightFoot.transform.position, v3RightFootTarget, fracComplete);
                            goLeftFoot.transform.position = Vector3.Lerp(goLeftFoot.transform.position, v3LeftFootTarget, fracComplete);

                            if (goSwordHand.transform.position == tSlashStarts[iIndex].position &&
                            goRightFoot.transform.position == goRightFootSlashStart.transform.position &&
                            goLeftFoot.transform.position == goLeftFootSlashStart.transform.position)
                            {
                                iSlashStage = 2;
                                startTime = Time.time;
                                arcStart = tSlashStarts[iIndex].position;
                                arcEnd = tSlashEnds[iIndex].position;
                                slashTime = 1f / (fAttackSpeed * fAttackWeight);
                            }
                        }
                        //once the arms get to starting position
                        else if (iSlashStage == 2)
                        {
                            //plan arc
                            

                            Vector3 arcCentre = tSlashCentre.position;

                            Vector3 arcStartRel = arcStart - arcCentre;
                            Vector3 arcEndRel = arcEnd - arcCentre;

                            fracComplete = (Time.time - startTime) / slashTime;

                            //rotate chest to angle towards end point (based on fAttackSkill)
                            Vector3 torsoCentre = new Vector3(v3SlashCentre.x, goChestEffector.transform.position.y, v3SlashCentre.z); //Vector3 for the centre of the torso's rotation (directly facing the direction of v3SlashCentre)
                            Vector3 lookPoint = Vector3.Lerp(tSlashEnds[iIndex].position, torsoCentre, fAttackSkill);
                            Vector3 direction = lookPoint - goChestEffector.transform.position;
                            Quaternion toRotation = Quaternion.FromToRotation(goChestEffector.transform.forward, direction);
                            goChestEffector.transform.rotation = Quaternion.Lerp(goChestEffector.transform.rotation, toRotation, fracComplete);

                            //lerp hand positions and rotations, move chest to match
                            goSwordHand.transform.position = Vector3.Slerp(arcStartRel, arcEndRel, fracComplete);
                            goSwordHand.transform.position += arcCentre;
                            goSwordHand.transform.rotation = Quaternion.Lerp(goSwordHand.transform.rotation, tSlashEnds[iIndex].rotation, fracComplete);
                            //move feet
                            v3RightFootTarget = goRightFootSlashEnd.transform.position;
                            v3LeftFootTarget = goLeftFootSlashEnd.transform.position;
                            goRightFoot.transform.position = Vector3.Lerp(goRightFoot.transform.position, v3RightFootTarget, fracComplete);
                            goLeftFoot.transform.position = Vector3.Lerp(goLeftFoot.transform.position, v3LeftFootTarget, fracComplete);

                            if (goSwordHand.transform.position == tSlashEnds[iIndex].position &&
                                goRightFoot.transform.position == goRightFootSlashEnd.transform.position &&
                                goLeftFoot.transform.position == goLeftFootSlashEnd.transform.position)
                            {
                                //after all positions, iterate
                                iIndex++;
                                startTime = Time.time;
                                windupTime = 1f / (fAttackSpeed);
                                arcStart = tSlashEnds[iIndex - 1].position;
                                arcEnd = goSlashIdle.transform.position;
                                eAnimState = ANIMATION_STATE.RETURN;
                            }
                        }
                        else
                        {
                            Debug.LogError("Slash Stage Is Neither 1 or 2 During Attack State");
                            return;
                        }
                        break;
                    case ATTACK_TYPE.THRUST:
                        //get current attack
                        currentAttack = stabLocations[iIndex];
                        //adjust for torso rotation
                        Vector3 dir = currentAttack - goStabStart.transform.position;
                        float rotation = (fStabTorsoRotation * -1) / 2;
                        dir = Quaternion.Euler(0, rotation, 0) * dir;
                        currentAttack = dir + goStabStart.transform.position;
                        goSwordHand.transform.LookAt(currentAttack);
                        goSwordHand.transform.rotation *= goStabStart.transform.localRotation;
                        fracComplete = (Time.time - startTime) / stabTime;
                        goSwordHand.transform.position = Vector3.Lerp(goSwordHand.transform.position, currentAttack, fracComplete);

                        //move feet?
                        fCentreOfMassDistribution = Mathf.Lerp(fCentreOfMassDistribution, 0.5f, fracComplete);
                        v3RightFootTarget = goRightFootStab.transform.position;
                        v3LeftFootTarget = goLeftFootStab.transform.position;
                        goRightFoot.transform.position = Vector3.Lerp(goRightFoot.transform.position, v3RightFootTarget, fracComplete);
                        goLeftFoot.transform.position = Vector3.Lerp(goLeftFoot.transform.position, v3LeftFootTarget, fracComplete);
                        goLeftFoot.transform.rotation = Quaternion.Lerp(goLeftFoot.transform.rotation, goLeftFootStab.transform.rotation, fracComplete);


                        if (goSwordHand.transform.position == currentAttack &&
                            goRightFoot.transform.position == goRightFootStab.transform.position &&
                            goLeftFoot.transform.position == goLeftFootStab.transform.position)
                        {
                            iIndex++;
                            startTime = Time.time;
                            windupTime = 1f / (fAttackSpeed * fStabSpeedMultiplier);
                            eAnimState = ANIMATION_STATE.WINDUP;
                        }
                        break;
                    default:
                        break;
                }
                break;
            case ANIMATION_STATE.RETURN:
                //return to default position, delete all markers
                switch (eAttackType)
                {
                    case ATTACK_TYPE.SLASH:
                        //return to idle

                        //plan arc
                        Vector3 arcCentre = tSlashCentre.position;

                        Vector3 arcStartRel = arcStart - arcCentre;
                        Vector3 arcEndRel = arcEnd - arcCentre;

                        fracComplete = (Time.time - startTime) / windupTime;

                        //rotate chest to angle towards centre
                        Vector3 torsoCentre = new Vector3(v3SlashCentre.x, goChestEffector.transform.position.y, v3SlashCentre.z); //Vector3 for the centre of the torso's rotation (directly facing the direction of v3SlashCentre)
                        Vector3 direction = torsoCentre - goChestEffector.transform.position;
                        Quaternion toRotation = Quaternion.FromToRotation(goChestEffector.transform.forward, direction);
                        goChestEffector.transform.rotation = Quaternion.Lerp(goChestEffector.transform.rotation, toRotation, fracComplete);

                        goSwordHand.transform.position = Vector3.Slerp(arcStartRel, arcEndRel, fracComplete);
                        goSwordHand.transform.position += arcCentre;
                        goSwordHand.transform.rotation = Quaternion.Lerp(goSwordHand.transform.rotation, goSlashIdle.transform.rotation, fracComplete);
                        //move feet
                        v3RightFootTarget = goRightFootStart.transform.position;
                        v3LeftFootTarget = goLeftFootStart.transform.position;
                        goRightFoot.transform.position = Vector3.Lerp(goRightFoot.transform.position, v3RightFootTarget, fracComplete);
                        goLeftFoot.transform.position = Vector3.Lerp(goLeftFoot.transform.position, v3LeftFootTarget, fracComplete);

                        if (goSwordHand.transform.position == goSlashIdle.transform.position &&
                            goRightFoot.transform.position == goRightFootStart.transform.position &&
                            goLeftFoot.transform.position == goLeftFootStart.transform.position)
                        {
                            if (iIndex > slashPoints.Length - 1)
                            {
                                eAnimState = ANIMATION_STATE.NONE;
                            }
                            else
                            {
                                startTime = Time.time;
                                windupTime = 1f / (fAttackSpeed);
                                arcStart = tSlashEnds[iIndex - 1].position;
                                arcEnd = tSlashWindups[iIndex].position;
                                v3RightFootTarget = goRightFootStart.transform.position;
                                v3LeftFootTarget = goLeftFootStart.transform.position;

                                eAnimState = ANIMATION_STATE.WINDUP;
                            }
                        }
                        break;
                    case ATTACK_TYPE.THRUST:
                        //return to default
                        fracComplete = (Time.time - startTime) / windupTime;
                        //rotate hand back to normal
                        goSwordHand.transform.position = Vector3.Lerp(goSwordHand.transform.position, goStabStart.transform.position, fracComplete);
                        goSwordHand.transform.rotation = goStabStart.transform.rotation;

                        //move left hand
                        goOffHand.transform.position = Vector3.Lerp(goOffHand.transform.position, goOffHandStabIdle.transform.position, fracComplete);
                        goOffHand.transform.rotation = goOffHandStabIdle.transform.rotation;

                        //move feet back
                        fCentreOfMassDistribution = Mathf.Lerp(fCentreOfMassDistribution, -0.5f, fracComplete);
                        v3RightFootTarget = goRightFootStart.transform.position;
                        v3LeftFootTarget = goLeftFootStart.transform.position;
                        goRightFoot.transform.position = Vector3.Lerp(goRightFoot.transform.position, v3RightFootTarget, fracComplete);
                        goLeftFoot.transform.position = Vector3.Lerp(goLeftFoot.transform.position, v3LeftFootTarget, fracComplete);
                        goLeftFoot.transform.rotation = Quaternion.Lerp(goLeftFoot.transform.rotation, goLeftFootStart.transform.rotation, fracComplete);

                        if (goSwordHand.transform.position == goStabStart.transform.position &&
                            goRightFoot.transform.position == goRightFootStart.transform.position &&
                            goLeftFoot.transform.position == goLeftFootStart.transform.position &&
                            goOffHand.transform.position == goOffHandStabIdle.transform.position)
                        {
                            eAnimState = ANIMATION_STATE.NONE;
                        }
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }

        //foot/chest placement
        //make sure feet stay on floor when not moving
        if (!bRightLegMoving)
        {
            goRightFoot.transform.position = new Vector3(goRightFoot.transform.position.x, fFootHeight, goRightFoot.transform.position.z);
        }
        if (!bLeftLegMoving)
        {
            goLeftFoot.transform.position = new Vector3(goLeftFoot.transform.position.x, fFootHeight, goLeftFoot.transform.position.z);
        }

        //get centre of mass based on foot position
        float ratio = fCentreOfMassDistribution + 1;
        //fix to get local direction instead of global
        Vector3 leftDistributed = goLeftFoot.transform.position * (2 - ratio);
        Vector3 rightDistributed = goRightFoot.transform.position * ratio;
        Vector3 midpoint = (leftDistributed + rightDistributed) / 2;

        v3TorsoTarget = midpoint;
        
        //torso follows to keep centre of mass balanced (within a range)
        goChestEffector.transform.position = v3TorsoTarget; //no need to lerp as the feet lerping will gradually change the target position instead.
        
        //calculate height to put torso at based on distance between feet
        float footDifference = Mathf.Abs(goLeftFoot.transform.localPosition.z - goRightFoot.transform.localPosition.z);
        float height = fStartHeight - footDifference / fCrouchRate;
        //move player to that height
        goChestEffector.transform.position = new Vector3(goChestEffector.transform.position.x, height, goChestEffector.transform.position.z);
        if (eAttackType == ATTACK_TYPE.THRUST)
        {
            //twist torso if thrusting sword
            Vector3 direction = goLeftFoot.transform.localPosition - goRightFoot.transform.localPosition;
            Vector3 left = Vector3.Cross(Vector3.up, direction);
            goChestEffector.transform.rotation = goCharacter.transform.rotation * Quaternion.Euler(0, left.x * fStabTorsoRotMulti, 0);
        }
        else
        {
            goChestEffector.transform.rotation = goCharacter.transform.rotation;
        }
    }



    public void Generate()
    {
        if (Application.isPlaying)
        {
            //delete any markers
            GameObject[] markers = GameObject.FindGameObjectsWithTag("Marker");
            if (markers.Length != 0)
            {
                for (int i = 0; i < markers.Length; i++)
                {
                    Destroy(markers[i]);
                }
                Debug.Log("Destroyed Markers");
            }
            else
            {
                Debug.Log("No Markers To Destroy");
            }

            //put feet into starting positions (arms are handled in the switch statement)
            fCentreOfMassDistribution = -0.5f;
            v3RightFootTarget = goRightFootStart.transform.position;
            v3LeftFootTarget = goLeftFootStart.transform.position;
            goRightFoot.transform.position = v3RightFootTarget;
            goLeftFoot.transform.position = v3LeftFootTarget;
            switch (eAttackType)
            {
                case ATTACK_TYPE.THRUST:
                    //set a number of random positions in front of the character (spread affected by skill)
                    Vector3 stabCentre = goStabStart.transform.position +
                        (fStabDistance * goCharacter.transform.forward);
                    stabLocations = new Vector3[iNumOfAttacks];
                    if (fAttackSkill < 1)
                    {
                        for (int i = 0; i < iNumOfAttacks; i++)
                        {
                            //set to random location
                            Vector2 randomPos = Random.insideUnitCircle * (1 / fAttackSkill) * fStabMultiplier;
                            Vector3 randomPoint = stabCentre + new Vector3(randomPos.x, randomPos.y, 0); //convert to 3D point
                                                                                                         //rotate point around pivot
                            Vector3 dir = randomPoint - stabCentre;
                            dir = goCharacter.transform.rotation * dir;
                            randomPoint = dir + stabCentre;
                            stabLocations[i] = randomPoint;
                            GameObject.Instantiate(goStabMarkerPrefab, stabLocations[i], goCharacter.transform.rotation);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < iNumOfAttacks; i++)
                        {
                            //set to stab position
                            stabLocations[i] = stabCentre;
                            GameObject.Instantiate(goStabMarkerPrefab, stabLocations[i], goCharacter.transform.rotation);
                        }
                    }
                    //move left hand
                    goOffHand.transform.position = goOffHandStab.transform.position;
                    goOffHand.transform.rotation = goOffHandStab.transform.rotation;
                    //move fingers
                    goOffIndex.transform.position = goOffStabIndex.transform.position;
                    goOffIndex.transform.rotation = goOffStabIndex.transform.rotation;
                    goOffMiddle.transform.position = goOffStabMiddle.transform.position;
                    goOffMiddle.transform.rotation = goOffStabMiddle.transform.rotation;
                    goOffRing.transform.position = goOffStabRing.transform.position;
                    goOffRing.transform.rotation = goOffStabRing.transform.rotation;
                    goOffPinky.transform.position = goOffStabPinky.transform.position;
                    goOffPinky.transform.rotation = goOffStabPinky.transform.rotation;
                    goOffThumb.transform.position = goOffStabThumb.transform.position;
                    goOffThumb.transform.rotation = goOffStabThumb.transform.rotation;

                    iIndex = 0;
                    startTime = Time.time;
                    windupTime = 1f / (fAttackSpeed * fStabSpeedMultiplier);
                    eAnimState = ANIMATION_STATE.WINDUP;
                    break;
                case ATTACK_TYPE.SLASH:
                    //plan slashing positions
                    v3SlashCentre = goCharacter.transform.position +
                        new Vector3(0, fSlashHeight * goCharacter.transform.up.y, 0) +
                        (fSlashDistance * goCharacter.transform.forward);
                    slashPoints = new GameObject[iNumOfAttacks];
                    tSlashStarts = new Transform[iNumOfAttacks];
                    tSlashEnds = new Transform[iNumOfAttacks];
                    tSlashWindups = new Transform[iNumOfAttacks];

                    for (int i = 0; i < iNumOfAttacks; i++)
                    {
                        slashPoints[i] = Instantiate(goSlashPointPrefab, v3SlashCentre, Quaternion.identity);
                        slashPoints[i].transform.Rotate(Vector3.up, goCharacter.transform.rotation.eulerAngles.y);
                        //If the user wants random directions, randomise some directions
                        if (bRandomDirection)
                        {
                            int randomDirection = Random.Range(0, 359);
                            iAttackDirection = randomDirection;
                            slashPoints[i].transform.Rotate(Vector3.forward, iAttackDirection + 90f); //add 90 degrees to rotation so that 0 is vertically down
                        }
                        else //else, put all attacks in the same direction
                        {
                            slashPoints[i].transform.Rotate(Vector3.forward, iAttackDirection + 90f); //add 90 degrees to rotation so that 0 is vertically down
                        }
                        if (slashPoints[i].GetComponent<SlashPoint>() != null) //is there a slash point script?
                        {
                            if (slashPoints[i].GetComponent<SlashPoint>().slashLocations.Length == 2) //Are there two slash locations?
                            {
                                tSlashStarts[i] = slashPoints[i].GetComponent<SlashPoint>().slashLocations[0].transform;
                                tSlashEnds[i] = slashPoints[i].GetComponent<SlashPoint>().slashLocations[1].transform;
                                slashPoints[i].GetComponent<SlashPoint>().slashWindup.transform.position = new Vector3(slashPoints[i].GetComponent<SlashPoint>().slashWindup.transform.position.x,
                                                                                                                            slashPoints[i].GetComponent<SlashPoint>().slashWindup.transform.position.y,
                                                                                                                            fWindupDistance);
                                if ((iAttackDirection + 90f) > fMinAvoidAngle && (iAttackDirection + 90f) < fMaxAvoidAngle)
                                {
                                    //get windup position closest to windup of the slash point
                                    GameObject nearestWindup = null;
                                    float minDistance = Mathf.Infinity;
                                    foreach (GameObject windup in presetWindups)
                                    {
                                        float distance = Vector3.Distance(windup.transform.position, slashPoints[i].GetComponent<SlashPoint>().slashWindup.transform.position);
                                        if (distance < minDistance)
                                        {
                                            nearestWindup = windup;
                                            minDistance = distance;
                                        }
                                    }
                                    tSlashWindups[i] = nearestWindup.transform;
                                    Debug.Log(nearestWindup.name);
                                }
                                else
                                {
                                    tSlashWindups[i] = slashPoints[i].GetComponent<SlashPoint>().slashWindup.transform;
                                }
                            }
                            else
                            {
                                Debug.LogError("There must be two slash locations on the slash point");
                            }
                        }
                        else
                        {
                            Debug.LogError("Slash Point has no script!");
                        }
                    }
                    //put character in slashing stance
                    goSwordHand.transform.position = goSlashIdle.transform.position;
                    goSwordHand.transform.rotation = goSlashIdle.transform.rotation;
                    //move fingers
                    goOffIndex.transform.position = goOffSlashIndex.transform.position;
                    goOffIndex.transform.rotation = goOffSlashIndex.transform.rotation;
                    goOffMiddle.transform.position = goOffSlashMiddle.transform.position;
                    goOffMiddle.transform.rotation = goOffSlashMiddle.transform.rotation;
                    goOffRing.transform.position = goOffSlashRing.transform.position;
                    goOffRing.transform.rotation = goOffSlashRing.transform.rotation;
                    goOffPinky.transform.position = goOffSlashPinky.transform.position;
                    goOffPinky.transform.rotation = goOffSlashPinky.transform.rotation;
                    goOffThumb.transform.position = goOffSlashThumb.transform.position;
                    goOffThumb.transform.rotation = goOffSlashThumb.transform.rotation;

                    iIndex = 0;
                    iSlashStage = 0;
                    startTime = Time.time;
                    windupTime = 1f / (fAttackSpeed);
                    arcStart = goSlashIdle.transform.position;
                    arcEnd = tSlashWindups[iIndex].position;
                    eAnimState = ANIMATION_STATE.WINDUP;
                    break;
                default:
                    break;
            }
        }
        else
        {
            Debug.LogError("Animations can only be generated in Play Mode");
        }
    }
}