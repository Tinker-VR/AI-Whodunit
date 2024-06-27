using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Convai.Scripts.Narrative_Design;

[Serializable]
public class EvidenceEntry
{
    public string name;
    public bool found;
}

public class EvidenceManager : MonoBehaviour{

    [SerializeField]
    private List<EvidenceEntry> evidenceList = new List<EvidenceEntry>();
    public NarrativeDesignTrigger allEvidenceFoundTrigger;
    public NarrativeDesignTrigger oliviaGameEndTrigger;
    public NarrativeDesignTrigger markGameEndTrigger;
    public NarrativeDesignTrigger noahGameEndTrigger;
    public NarrativeDesignTrigger jamesGameEndTrigger;
    
    



    public void MarkEvidenceAsFound(string evidenceName)
    {
        bool allFound = true;
        // Find the evidence in the list and set it to found
        foreach (var entry in evidenceList)
        {
            if (entry.name == evidenceName && !entry.found)
            {
                entry.found = true;
                Debug.Log($"{evidenceName} has been set to true.");
            }   
            // Check if any evidence is not found, set allFound to false
            if (!entry.found)
            {
                allFound = false;
            }
            
        }
        if (allFound)
        {
             StartCoroutine(WaitOliviaToTrigger());
        }
    }

    public bool CheckEvidenceState(string evidenceName)
    {
        foreach (var entry in evidenceList)
        {
            if (entry.name == evidenceName)
            {
                return entry.found;
            }
        }
        Debug.Log("Evidence not found in the list.");
        return false;
    }

    public void GameWon()
    {
        // Start separate coroutines for each NPC
        StartCoroutine(WaitAndTrigger(oliviaGameEndTrigger));
        StartCoroutine(WaitAndTrigger(jamesGameEndTrigger));
        StartCoroutine(WaitAndTrigger(markGameEndTrigger));
        StartCoroutine(WaitAndTrigger(noahGameEndTrigger));
    }

    private IEnumerator WaitAndTrigger(NarrativeDesignTrigger npcTrigger)
    {
        // Wait until the NPC is focused
        yield return new WaitUntil(() => npcTrigger.convaiNPC.IsActive == true);

        // Trigger the event
        npcTrigger.InvokeSelectedTrigger();
    }

    private IEnumerator WaitOliviaToTrigger()
    {
        // Wait until the NPC is focused
        yield return new WaitUntil(() => oliviaGameEndTrigger.convaiNPC.IsActive == true);

        // Trigger the event
        allEvidenceFoundTrigger.InvokeSelectedTrigger();
    }


}
