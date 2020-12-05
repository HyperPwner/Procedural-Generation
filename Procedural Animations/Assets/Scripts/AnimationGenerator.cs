using UnityEngine;

//TO DO:
//HAVE HAND CONTROLLERS SYNC WITH HAND POSITION/ROTATION
//MAKE ELBOWS ROTATE WITH HAND
//FIX ROTATIONS (Stabbing needs to stay in the same direction, but the hand keeps going the wrong way if the character rotates
//               Also, for slashing, the character's chest should rotate with the slashing movement)
//SLASHING

public class AnimationGenerator : MonoBehaviour
{
    [Header("General Settings")]
    [SerializeField] float fAttackSpeed; //How fast should the attack be?
    [Range(0.1f, 1)]
    [SerializeField] float fAttackSkill; //affects things like how easily the sword is controlled. At low values the sword swings around and is hard to control, at high values the sword is easily controlled
    [SerializeField] float fAttackWeight; //The weight behind the sword attack
    [Min(0)]
    [SerializeField] int iNumOfAttacks; //number of animations generated (?)

    [Header("Rig")]
    [SerializeField] GameObject goCharacter;
    [SerializeField] GameObject goChest;
    [SerializeField] GameObject goHead;
    [SerializeField] GameObject goLeftHand;
    [SerializeField] GameObject goRightHand;
    [SerializeField] GameObject goLeftFoot;
    [SerializeField] GameObject goRightFoot;

    [Header("Stab Settings")]
    [SerializeField] Vector3[] stabLocations;
    [SerializeField] GameObject goStabMarkerPrefab;
    [SerializeField] float fStabMultiplier;

    Vector3 v3StabStart;
    [SerializeField] float fStabDistance = 0.5f;

    [SerializeField] int iIndex;
    Vector3 currentAttack;
    float fStep;

    [SerializeField] float fStabRotation; //rotation to move the chest to for stabs
    float fStabRotationCheck; //convert fStabRotation to a positive number because for some reason unity doesn't want to accept that -85 degrees = 275 degrees on a circle
    [SerializeField] float fRotateSpeed;

    [Header("Slash Settings")]
    [SerializeField] Vector3[] slashLocations;
    [SerializeField] float fSlashDistance = 0.5f;
    [SerializeField] GameObject goSlashMarkerPrefab;
    [SerializeField] GameObject goSlashPointPrefab;
    [SerializeField] float fAttackDirection; //A direction that the attacks will generally go in (not exactly in that direction however, might replace with enum)
    [SerializeField] bool bRandomDirection;
    [SerializeField] GameObject[] slashPoints;
    [SerializeField] Transform[] tSlashStarts; //start and end points of each slash
    [SerializeField] Transform[] tSlashEnds;
    [SerializeField] Transform[] tSlashWindups;
    [SerializeField] GameObject goSlashIdle;
    Vector3 v3SlashCentre;

    enum ATTACK_TYPE //What type of attack will it be?
    {
        THRUST,
        SLASH
    }
    [SerializeField] ATTACK_TYPE eAttackType;

    enum ANIMATION_STATE
    {
        NONE,
        WINDUP,
        ATTACK,
        RETURN
    }

    [SerializeField] ANIMATION_STATE eAnimState;

    // Start is called before the first frame update
    void Start()
    {
        eAnimState = ANIMATION_STATE.NONE;
    }

