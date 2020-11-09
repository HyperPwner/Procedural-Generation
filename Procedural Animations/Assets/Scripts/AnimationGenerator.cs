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
    [SerializeField] int iNumOfAttacks; //number of animations generated (?)
    [SerializeField] bool bTwoHanded; //Is the sword two handed?

    [SerializeField] GameObject goLeftSword;
    [SerializeField] GameObject goRightSword;

    [SerializeField] Vector3 v3LeftStabStart;
    [SerializeField] Vector3 v3LeftStabCentre;

    [SerializeField] GameObject goRightStabStart;
    [SerializeField] Vector3 v3RightStabCentre;

    [SerializeField] Vector3[] stabLocations;
    [SerializeField] GameObject goStabMarkerPrefab;
    [SerializeField] float fStabMultiplier;
    
    int iIndex;
    Vector3 currentAttack;
    float fStep;
    bool bAnimate;

    enum SWORD_ARM //What arm will the sword(s) be in?
    {
        RIGHT,
        LEFT,
        BOTH
    }

    enum ATTACK_TYPE //What type of attack will it be?
    {
        THRUST,
        SLASH,
        SPIN
    }

    [SerializeField] SWORD_ARM eSwordArm;
    [SerializeField] ATTACK_TYPE eAttackType;



    // Start is called before the first frame update
    void Start()
    {
        bAnimate = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (bAnimate)
        {
            currentAttack = stabLocations[iIndex];
            fStep += Time.deltaTime * fAttackSpeed;
            goRightSword.transform.position = Vector3.Lerp(goRightSword.transform.position, currentAttack, fStep);
            //goRightSword.transform.position = Vector3.Lerp(goRightSword.transform.position, goRightStabStart.transform.position, fStep);
            if (goRightSword.transform.position == currentAttack)
            {
                iIndex++;
            }
            if (iIndex > iNumOfAttacks - 1)
            {
                bAnimate = false;
            }

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
                    case ATTACK_TYPE.SPIN:
                        //put character in spinning stance
                        //apply rotational force
                        break;
                }
                break;
            case SWORD_ARM.RIGHT:
                switch (eAttackType)
                {
                    case ATTACK_TYPE.THRUST:
                        //put character in thrusting stance (somehow)
                        //set a number of random positions in front of the character (spread affected by skill)
                        stabLocations = new Vector3[iNumOfAttacks];
                        for (int i = 0; i < iNumOfAttacks; i++)
                        {
                            Vector2 randomPos = Random.insideUnitCircle * (1 / fAttackSkill) * fStabMultiplier;
                            stabLocations[i] = v3RightStabCentre + (new Vector3(randomPos.x, randomPos.y, 0)); //find way to work in any rotation
                            GameObject.Instantiate(goStabMarkerPrefab, stabLocations[i], Quaternion.identity);
                        }
                        goRightSword.transform.position = goRightStabStart.transform.position;
                        goRightSword.transform.rotation = goRightStabStart.transform.rotation;
                        iIndex = 0;
                        bAnimate = true;
                        break;
                    case ATTACK_TYPE.SLASH:
                        //put character in slashing stance
                        //apply slashing force
                        break;
                    case ATTACK_TYPE.SPIN:
                        //put character in spinning stance
                        //apply rotational force
                        break;
                }
                break;
            case SWORD_ARM.BOTH:
                switch (eAttackType)
                {
                    case ATTACK_TYPE.THRUST: //?
                        //put character in thrusting stance (somehow)
                        //set a number of random positions in front of the character (spread affected by skill)
                        break;
                    case ATTACK_TYPE.SLASH:
                        //put character in slashing stance
                        //apply slashing force
                        break;
                    case ATTACK_TYPE.SPIN:
                        //put character in spinning stance
                        //apply rotational force
                        break;
                }
                break;
        }
        
    }
}
