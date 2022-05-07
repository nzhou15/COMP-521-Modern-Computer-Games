using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.AI;

public class SquirrelAI : MonoBehaviour
{
    // world states
    int has_nuts;
    bool see_nuts;   
    int has_garbage;
    bool see_garbage_can;
    bool see_player;
    bool store_food;
    bool at_home_tree;

    bool is_done;

    public NavMeshAgent agent;
    Transform homeTree;
    float visual_range = 5f;
    Transform target;
    bool reach_target;
    Transform closest_tree;
    Transform second_closest_tree;
    List<GameObject> list_nuts = new List<GameObject>();
    List<GameObject> list_garbages = new List<GameObject>();

    public HashSet<string> actions = new HashSet<string>();

    // Start is called before the first frame update
    void Start()
    {
        has_nuts = 0;
        see_nuts = false;   
        has_garbage = 0;
        see_garbage_can = false;
        see_player = false;
        store_food = false;
        at_home_tree = false;
        is_done = false;

        homeTree = gameObject.transform.parent;
        reach_target = false;
    
        CreaetActions();
    }

    void CreaetActions()
    {
        actions.Add("Roaming");
        actions.Add("Exploring");
        actions.Add("PickUpNuts");
        actions.Add("PickUpGarbage");
        actions.Add("ReturnHomeTree");
        actions.Add("RunAway");
    }

    void Update() {
        InVisualRange();

        // if a nuts in the memory is destroyed then remove it from the list
        foreach (GameObject nut in list_nuts)
        {
            if(nut == null)
            {
                list_nuts.Remove(nut);
            }
        }
    }

    void InVisualRange()
    {
        Collider[] colliders = Physics.OverlapSphere(gameObject.transform.position, visual_range);
        foreach (Collider c in colliders)
        {   
            if(list_nuts.Count == 0)
            {
                see_nuts = false;
            }

            if(list_garbages.Count == 0)
            {
                see_garbage_can = false;
            }

            see_player = false;

            if(c.transform.name == "Nut(Clone)")
            {   
                // print("see_nuts");
                see_nuts = true;
                
                if(list_nuts.Count >= 5)
                {
                    list_nuts.RemoveAt(0);
                }
                list_nuts.Add(c.gameObject);
            }  

            if(c.transform.name == "Garbage Can(Clone)")
            {   
                see_garbage_can = true;

                if(list_garbages.Count >= 2)
                {
                    list_garbages.RemoveAt(0);  
                }
                list_garbages.Add(c.gameObject);
            }

            if(c.transform.name == "Player")
            {   
                see_player = true;
             }        
        }
        closest_tree = FindClosestTree();
    }

    // idle behaviour 1: roaming around the home tree
    public IEnumerator Roaming()
    {
        is_done = false;
        at_home_tree = true;
        // worldState[6] = new KeyValuePair<string, object>("at_home_tree", true);

        Vector3 rand = Random.insideUnitSphere * 3f;
        rand += homeTree.position;
        
        NavMeshHit hit;
        NavMesh.SamplePosition(rand, out hit, 2f, 1);
        agent.SetDestination(hit.position);

        yield return new WaitForSeconds(2);
        
        print("roaming is done");
        is_done = true;
    }

    public bool isDone()
    {
        return is_done;
    }

    // idle behaviour 2: heading to a random tree
    public IEnumerator Exploring()
    {
        print(see_nuts);

        is_done = false;
        at_home_tree = false;

        Transform tree = GameObject.Find("Trees").transform.GetChild(Random.Range(0, 10));
        agent.SetDestination(tree.position);
        target = tree;

        yield return new WaitUntil(() => reach_target == true);
        reach_target = false;

        is_done = true;
        print("exploring is done");
    }

    // a function that picks up nuts
    // pre-condition: see_nuts = true, has_nuts < 3
    public IEnumerator PickUpNuts()
    {
        is_done = false;
        at_home_tree = false;
        // worldState[6] = new KeyValuePair<string, object>("at_home_tree", false);

        GameObject nut = list_nuts[0];
        list_nuts.RemoveAt(0);

        agent.SetDestination(nut.transform.position);
        target = nut.transform;

        yield return new WaitUntil(() => reach_target == true);
        reach_target = false;

        // nut.transform.position = gameObject.transform.position + new Vector3(0f, 2f, 0f);
        // nut.transform.parent = gameObject.transform;

        has_nuts++;
        print("nuts++");

        is_done = true;
        print("pick up nuts is done");
    }

