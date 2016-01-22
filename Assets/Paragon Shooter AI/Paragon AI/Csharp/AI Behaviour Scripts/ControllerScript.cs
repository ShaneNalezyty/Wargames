﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * This script provides a number of functions, which mostly includeproviding agents with lists of cover nodes and other agents
 * It is also used to allow the agents to hear sounds
 * */

namespace ParagonAI
{
    public class ControllerScript : MonoBehaviour
    {

        public static ParagonAI.ControllerScript currentController = null;
        ParagonAI.CoverNodeScript[] coverNodeScripts;
        List<ParagonAI.Target> currentTargets = new List<ParagonAI.Target>();
        int currentID = 0;

        //Dynamic Cover
        List<Vector3> currentDynamicCoverSpots = new List<Vector3>();
        public float minDistForDynamicCoverSimilarity = 3;
        public LayerMask layerMask;

        // Use this for initialization
        void Awake()
        {
            currentController = this;
            transform.tag = "AI Controller";
            GameObject[] tempCoverNodeObjects = GameObject.FindGameObjectsWithTag("Cover");

            minDistForDynamicCoverSimilarity = minDistForDynamicCoverSimilarity * minDistForDynamicCoverSimilarity;

            //Compile all cover nodes into a list in order to quickly find them
            //Prevents us from doing costly FindWithTag and GetComponent
            //Also doesn't require the use of tags.
            List<ParagonAI.CoverNodeScript> tempScripsList = new List<ParagonAI.CoverNodeScript>();
            for (int i = 0; i < tempCoverNodeObjects.Length; i++)
            {
                tempScripsList.Add(tempCoverNodeObjects[i].GetComponent<ParagonAI.CoverNodeScript>());
            }
            coverNodeScripts = tempScripsList.ToArray();
        }

        //Updateing and adding to lists
        public void UpdateAllEnemiesEnemyLists()
        {
		    //Provide agents with lists of allys and enemies
            //Again, lets us not use tags and is faster
            for (int y = 0; y < currentTargets.Count; y++)
            {
                currentTargets[y].targetScript.UpdateEnemyAndAllyLists(GetCurrentTargetsWithIDs(currentTargets[y].targetScript.alliedTeamsIDs),
                                                                   GetCurrentTargetsWithIDs(currentTargets[y].targetScript.enemyTeamsIDs));
            }
        }

        //Add a TargetScript to the game
        //Without this, agents wouldn't be able to find a target
        public int AddTarget(int id, Transform transformToAdd, ParagonAI.TargetScript script)
        {
            currentID++;
            ParagonAI.Target agentToAdd = new ParagonAI.Target(currentID, id, transformToAdd, script);
            currentTargets.Add(agentToAdd);
            UpdateAllEnemiesEnemyLists();
            return currentID;
        }

        //Used upon agent death
        //Eliminates the target from all lists.
        public void RemoveTargetFromTargetList(int id)
        {
            if (currentTargets.Count > 0)
            {
                for (int y = 0; y < currentTargets.Count; y++)
                {
                    if (currentTargets[y].targetScript.GetUniqueID() == id)
                    {
                        currentTargets.RemoveAt(y);
                        UpdateAllEnemiesEnemyLists();
                        return;
                    }
                }
            }
        }

        //Sounds			
        //Let's user create a sound that is only heard by specified teams.
        //NOTE THAT PARAGON AI SOUNDS ARE NOT RELATED IN ANY WAY WITH SOUNDS THAT ARE AUDIBLE TO THE PLAYER
        public void CreateSound(Vector3 pos, float radius, int[] teams)
        {
            radius = radius * radius;
            int y = 0;
            for (int x = 0; x < currentTargets.Count; x++)
            {
                for (y = 0; y < teams.Length; y++)
                {
                    //If the agent is on one of the specified teams and within the designated radius, then inform them of the sound
                    if (currentTargets[x].transform && currentTargets[x].teamID == teams[y])
                    {
                        if (Vector3.SqrMagnitude(currentTargets[x].transform.position - pos) < radius)
                        {
                            currentTargets[x].targetScript.HearSound(pos);
                        }
                        y = teams.Length;
                    }
                }
            }
        }

