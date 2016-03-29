﻿using UnityEngine;
using System.Collections;
using System.Text;

namespace WarGames {
    /// <summary>
    /// Contains a set of actions that when executed sequentially satisfies a goal.
    /// </summary>
    /// <seealso cref="WarGames.Goal" />
    public class Plan {
        private Goal goalToSatisfy;
        private ArrayList actionPlan;
        private int currentAction;
        public Plan( Goal goal, ArrayList actions ) {
            actionPlan = actions;
            actionPlan.TrimToSize();
            goalToSatisfy = goal;
            currentAction = 0;
        }
        public void NextAICycle( bool inCombat ) {
            Actionable currentAction = GetCurrentAction();
            bool actionComplete = currentAction.NextAICycle( inCombat );
            if (actionComplete) {
                CompleteAction();
            }
        }
        private Actionable GetCurrentAction() {
            return (Actionable)actionPlan[currentAction];
        }

        private void CompleteAction() {
            GetCurrentAction().OnComplete();
            currentAction++;
        }
        public void EndAction() {
            GetCurrentAction().OnEnd();
        }
        public int GetPercentDone() {
            return (int)(100*(currentAction / actionPlan.Capacity));
        }
        public bool Satisfies( Goal checkGoal ) {
            if (checkGoal != null) {
                return goalToSatisfy.Equals( checkGoal );
            } else {
                if (goalToSatisfy == checkGoal) {
                    return true;
                } else {
                    return false;
                }
            }
        }
        override public string ToString() {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine( "This plan contains " + actionPlan.Capacity + " actions." );
            stringBuilder.AppendLine( "This plan is currently " + GetPercentDone() + "% complete." );
            stringBuilder.AppendLine( "Goal this plan satisfies: " );
            if (goalToSatisfy != null) {
                stringBuilder.AppendLine( goalToSatisfy.ToString() );
            }
            stringBuilder.AppendLine( "List of actions: " );
            foreach (Actionable action in actionPlan) {
                stringBuilder.Append( action.ToString() );
            }
            stringBuilder.AppendLine( "End of plan." );
            return stringBuilder.ToString( 0, stringBuilder.Length - 1 );
        }
    }
}