    public bool CheckPickUpNuts()
    {
        return see_nuts == true && has_nuts < 3;
    }

   
    // a function that picks up garbage
    // pre-condition: see_garbage_can = true, has_garbage < 1, has_nuts < 1
    public IEnumerator PickUpGarbage()
    {
        is_done = false;
        at_home_tree = false;
        // worldState[6] = new KeyValuePair<string, object>("at_home_tree", false);

        GameObject o = list_garbages[0];
        list_garbages.RemoveAt(0);

        agent.SetDestination(o.transform.position);
        target = o.transform;

        yield return new WaitUntil(() => reach_target == true);
        reach_target = false;
        
        GarbageCanBehaviour garbage_can = o.GetComponent<GarbageCanBehaviour>();
        if(garbage_can.state == GarbageCanBehaviour.State.Full)
        {
            garbage_can.transform.GetChild(0).transform.position = gameObject.transform.position + new Vector3(0f, 0.7f, 0f);
            garbage_can.transform.GetChild(0).parent = gameObject.transform;

            has_garbage++;
            
            garbage_can.state = GarbageCanBehaviour.State.Empty;
        }
        else
        {
            yield return new WaitForSeconds(2);
        }
        is_done = true;
        print("pick up garbage is done");
    }

    public bool CheckPickUpGarbage()
    {
        return see_garbage_can == true && has_nuts < 1 && has_garbage < 1;
    }

    // pre-condition: at_home_tree = false
    public IEnumerator ReturnHomeTree()
    {   
        is_done = false;

        agent.SetDestination(homeTree.position);
        target = homeTree;

        yield return new WaitUntil(() => reach_target == true);
        reach_target = false;

        at_home_tree = true;

        StoreFood();
    }

    // a function that stores food
    // pre-condition: at_home_tree = true, has_nuts > 0 or has_garbage > 0
    public void StoreFood()
    {
        print("--> storing food");

        Vector3 pos = homeTree.position + new Vector3(1f, 0f, 1f);
        for(int i = 0; i < gameObject.transform.childCount; i++)
        {
            Transform c = gameObject.transform.GetChild(i);
            if(c.name == "Nut(Clone)")
            {
                c.position = pos;
            }
        }
        has_nuts = 0;
        has_garbage = 0;       
        store_food = true;

        is_done = true;
        print("store food is done");
    }

    public bool CheckReturnHome()
    {
        return at_home_tree == false && (has_nuts > 0 || has_garbage > 0);
    }

    public bool CheckStoreFood()
    {
        return store_food;
    }

    // squirrels run away if they see the player comes close
    // pre-condition: see_player = true
    public IEnumerator RunAway()
    {   
        is_done = false;

        Collider[] colliders = Physics.OverlapSphere(closest_tree.position + new Vector3(0f, 4f, 0f), 1f);
        
        // no more than one squirrel can be in a given tree at the same time
        foreach (Collider c in colliders)
        {   
            if(c.transform.name == "Squirrel(Clone)")
            {   
                // seeks the second closest tree for refuge
                closest_tree = second_closest_tree;
            }    
        }

        agent.SetDestination(closest_tree.position);
        target = closest_tree;
        
        yield return new WaitUntil(() => reach_target == true);
        reach_target = false;

        // climbs the tree and hides unitl the player is gone
        gameObject.transform.position = closest_tree.position + new Vector3(0f, 4f, 0f);
        yield return new WaitUntil(() => see_player == false);

        // climbs off the tree
        gameObject.transform.position = closest_tree.position + new Vector3(0f, 0f, 0.5f);

        if(closest_tree == homeTree)
        {
            at_home_tree = true;
        }
        is_done = true;
        print("run away is done");
    }

    public bool CheckRunAway()
    {
        return see_player == true;
    }

    // return the current world states
    public List<KeyValuePair<string,object>> GetWorldStates()
    {
        List<KeyValuePair<string,object>> state = new List<KeyValuePair<string,object>>();
        state.Add(new KeyValuePair<string, object>("has_nuts", has_nuts));
        state.Add(new KeyValuePair<string, object>("see_nuts", see_nuts));
        state.Add(new KeyValuePair<string, object>("has_garbage", has_garbage));
        state.Add(new KeyValuePair<string, object>("see_garbage_can", see_garbage_can));
        state.Add(new KeyValuePair<string, object>("see_player", see_player));
        state.Add(new KeyValuePair<string, object>("store_food", store_food));
        state.Add(new KeyValuePair<string, object>("at_home_tree", at_home_tree));
        return state;
    }

    Transform FindClosestTree()
    {
        Vector3 pos = gameObject.transform.position;
        float min = Mathf.Infinity;
        Transform closest = homeTree;

        GameObject all_trees = GameObject.Find("Trees");
        for(int i = 0; i < all_trees.transform.childCount; i++)
        {   
            float dist = Vector3.Distance(pos, all_trees.transform.GetChild(i).position);
            if(dist < min)
            {
                min = dist;
                second_closest_tree = closest;
                closest = all_trees.transform.GetChild(i);
            }
        }
        return closest;
    }

    void OnCollisionEnter(Collision other) {     
        if(target)
        {
            if(other.transform == target)
            {
                reach_target = true;
            }
            else
            {
                reach_target = false;
            }
        }       
    }
}
