using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : LivingEntity {

    public const int maxViewDistance = 10;

    [EnumFlags]
    public Species diet;

    public CreatureAction currentAction;
    public Genes genes;
    public Color maleColour;
    public Color femaleColour;

    // Settings:
    float timeBetweenActionChoices = 1;
    float moveSpeed = 1.5f;
    float timeToDeathByHunger = 200;
    float timeToDeathByThirst = 200;

    float timeToDeathByHorny = 100;

    float drinkDuration = 6;
    float eatDuration = 10;
    float mateDuration = 20;

    float criticalPercent = 0.7f;

    // Visual settings:
    float moveArcHeight = .2f;

    // State:
    [Header ("State")]
    public float hunger;
    public float thirst;
    public float horny;

    protected LivingEntity foodTarget;
    protected LivingEntity mateTarget;
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

        material.color = (genes.isMale) ? maleColour : femaleColour;

        ChooseNextAction ();
    }



    protected virtual void Update () {
        // Increase hunger and thirst over time
        hunger += Time.deltaTime * 1 / timeToDeathByHunger;
        thirst += Time.deltaTime * 1 / timeToDeathByThirst;
        if (species == Species.Rabbit) {
            horny += Time.deltaTime * 1 / timeToDeathByHorny;
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
        }

        if (hunger >= 1) {
            Die (CauseOfDeath.Hunger);
        } else if (thirst >= 1) {
            Die (CauseOfDeath.Thirst);
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
            if (biggestUrge == horny && thirst < criticalPercent && hunger < criticalPercent) {
                if (genes.isMale) {
                    FindMate ();
                } else {
                    WaitMate ();
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
        
        LivingEntity mateSource = Environment.SensePotentialMates (coord, this);
        if (mateSource) {
            mateTarget = mateSource;
            currentAction = CreatureAction.GoingToMate;
            CreatePath (mateTarget.coord);
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
        if (mateTarget) {
            currentAction = CreatureAction.WaitingToMate;
        } else {
            LivingEntity mateSource = Environment.SensePotentialMates (coord, this);
            if (mateSource) {
                mateTarget = mateSource;
                currentAction = CreatureAction.WaitingToMate;
            } else {
                currentAction = CreatureAction.SearchingForMate;
            }
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
                    LookAt (mateTarget.coord);
                    currentAction = CreatureAction.Mating;
                } else {
                    Debug.Log(path);
                    Debug.Log(pathIndex);
                    StartMoveToCoord (path[pathIndex]);
                    pathIndex++;
                }
                break;
            case CreatureAction.WaitingToMate:
                if (Coord.AreNeighbours (coord, mateTarget.coord)) {
                    LookAt (mateTarget.coord);
                    currentAction = CreatureAction.Mating;
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
        if (target != coord) {
            Coord offset = target - coord;
            transform.eulerAngles = Vector3.up * Mathf.Atan2 (offset.x, offset.y) * Mathf.Rad2Deg;
        }
    }

    void HandleInteractions () {
        if (currentAction == CreatureAction.Eating) {
            if (foodTarget && hunger > 0) {
                float eatAmount = Mathf.Min (hunger, Time.deltaTime * 1 / eatDuration);
                if (foodTarget.species == Species.Plant) {
                    eatAmount = ((LivingEntity) foodTarget).Consume (eatAmount);
                } else {
                    ((LivingEntity) foodTarget).Consume (1);
                    eatAmount = eatAmount * 2;
                }
                hunger -= eatAmount;
            }
        } else if (currentAction == CreatureAction.Drinking) {
            if (thirst > 0) {
                thirst -= Time.deltaTime * 1 / drinkDuration;
                thirst = Mathf.Clamp01 (thirst);
            }
        } else if (currentAction == CreatureAction.Mating) {
            if (mateTarget && horny > 0) {
                this.Mate();
                ((Animal) mateTarget).Mate();
            }
        }
    }

    void Mate () {
        float mateAmount = Mathf.Min (horny, Time.deltaTime * 1 / mateDuration);
        horny -= mateAmount;
    }

    void AnimateMove () {
        // Move in an arc from start to end tile
        moveTime = Mathf.Min (1, moveTime + Time.deltaTime * moveSpeed * moveSpeedFactor);
        float height = (1 - 4 * (moveTime - .5f) * (moveTime - .5f)) * moveArcHeight * moveArcHeightFactor;
        transform.position = Vector3.Lerp (moveStartPos, moveTargetPos, moveTime) + Vector3.up * height;

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