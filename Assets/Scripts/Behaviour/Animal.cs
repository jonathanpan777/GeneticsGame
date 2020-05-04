using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Animal : LivingEntity {

    /**
    ALL UPGRADE VARIABLES HERE
    **/
    public static float sexualUpgrade = 1f; 
    public static float defenseUpgrade = 1f; 
    public static float attackUpgrade = 1f; 
    public static float speedUpgrade = 1f;

    public static int points = 10;

    /**
    EVOLVE TRAITS
    **/
    public static bool luckyMate1 = false; 
    public static bool luckyMate2 = false;

    public static bool fastKids = false; 
    public static bool opKids = false;

    public static bool stun = false; 
    public static bool fightBack = false; 

    public static float deathThresh = 0.5f;

    public const int maxViewDistance = 10;

    [EnumFlags]
    public Species diet;

    public CreatureAction currentAction;
    public Genes genes;
    public Color maleColour;
    public Color femaleColour;

    // Settings:
    float timeBetweenActionChoices = 1;
    public float moveSpeed = 1.5f;
    float timeToDeathByHunger = 200;
    float timeToDeathByThirst = 200;

    float timeToDeathByHorny = 800;

    float drinkDuration = 6;
    float eatDuration = 10;
    float mateDuration = 20;

    float criticalPercent = 0.7f * deathThresh;

    // Visual settings:
    float moveArcHeight = .2f;

    // State:
    [Header ("State")]
    public float hunger;
    public float thirst;
    public float horny;

    protected LivingEntity foodTarget;
    protected Animal mateTarget;
    protected Coord waterTarget;

    // Move data:
    bool animatingMovement;
    Coord moveFromCoord;
    Coord moveTargetCoord;
    Vector3 moveStartPos;
    Vector3 moveTargetPos;
    float moveTime;
    float moveSpeedFactor;
    float moveArcHeightFactor;
    Coord[] path;
    int pathIndex;

    // Other
    float lastActionChooseTime;
    const float sqrtTwo = 1.4142f;
    const float oneOverSqrtTwo = 1 / sqrtTwo;
    
    public override void Init (Coord coord) {
        base.Init (coord);
        moveFromCoord = coord;
        genes = Genes.RandomGenes (1);

        // Change Color depending on gender
        if (species == Species.Rabbit) {
            gameObject.transform.GetChild(0).GetChild(1).GetComponent<SkinnedMeshRenderer>().materials[0].SetColor("_Color", (genes.isMale) ? maleColour : femaleColour);
            gameObject.transform.GetChild(0).GetChild(2).GetComponent<SkinnedMeshRenderer>().materials[1].SetColor("_Color", (genes.isMale) ? maleColour : femaleColour);
        }

        ChooseNextAction ();
    }

    public static void incrementSex() {
        sexualUpgrade -= 0.1f;
    }
    public static void incrementDefense() {
        defenseUpgrade *= 0.1f;
    }
    public static void incrementAttack() {
        attackUpgrade -= 0.1f;
    }
    public static void incrementSpeed() {
        speedUpgrade += 0.20f;
    }

    public static void fightBackEvolve()
    {
        Environment.fightBackGene();
    }

    protected virtual void Update () {

        if (this == null)
        {
            return;
        }

        // Increase hunger and thirst over time
        hunger += Time.deltaTime * 1 / timeToDeathByHunger;
        thirst += Time.deltaTime * 1 / timeToDeathByThirst;
        if (species == Species.Rabbit && growScale >= 1) {
            horny += Time.deltaTime * 1 / (timeToDeathByHorny * sexualUpgrade);
        }

        // Animate movement. After moving a single tile, the animal will be able to choose its next action
        if (animatingMovement) {
            AnimateMove ();
        } else {
            // Handle interactions with external things, like food, water, mates
            HandleInteractions ();
            float timeSinceLastActionChoice = Time.time - lastActionChooseTime;
            if (timeSinceLastActionChoice > timeBetweenActionChoices) {
                ChooseNextAction ();
            }
            if (growScale < 1) {
                growScale += 0.005f;
                amountRemaining += 0.005f;
                transform.localScale = Vector3.one * growScale;
                moveSpeed = 1.5f * growScale * speedUpgrade;
            }
        }

        if (hunger >= deathThresh ){//* defenseUpgrade) {
            if (this.species != Species.Fox) {
                Environment.DecrementBunnyCount();
                Die (CauseOfDeath.Hunger);
            }
        } else if (thirst >= deathThresh ){//* defenseUpgrade) {
            if (this.species != Species.Fox) {
                Environment.DecrementBunnyCount();
                Die (CauseOfDeath.Thirst);
            }
        }
    }

    // Animals choose their next action after each movement step (1 tile),
    // or, when not moving (e.g interacting with food etc), at a fixed time interval
    protected virtual void ChooseNextAction () {
        lastActionChooseTime = Time.time;
        // Get info about surroundings

        // Decide next action:
        // Eat if (more hungry than thirsty) or (currently eating and not critically thirsty)
        bool currentlyEating = currentAction == CreatureAction.Eating && foodTarget && hunger > 0;
        bool currentlyMating = currentAction == CreatureAction.Mating && mateTarget && horny > 0;

        float biggestUrge = Mathf.Max(hunger, thirst, horny);
        if (currentlyEating && thirst < criticalPercent) {
            FindFood ();
        } else if (currentlyMating && thirst < criticalPercent) {
            if (genes.isMale) {
                Mate ();
            } else {
                Mate ();
            }
        } else {
            gameObject.GetComponent<Animator>().SetBool("eating", false);
            if (currentAction == CreatureAction.GoingToMate || currentAction == CreatureAction.WaitingToMate || biggestUrge == horny && thirst < criticalPercent && hunger < criticalPercent) {
                if (genes.isMale) {
                    FindMate();
                }
                else {
                    WaitMate();
                }
            } else if (biggestUrge == hunger || hunger > criticalPercent) {
                FindFood ();
            } else if (biggestUrge == thirst || thirst > criticalPercent) {
                FindWater ();
            }
        }

        Act ();

    }

    protected virtual void FindFood () {
        LivingEntity foodSource = Environment.SenseFood (coord, this, FoodPreferencePenalty);
        if (foodSource) {
            currentAction = CreatureAction.GoingToFood;
            foodTarget = foodSource;
            CreatePath (foodTarget.coord);
        } else {
            currentAction = CreatureAction.Exploring;
        }
    }

    protected virtual void FindWater () {
        Coord waterTile = Environment.SenseWater (coord);
        if (waterTile != Coord.invalid) {
            currentAction = CreatureAction.GoingToWater;
            waterTarget = waterTile;
            CreatePath (waterTarget);
        } else {
            currentAction = CreatureAction.Exploring;
        }
    }

    protected virtual void FindMate () {
        
        Animal mateSource = Environment.SensePotentialMates (coord, this);
        if (mateTarget || currentAction == CreatureAction.GoingToMate) {
            //CreatePath (mateTarget.coord);
        } else if (mateSource) {
            mateTarget = mateSource;
            currentAction = CreatureAction.GoingToMate;
            foodTarget = null;
            CreatePath (mateTarget.coord);

            mateTarget.mateTarget = this;
            mateTarget.currentAction = CreatureAction.WaitingToMate;
        } else {
            currentAction = CreatureAction.SearchingForMate;
        }
        // if (mateTarget) {
        //     // Debug.Log(mateTarget.coord);
        //     currentAction = CreatureAction.GoingToMate;
        //     CreatePath (mateTarget.coord);
        // } else {
        //     LivingEntity mateSource = Environment.SensePotentialMates (coord, this);
        //     if (mateSource) {
        //         mateTarget = mateSource;
        //         currentAction = CreatureAction.GoingToMate;
        //         CreatePath (mateTarget.coord);
        //     } else {
        //         currentAction = CreatureAction.SearchingForMate;
        //     }
        // }
    }
    protected virtual void WaitMate () {
        if (mateTarget && mateTarget.currentAction == CreatureAction.GoingToMate) {
            currentAction = CreatureAction.WaitingToMate;
            LookAt(mateTarget.coord);
            gameObject.GetComponent<Animator>().SetBool("waiting", true);
        } else {
            /*LivingEntity mateSource = Environment.SensePotentialMates (coord, this);
            if (mateSource) {
                mateTarget = mateSource;
                currentAction = CreatureAction.WaitingToMate;
            } else {
                currentAction = CreatureAction.SearchingForMate;
            }*/
            currentAction = CreatureAction.SearchingForMate;
            mateTarget = null;
        }
    }



    // When choosing from multiple food sources, the one with the lowest penalty will be selected
    protected virtual int FoodPreferencePenalty (LivingEntity self, LivingEntity food) {
        return Coord.SqrDistance (self.coord, food.coord);
    }

    protected void Act () {
        switch (currentAction) {
            case CreatureAction.Exploring:
                StartMoveToCoord (Environment.GetNextTileWeighted (coord, moveFromCoord));
                break;
            case CreatureAction.GoingToFood:
                if (Coord.AreNeighbours (coord, foodTarget.coord)) {
                    LookAt (foodTarget.coord);
                    currentAction = CreatureAction.Eating;
                } else {
                    StartMoveToCoord (path[pathIndex]);
                    pathIndex++;
                }
                break;
            case CreatureAction.GoingToWater:
                if (Coord.AreNeighbours (coord, waterTarget)) {
                    LookAt (waterTarget);
                    currentAction = CreatureAction.Drinking;
                } else {
                    StartMoveToCoord (path[pathIndex]);
                    pathIndex++;
                }
                break;
            case CreatureAction.SearchingForMate:
                StartMoveToCoord (Environment.GetNextTileWeighted (coord, moveFromCoord));
                break;
            case CreatureAction.GoingToMate:
                if (Coord.AreNeighbours (coord, mateTarget.coord)) {

                    // Match horniness
                    float minHorny = Mathf.Min(horny, mateTarget.horny);
                    horny = minHorny;
                    mateTarget.horny = minHorny;

                    // Look at each other and mate
                    LookAt (mateTarget.coord);
                    currentAction = CreatureAction.Mating;
                    mateTarget.currentAction = CreatureAction.Mating;
                    mateTarget.LookAt(this.coord);
                    Environment.spawnChild(mateTarget.coord, bunnyPrefab, foxPrefab);
                    points += 1;
                    Debug.Log(luckyMate1);
                    float ra = Random.Range(0f, 1f);
                    Debug.Log(ra);
                    if (luckyMate1 && ra <= 0.15f || luckyMate2 && ra <= 0.30f)
                    {
                        Environment.spawnChild(mateTarget.coord, bunnyPrefab, foxPrefab);
                        points += 1;
                    }

                } else {
                    //Debug.Log(path);
                    //Debug.Log(pathIndex);
                    StartMoveToCoord (path[pathIndex]);
                    pathIndex++;
                }
                break;
            case CreatureAction.WaitingToMate:
                if (mateTarget.currentAction != CreatureAction.GoingToMate) {
                    mateTarget = null;
                    currentAction = CreatureAction.SearchingForMate;
                    break;
                }
                gameObject.GetComponent<Animator>().SetBool("waiting", true);
                if (Coord.AreNeighbours (coord, mateTarget.coord)) {
                    LookAt (mateTarget.coord);
                    //currentAction = CreatureAction.Mating;
                }
                break;
        }
    }

    protected void CreatePath (Coord target) {
        // Create new path if current is not already going to target
        if (path == null || pathIndex >= path.Length || (path[path.Length - 1] != target || path[pathIndex - 1] != moveTargetCoord)) {
            path = EnvironmentUtility.GetPath (coord.x, coord.y, target.x, target.y);
            pathIndex = 0;
        }
    }

    protected void StartMoveToCoord (Coord target) {
        moveFromCoord = coord;
        moveTargetCoord = target;
        moveStartPos = transform.position;
        moveTargetPos = Environment.tileCentres[moveTargetCoord.x, moveTargetCoord.y];
        animatingMovement = true;

        bool diagonalMove = Coord.SqrDistance (moveFromCoord, moveTargetCoord) > 1;
        moveArcHeightFactor = (diagonalMove) ? sqrtTwo : 1;
        moveSpeedFactor = (diagonalMove) ? oneOverSqrtTwo : 1;

        LookAt (moveTargetCoord);
    }

    protected void LookAt (Coord target) {
        if (this == null)
        {
            return;
        }
        if (target != coord) {
            Coord offset = target - coord;
            transform.eulerAngles = Vector3.up * Mathf.Atan2 (offset.x, offset.y) * Mathf.Rad2Deg;
        }
    }

    void HandleInteractions () {

        // Stop walking animation
        gameObject.GetComponent<Animator>().SetBool("moving", false);

        // handle interactions
        if (currentAction == CreatureAction.Eating) {
            gameObject.GetComponent<Animator>().SetBool("mating", false);
            gameObject.GetComponent<Animator>().SetBool("eating", true);
            if (foodTarget && hunger > 0) {
                float eatAmount = Mathf.Min(hunger, Time.deltaTime * 1 / (eatDuration * attackUpgrade));
                if (foodTarget.species == Species.Plant) {
                    eatAmount = ((LivingEntity) foodTarget).Consume (eatAmount);
                } else {
                    ((LivingEntity)foodTarget).Consume(0.125f * defenseUpgrade);
                    eatAmount = eatAmount * 2;
                }
                hunger -= eatAmount;
            }
        } else if (currentAction == CreatureAction.Drinking) {
            gameObject.GetComponent<Animator>().SetBool("mating", false);
            gameObject.GetComponent<Animator>().SetBool("eating", true);
            if (thirst > 0) {
                thirst -= Time.deltaTime * 1 / (drinkDuration * attackUpgrade);
                thirst = Mathf.Clamp01 (thirst);
            }
        } else if (currentAction == CreatureAction.Mating) {

            // Mating dance animation
            gameObject.GetComponent<Animator>().SetBool("waiting", false);
            gameObject.GetComponent<Animator>().SetBool("mating", true);

            if (mateTarget && horny > 0) {
                this.Mate();
                //(mateTarget).Mate();
            }
        }
    }

    void Mate () {

        // Decrease horniness
        float mateAmount = Time.deltaTime * 1 / mateDuration;
        horny -= mateAmount;

        // Finish mating, set target to null and end animation
        if (horny <= 0f && mateTarget.horny <= 0f)
        {
            gameObject.GetComponent<Animator>().SetBool("mating", false);
            mateTarget.mateTarget = null;
            mateTarget = null;
        }

        horny = Mathf.Clamp01(horny);
    }

    void AnimateMove () {
        // Move in an arc from start to end tile
        moveTime = Mathf.Min (1, moveTime + Time.deltaTime * moveSpeed * speedUpgrade * moveSpeedFactor);
        //float height = (1 - 4 * (moveTime - .5f) * (moveTime - .5f)) * moveArcHeight * moveArcHeightFactor;
        transform.position = Vector3.Lerp(moveStartPos, moveTargetPos, moveTime);// + Vector3.up * height;

        // Start walking Animation
        gameObject.GetComponent<Animator>().SetBool("mating", false);
        gameObject.GetComponent<Animator>().SetBool("moving", true);

        // Finished moving
        if (moveTime >= 1) {
            Environment.RegisterMove (this, coord, moveTargetCoord);
            coord = moveTargetCoord;

            animatingMovement = false;
            moveTime = 0;
            ChooseNextAction ();
        }
    }

    void OnDrawGizmosSelected () {
        if (Application.isPlaying) {
            var surroundings = Environment.Sense (coord);
            Gizmos.color = Color.white;
            if (surroundings.nearestFoodSource != null) {
                Gizmos.DrawLine (transform.position, surroundings.nearestFoodSource.transform.position);
            }
            if (surroundings.nearestWaterTile != Coord.invalid) {
                Gizmos.DrawLine (transform.position, Environment.tileCentres[surroundings.nearestWaterTile.x, surroundings.nearestWaterTile.y]);
            }

            if (currentAction == CreatureAction.GoingToFood) {
                var path = EnvironmentUtility.GetPath (coord.x, coord.y, foodTarget.coord.x, foodTarget.coord.y);
                Gizmos.color = Color.black;
                for (int i = 0; i < path.Length; i++) {
                    Gizmos.DrawSphere (Environment.tileCentres[path[i].x, path[i].y], .2f);
                }
            }
        }
    }

}