        //All
        public void CreateSound(Vector3 pos, float radius)
        {
            radius = radius * radius;
            //int y = 0;
            for (int x = 0; x < currentTargets.Count; x++)
            {
                //Any agents within the specified radius of the sound are informed of the sound.
                if (Vector3.SqrMagnitude(currentTargets[x].transform.position - pos) < radius)
                {
                    currentTargets[x].targetScript.HearSound(pos);
                }
            }
        }

        //Dynamic Cover	
		//Prevents agents from taking dynamic cover too close too each other.
	    //Otherwise, on a navemesh with a dense assortment of vertices agents would be trying to phase through each other to reach their cover nodes.
        public bool isDynamicCoverSpotCurrentlyUsed(Vector3 v)
        {
            for (int i = 0; i < currentDynamicCoverSpots.Count; i++)
            {
                if (Vector3.SqrMagnitude(v - currentDynamicCoverSpots[i]) < minDistForDynamicCoverSimilarity)
                {
                    return true;
                }
            }

            return false;
        }

        public void AddACoverSpot(Vector3 v)
        {
            currentDynamicCoverSpots.Add(v);
        }

        public void RemoveACoverSpot(Vector3 v)
        {
            currentDynamicCoverSpots.Remove(v);
        }

        //Cters
        public ParagonAI.CoverNodeScript[] GetCovers()
        {
            return coverNodeScripts;
        }

        //Finds the TRANSFORMS of agents on the given teams
        public Transform[] GetCurrentAIsWithIDs(int[] ids)
        {
            List<Transform> targets = new List<Transform>();
            int x;
            for (int i = 0; i < currentTargets.Count; i++)
            {
                for (x = 0; x < ids.Length; x++)
                {
                    //Will not detect any targets with a targetPriority lower than 0
                    if (ids[x] == currentTargets[i].teamID && currentTargets[i].targetScript.targetPriority >= 0)
                    {
                        targets.Add(currentTargets[i].transform);
                        break;
                    }
                }
            }
            return targets.ToArray();
        }

        //Finds the TARGET CLASSES of agents on the given teams
        public ParagonAI.Target[] GetCurrentTargetsWithIDs(int[] ids)
        {
            List<ParagonAI.Target> targets = new List<ParagonAI.Target>();
            int x;
            for (int i = 0; i < currentTargets.Count; i++)
            {
                for (x = 0; x < ids.Length; x++)
                {
                    //Will not detect any targets with a targetPriority lower than 0
                    if (ids[x] == currentTargets[i].teamID && currentTargets[i].targetScript.targetPriority >= 0)
                    {
                        targets.Add(currentTargets[i]);
                        break;
                    }
                }
            }
            return targets.ToArray();
        }

        //Finds the TARGET CLASSES of agents on the given teams within the given radius of the given origin position.
        public ParagonAI.Target[] GetCurrentAIsWithinRadius(int[] ids, float rad, Vector3 origin)
        {
            List<ParagonAI.Target> targets = new List<ParagonAI.Target>();
            rad = rad * rad;
            int x;
            int i;
            for (i = 0; i < currentTargets.Count; i++)
            {
                for (x = 0; x < ids.Length; x++)
                {
                    //Will not detect any targets with a targetPriority lower than 0
                    if (ids[x] == currentTargets[i].teamID && currentTargets[i].targetScript.targetPriority >= 0)
                    {
                        if (Vector3.SqrMagnitude(currentTargets[i].transform.position - origin) < rad)
                        {
                            targets.Add(currentTargets[i]);
                            break;
                        }
                    }
                }
            }
            return targets.ToArray();
        }

        //Gets all targets, regardless of priority, team, or distance.
        public ParagonAI.Target[] GetCurrentTargets()
        {
            ParagonAI.Target[] targets = currentTargets.ToArray();
            return targets;
        }

        //Returns the universal AI layermask
        public LayerMask GetLayerMask()
        {
            return layerMask;
        }
    }
}

//Simple class that holds information for each target
namespace ParagonAI
{
    public class Target
    {
        public int uniqueIdentifier;
        public int teamID;
        public Transform transform;
        public ParagonAI.TargetScript targetScript;

        public Target(int identity, int id, Transform transformToAdd, ParagonAI.TargetScript script)
        {
            uniqueIdentifier = identity;
            teamID = id;
            transform = transformToAdd;
            targetScript = script;
        }
    }
}