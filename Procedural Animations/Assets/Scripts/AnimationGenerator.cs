using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AnimationGenerator : MonoBehaviour
{
    [SerializeField] float fAttackSpeed; //How fast should the attack be?
    [Range(0.1f, 1)]
    [SerializeField] float fAttackSkill; //affects things like how easily the sword is controlled. At low values the sword swings around and is hard to control, at high values the sword is easily controlled
    [SerializeField] float fAttackWeight; //The weight behind the sword attack
    [SerializeField] float fAttackDirection; //A direction that the attacks will generally go in (not exactly in that direction however, might replace with enum)
    [Min(0)]
    [SerializeField] int iNumOfAttacks; //number of animations generated (?)
    [SerializeField] bool bTwoHanded; //Is the sword two handed?

    [SerializeField] GameObject goLeftSword;
    [SerializeField] GameObject goRightSword;

    [SerializeField] GameObject goLeftStabStart;
    [SerializeField] Vector3 v3LeftStabCentre;

    [SerializeField] GameObject goRightStabStart;
    [SerializeField] Vector3 v3RightStabCentre;

    [SerializeField] Vector3[] stabLocations;
    [SerializeField] GameObject goStabMarkerPrefab;
    [SerializeField] float fStabMultiplier;
    
    [SerializeField] int iIndex;
    Vector3 currentAttack;
    float fStep;

    [SerializeField] GameObject goCharacter;


    [SerializeField] Vector3[] slashLocations;
    [SerializeField] float fSlashDistance = 0.5f;
    [SerializeField] float fStabDistance = 0.5f;
    [SerializeField] GameObject goSlashMarkerPrefab;
    [SerializeField] GameObject goSlashPointPrefab;
    [SerializeField] float fSlashRadius; //radius of the arc of the slash
    [SerializeField] float fSlashDepth; //depth of the arc of the slash
    [SerializeField] bool bRandomDirection;
    [SerializeField] GameObject[] slashPoints;

    enum SWORD_ARM //What arm will the sword(s) be in?
    {
        RIGHT,
        LEFT
    }

    enum ATTACK_TYPE //What type of attack will it be?
    {
        THRUST,
        SLASH
    }

    [SerializeField] SWORD_ARM eSwordArm;
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
                switch (eSwordArm)
                {
                    case SWORD_ARM.LEFT:
                        break;
                    case SWORD_ARM.RIGHT:
                        switch (eAttackType)
                        {
                            case ATTACK_TYPE.THRUST:
                                fStep = Time.deltaTime * fAttackSpeed;
                                goRightSword.transform.position = Vector3.Lerp(goRightSword.transform.position, goRightStabStart.transform.position, fStep);
                                if (goRightSword.transform.position == goRightStabStart.transform.position)
                                {
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
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
                break;
            case ANIMATION_STATE.ATTACK:
                switch (eSwordArm)
                {
                    case SWORD_ARM.LEFT:
                        switch (eAttackType)
                        {
                            case ATTACK_TYPE.SLASH:
                                break;
                            case ATTACK_TYPE.THRUST:
                                currentAttack = stabLocations[iIndex];
                                fStep = Time.deltaTime * fAttackSpeed;
                                goLeftSword.transform.position = Vector3.Lerp(goLeftSword.transform.position, currentAttack, fStep);
                                if (goLeftSword.transform.position == currentAttack)
                                {
                                    iIndex++;
                                }
                                if (iIndex > stabLocations.Length - 1)
                                {
                                    eAnimState = ANIMATION_STATE.RETURN;
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                    case SWORD_ARM.RIGHT:
                        switch (eAttackType)
                        {
                            case ATTACK_TYPE.SLASH:
                                //set current slash to next one

                                //move arm to starting position of the slash

                                //once the arm gets to starting position
                                //for each position of the slash
                                //lerp arm through points

                                //after all positions, iterate

                                //after all attacks, return to default positions
                                break;
                            case ATTACK_TYPE.THRUST:
                                currentAttack = stabLocations[iIndex];
                                fStep = Time.deltaTime * fAttackSpeed;
                                goRightSword.transform.position = Vector3.Lerp(goRightSword.transform.position, currentAttack, fStep);
                                if (goRightSword.transform.position == currentAttack)
                                {
                                    eAnimState = ANIMATION_STATE.WINDUP;
                                }
                                
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
                break;
            case ANIMATION_STATE.RETURN:
                //return to default position
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
        switch (eSwordArm)
        {
            case SWORD_ARM.LEFT:
                switch (eAttackType)
                {
                    case ATTACK_TYPE.THRUST:
                        //put character in thrusting stance (somehow)
                        //set a number of random positions in front of the character (spread affected by skill)
                        
                        break;
                    case ATTACK_TYPE.SLASH:
                        //put character in slashing stance
                        //apply slashing force
                        break;
                    default:
                        break;
                }
                break;
            case SWORD_ARM.RIGHT:
                switch (eAttackType)
                {
                    case ATTACK_TYPE.THRUST:
                        //set a number of random positions in front of the character (spread affected by skill)
                        Vector3 stabCentre = goCharacter.transform.position + 
                            new Vector3(0.27f * goCharacter.transform.right.x, 1.435656f * goCharacter.transform.up.y, 0.5f * goCharacter.transform.forward.z) + 
                            (fStabDistance * goCharacter.transform.forward);
                        stabLocations = new Vector3[iNumOfAttacks];
                        for (int i = 0; i < iNumOfAttacks; i++)
                        {
                            //if i is even, set the stab location to the starting position
                            //else, set to random location
                            Vector2 randomPos = Random.insideUnitCircle * (1 / fAttackSkill) * fStabMultiplier;
                            stabLocations[i] = v3RightStabCentre + (new Vector3(randomPos.x, randomPos.y, 0)); //find way to work in any rotation
                            GameObject.Instantiate(goStabMarkerPrefab, stabLocations[i], Quaternion.identity);
                        }
                        iIndex = 0;
                        eAnimState = ANIMATION_STATE.WINDUP;
                        break;
                    case ATTACK_TYPE.SLASH:
                        //plan slashing positions
                        Vector3 slashCentre = goCharacter.transform.position + 
                            new Vector3(0, 1.435656f * goCharacter.transform.up.y, 0) + 
                            (fSlashDistance * goCharacter.transform.forward);
                        slashLocations = new Vector3[iNumOfAttacks * 3];
                        slashPoints = new GameObject[iNumOfAttacks];

                        for (int i = 0; i < iNumOfAttacks; i++)
                        {
                            slashPoints[i] = Instantiate(goSlashPointPrefab, slashCentre, goCharacter.transform.rotation);
                            //if there's one attack, use the preset direction
                            if (iNumOfAttacks == 1 && !bRandomDirection)
                            {
                                slashPoints[i].transform.Rotate(slashPoints[i].transform.forward, fAttackDirection);
                            }
                            else if (iNumOfAttacks > 1 || bRandomDirection) //else, randomise
                            {
                                float randomDirection = Random.Range(0, 359);
                                slashPoints[i].transform.Rotate(slashPoints[i].transform.forward, randomDirection);
                            }
                            
                        }

                        

                        

                        //put character in slashing stance

                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
        
    }
}
