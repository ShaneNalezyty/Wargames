﻿using UnityEngine;
using System.Collections;
using System.Text;
using ParagonAI;

namespace WarGames {
    public class WanderAction : Actionable {
        private BaseScript baseScript;
        private bool haveWanderPoint;
        private int wanderDistance;
        private FindCoverAction findCoverAction;
        private bool lastRanIdle;
        private bool firstRunOfThisAction = true;
        private Soldier soldier;
        public WanderAction(BaseScript soldiersBaseScript, int distance) {
            baseScript = soldiersBaseScript;
            wanderDistance = distance;
        }
        public void OnComplete() {

        }
        public void OnEnd() {
            //Make sure if we end that we leave any cover
            if ( findCoverAction != null) {
                findCoverAction.LeaveCover();
            }
        }
        public bool NextAICycle( bool inCombat ) {
            if ( firstRunOfThisAction ) {
                baseScript.coverFinderScript.shouldUseDynamicCover = true;

                soldier = baseScript.gameObject.GetComponent<Soldier>();
                soldier.WriteToLog( "I'm starting a WanderAction", "A" );
                firstRunOfThisAction = false;
            }
            if (inCombat) {
                if ( lastRanIdle ) {
                    soldier.WriteToLog( "I've entered combat in a WanderAction", "A" );
                }
                lastRanIdle = false;
                //If in combat then run the combat version of this action.
                return NextCombatAICycle();
            } else {
                if ( !lastRanIdle ) {
                    soldier.WriteToLog( "I've left combat in a WanderAction", "A" );
                }
                lastRanIdle = true;
                //If not in combat then run the idle version of this action.
                return NextIdleAICycle();
            }
        }
        private bool NextIdleAICycle() {
            findCoverAction = null;
            NavmeshInterface navI = baseScript.currentBehaviour.navI;
            if (!haveWanderPoint) {
                //If we don't have a wander location then create one
                baseScript.currentBehaviour.targetVector = FindDestinationWithinRadius( baseScript.currentBehaviour.myTransform.position );
                haveWanderPoint = true;
            } else if (!navI.PathPending() && navI.GetRemainingDistance() < (wanderDistance / 10)) {
                //If we have a wander location and have gotten at least 90% to the location then trigger to find
                //a new location to wander to.
                haveWanderPoint = false;
            }
            return false;
        }
        private Vector3 FindDestinationWithinRadius( Vector3 originPos ) {
            //Returns destination within a square radius.
            return new Vector3( originPos.x + (Random.value - 0.5f) * wanderDistance, originPos.y, originPos.z + (Random.value - 0.5f) * wanderDistance );
        }
        private bool NextCombatAICycle() {
            //If we are wandering and enter combat we should go take cover
            if (findCoverAction == null) {
                //If this agent is entering combat we need to create a findCoverAction
                findCoverAction = new FindCoverAction( baseScript, baseScript.targetTransform.position, float.MaxValue );
            }
            return findCoverAction.NextAICycle( true );
        }
        override public string ToString() {
            //Return that this is a WonderAction and the distance the agent is allowed to wander.
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine( "WanderAction: " );
            stringBuilder.AppendLine( "WanderDistance: " + wanderDistance );
            return stringBuilder.ToString( 0, stringBuilder.Length - 1 );
        }
    }
}