    // Update is called once per frame
    void Update()
    {
        switch (eAnimState)
        {
            case ANIMATION_STATE.NONE:
                break;
            case ANIMATION_STATE.WINDUP:
                //move to start positions
                switch (eAttackType)
                {
                    case ATTACK_TYPE.THRUST:
                        fStep = Time.deltaTime * fAttackSpeed;
                        //rotate hand back
                        goRightHand.transform.position = Vector3.Lerp(goRightHand.transform.position, v3StabStart, fStep);
                        if (goRightHand.transform.position == v3StabStart)
                        {
                            //rotate hand
                            iIndex++;
                            if (iIndex > stabLocations.Length - 1)
                            {
                                eAnimState = ANIMATION_STATE.RETURN;
                            }
                            else
                            {
                                eAnimState = ANIMATION_STATE.ATTACK;
                            }
                        }
                        break;
                    case ATTACK_TYPE.SLASH:
                        fStep = Time.deltaTime * fAttackSpeed;
                        //rotate chest to angle towards windup point
                        Vector3 chestLookAt = (tSlashWindups[iIndex].transform.position - v3SlashCentre) * 0.5f; //find position between windup and centre
                        goChest.transform.LookAt(chestLookAt, Vector3.up);
                        //rotate head
                        //move/rotate right hand to windup point
                        //move/rotate left hand to right hand and move "down" the "handle"
                        
                        break;
                    default:
                        break;
                }
                break;
            case ANIMATION_STATE.ATTACK:
                switch (eAttackType)
                {
                    case ATTACK_TYPE.SLASH:
                        //set current slash to next one

                        //move arms and chest to starting position of the slash

                        //once the arms get to starting position
                        //for each position of the slash
                        //lerp arm through points, rotating chest to move with arm

                        //after all positions, iterate
                        break;
                    case ATTACK_TYPE.THRUST:
                        currentAttack = stabLocations[iIndex];
                        goRightHand.transform.LookAt(currentAttack);
                        fStep = Time.deltaTime * fAttackSpeed;
                        goRightHand.transform.position = Vector3.Lerp(goRightHand.transform.position, currentAttack, fStep);
                        if (goRightHand.transform.position == currentAttack)
                        {
                            eAnimState = ANIMATION_STATE.WINDUP;
                        }
                        break;
                    default:
                        break;
                }
                break;
            case ANIMATION_STATE.RETURN:
                //return to default position
                switch (eAttackType)
                {
                    case ATTACK_TYPE.SLASH:
                        //return to idle

                        break;
                    case ATTACK_TYPE.THRUST:
                        //return to default
                        fStep = Time.deltaTime * fAttackSpeed;
                        //rotate hand back to normal
                        goRightHand.transform.position = Vector3.Lerp(goRightHand.transform.position, v3StabStart, fStep);
                        if (goRightHand.transform.position == v3StabStart)
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
    }

    public void Generate()
    {
        //put character in starting stance(?)
        //apply attack force on character
        //somehow record this as an animation if possible
        switch (eAttackType)
        {
            case ATTACK_TYPE.THRUST:
                //set a number of random positions in front of the character (spread affected by skill)
                v3StabStart = goCharacter.transform.position +
                    new Vector3(0.256f * goCharacter.transform.right.x, 1.435656f * goCharacter.transform.up.y, 0.032f * goCharacter.transform.forward.z);
                Vector3 stabCentre = v3StabStart +
                    (fStabDistance * goCharacter.transform.forward);
                stabLocations = new Vector3[iNumOfAttacks];
                for (int i = 0; i < iNumOfAttacks; i++)
                {
                    //if i is even, set the stab location to the starting position
                    //else, set to random location
                    Vector2 randomPos = Random.insideUnitCircle * (1 / fAttackSkill) * fStabMultiplier;
                    stabLocations[i] = stabCentre + (new Vector3(randomPos.x, randomPos.y, 0)); //find way to work in any rotation
                    GameObject.Instantiate(goStabMarkerPrefab, stabLocations[i], Quaternion.identity);
                }
                iIndex = 0;
                eAnimState = ANIMATION_STATE.WINDUP;
                break;
            case ATTACK_TYPE.SLASH:
                //plan slashing positions
                v3SlashCentre = goCharacter.transform.position +
                    new Vector3(0, 1.435656f * goCharacter.transform.up.y, 0) +
                    (fSlashDistance * goCharacter.transform.forward);
                slashLocations = new Vector3[iNumOfAttacks * 3];
                slashPoints = new GameObject[iNumOfAttacks];
                tSlashStarts = new Transform[iNumOfAttacks];
                tSlashEnds = new Transform[iNumOfAttacks];
                tSlashWindups = new Transform[iNumOfAttacks];

                for (int i = 0; i < iNumOfAttacks; i++)
                {
                    slashPoints[i] = Instantiate(goSlashPointPrefab, v3SlashCentre, goCharacter.transform.rotation);
                    //if there's one attack, use the preset direction
                    if (iNumOfAttacks == 1 && !bRandomDirection)
                    {
                        slashPoints[i].transform.Rotate(slashPoints[i].transform.forward, fAttackDirection + 90f); //add 90 degrees to rotation so that 0 is vertically down
                    }
                    else if (iNumOfAttacks > 1 || bRandomDirection) //else, randomise
                    {
                        float randomDirection = Random.Range(0, 359);
                        slashPoints[i].transform.Rotate(slashPoints[i].transform.forward, randomDirection);
                    }
                    if (slashPoints[i].GetComponent<SlashPoint>() != null) //is there a slash point script?
                    {
                        if (slashPoints[i].GetComponent<SlashPoint>().slashLocations.Length == 2) //Are there two slash locations?
                        {
                            tSlashStarts[i] = slashPoints[i].GetComponent<SlashPoint>().slashLocations[0].transform;
                            tSlashEnds[i] = slashPoints[i].GetComponent<SlashPoint>().slashLocations[1].transform;
                            tSlashWindups[i] = slashPoints[i].GetComponent<SlashPoint>().slashWindup.transform;
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
                iIndex = 0;
                eAnimState = ANIMATION_STATE.WINDUP;
                break;
            default:
                break;
        }
    }
}