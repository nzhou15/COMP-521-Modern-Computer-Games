using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GOAPPlanner : MonoBehaviour
{
    SquirrelAI agent;
    HashSet<string> actions = new HashSet<string>();
    
    bool PickUpNuts_condition;
    bool PickUpGarbage_condition;
    bool ReturnHomeTree_condition;
    bool RunAway_condition;

    bool first_action;
    bool first_plan;

    Queue<string> plan;

    // Start is called before the first frame update
    void Start()
    {
        agent = gameObject.transform.GetComponent<SquirrelAI>();
        actions = agent.actions;

        first_action = false;
        first_plan = true;
    }

    // Update is called once per frame
    void Update()
    {   
        // if the world states change then replanning
        if(PickUpNuts_condition != agent.CheckPickUpNuts() || PickUpGarbage_condition != agent.CheckPickUpGarbage() ||
        ReturnHomeTree_condition != agent.CheckReturnHome() || RunAway_condition != agent.CheckRunAway())
        {
            if(GOAP_plan() != null)
            {
                plan = GOAP_plan();
                string txt = "plan: ";
                foreach (string p in plan)
                {
                    txt += p + " ";
                }
                txt += '\n';
                gameObject.transform.GetComponent<TextMesh>().text = txt;
                print("--> replanning " + txt);

                // record the current states
                PickUpNuts_condition = agent.CheckPickUpNuts();
                PickUpGarbage_condition = agent.CheckPickUpGarbage();
                ReturnHomeTree_condition = agent.CheckReturnHome();
                RunAway_condition = agent.CheckRunAway();
            }
        }

        if(GOAP_plan() != null && first_plan)
        {
            plan = GOAP_plan();
            first_plan = false;
            first_action = true;

            string txt = "plan: ";
            foreach (string p in plan)
            {
                txt += p + " ";
            }
            txt += '\n';
            gameObject.transform.GetComponent<TextMesh>().text = txt;
            print(txt);

            // record the current states
            PickUpNuts_condition = agent.CheckPickUpNuts();
            PickUpGarbage_condition = agent.CheckPickUpGarbage();
            ReturnHomeTree_condition = agent.CheckReturnHome();
            RunAway_condition = agent.CheckRunAway();
        }
        
        // wait until the current action is done then start the next action
        if(agent.isDone() || first_action)
        {
            first_action = false;
            
            string cur_action = plan.Dequeue();
            gameObject.transform.GetComponent<TextMesh>().text += "current action: " + cur_action;
            print("current action: " + cur_action);

            if(cur_action == "Roaming")
            {
                print("--> roaming");
                StartCoroutine(agent.Roaming());
            }

            if(cur_action == "Exploring")
            {
                print("--> exploring");
                StartCoroutine(agent.Exploring());
            }

            if(cur_action == "PickUpNuts")
            {
                print("--> picking up nuts");
                StartCoroutine(agent.PickUpNuts());
            }

            if(cur_action == "PickUpGarbage")
            {
                print("--> picking up garbages");
                StartCoroutine(agent.PickUpGarbage());
            }

            if(cur_action == "ReturnHomeTree")
            {
                print("--> returning home");
                StartCoroutine(agent.ReturnHomeTree());
            }

            if(cur_action == "RunAway")
            {
                print("--> running away from the player");
                StartCoroutine(agent.RunAway());
            }

            // if there is no next plan then replanning
            if(plan.Count == 0)
            {
                plan = GOAP_plan();
            }
        }
    }

    public Queue<string> GOAP_plan() 
	{
		// check what actions can run using their checkProceduralPrecondition
		HashSet<string> usableActions = new HashSet<string>();
        usableActions = actions;

		// build up the tree and record the leaf nodes that provide a solution to the goal.
        List<Node> leaves = new List<Node>();

		// build graph
		Node start = new Node (null, agent.GetWorldStates(), null);
		bool success = buildGraph(start, leaves, usableActions);

		if (!success) {
			Debug.Log("NO PLAN");
			return null;
		}

		// gets the node and work back through the parents
		List<string> result = new List<string> ();
		Node n = leaves[Random.Range(0, result.Count)];
		while (n != null) {
			if (n.action != null) {
                // inserts the action in the front
				result.Insert(0, n.action); 
			}
			n = n.parent;
		}

		// stores actions list in correct order
		Queue<string> queue = new Queue<string> ();
		foreach (string a in result) {
			queue.Enqueue(a);
		}

        if(queue.Count == 1)
        {
            queue.Enqueue("PickUpNuts");
            queue.Enqueue("ReturnHomeTree");
        }

		return queue;
	}

	// a function that returns true if at least one solution was found
	private bool buildGraph (Node parent, List<Node> leaves, HashSet<string> usableActions)
	{
		bool found = false;

		// go through each action available at this node and see if we can use it here
		foreach (string action in usableActions) {
			if ( inState(action, parent.state) ) {                
				List<KeyValuePair<string,object>> currentState = populateState(parent.state, action);

				Node node = new Node(parent, currentState, action);

                // found a solution if squirrel successfully gathers food
				if (inState("ReturnHomeTree", currentState)) {
					leaves.Add(node);
					found = true;
                    // print("found a solution!");

				} else {
					// tests all the remaining actions and branch out the tree if there is no  solution yet
					HashSet<string> subset = createSubset(usableActions, action);
					bool b = buildGraph(node, leaves, subset);
					if (b)
						found = true;
				}
			}
		}
		return found;
	}

	// removes the action toRemove to create a subset of actions
	HashSet<string> createSubset(HashSet<string> actions, string toRemove) {
		HashSet<string> subset = new HashSet<string> ();
		foreach (string a in actions) {
			if (!a.Equals(toRemove))
			{
                subset.Add(a);
            }
		}
		return subset;
	}

	// checks if the states satisfy pre-conditions
	bool inState(string s, List<KeyValuePair<string,object>> state) {
        if(s == "PickUpNuts")
        {   
            bool b = (bool)state[1].Value == true && (int)state[0].Value < 3;     
            return agent.CheckPickUpNuts() == b;
        } 
        if(s == "PickUpGarbage") 
        {
            bool b = (bool)state[3].Value == true && (int)state[2].Value < 1;
            return agent.CheckPickUpGarbage() == b;
        }
        if(s == "ReturnHomeTree")
        {
            bool b = ((int)state[0].Value > 0 || (int)state[2].Value > 0) && (bool)state[5].Value == false;
            return agent.CheckReturnHome() == b;
        }
        if(s == "RunAway")
        {
            bool b = (bool)state[4].Value == true;
            return agent.CheckRunAway() == b;
        }
        return true;
	}
	
	// applies post-conditions to the currentState
	List<KeyValuePair<string,object>> populateState(List<KeyValuePair<string,object>> currentState, string s) {
		if(s == "PickUpNuts")
        {   
            int n = (int)currentState[0].Value + 1;
            currentState[0] = new KeyValuePair<string,object>("has_nuts", n);
        } 
        if(s == "PickUpGarbage") 
        {
            int n = (int)currentState[2].Value + 1;
            currentState[2] = new KeyValuePair<string,object>("has_garbage", n);
        }
        if(s == "ReturnHomeTree")
        {
            currentState[0] = new KeyValuePair<string,object>("has_nuts", 0);
            currentState[2] = new KeyValuePair<string,object>("has_garbage", 0);
            currentState[5] = new KeyValuePair<string,object>("store_food", true);
            currentState[6] = new KeyValuePair<string,object>("at_home_tree", true);
        }
        if(s == "RunAway")
        {
            currentState[4] = new KeyValuePair<string,object>("see_player", false);
        }
        return currentState;
	}
	
	// a class of Node for building up the graph and holding the running costs of actions.
	class Node {
		public Node parent;
		public List<KeyValuePair<string,object>> state;
		public string action;

		public Node(Node parent, List<KeyValuePair<string,object>> state, string action) {
			this.parent = parent;
			this.state = state;
			this.action = action;
		}
	}

    // checks pre-conditions for each action
    bool checkProceduralPrecondition(string s)
    {
        if(s == "PickUpNuts") return agent.CheckPickUpNuts();
        if(s == "PickUpGarbage") return agent.CheckPickUpGarbage();
        if(s == "ReturnHomeTree") return agent.CheckReturnHome();
        if(s == "RunAway") return agent.CheckRunAway();
        return true;
    }

    /** Decision Tree Implementation **/
    // // a function that makes plan to achieve goal and returns a queue of actions
    // public Queue<string> Plan() 
	// {   
    //     PickUpNuts_condition = agent.CheckPickUpNuts();
    //     PickUpGarbage_condition = agent.CheckPickUpGarbage();
    //     ReturnHomeTree_condition = agent.CheckReturnHome();
    //     RunAway_condition = agent.CheckRunAway();

    //     Queue<string> queue = new Queue<string>();
    //     foreach (string s in actions)
    //     {
    //         if(s == "PickUpNuts" && agent.CheckPickUpNuts())
    //         {
    //             queue.Enqueue(s);
    //         }

    //         if(s == "PickUpGarbage" && agent.CheckPickUpGarbage())
    //         {
    //             queue.Enqueue(s);
    //         }

    //         if(s == "ReturnHomeTree" && agent.CheckReturnHome())
    //         {
    //             queue.Enqueue(s);
    //         }

    //         if(s == "RunAway" && agent.CheckRunAway()) 
    //         {
    //             queue.Enqueue(s);
    //         }
    //     }

    //     if(queue.Count == 0)
    //     {
    //         queue.Enqueue("Roaming");
    //         queue.Enqueue("Exploring");
    //     }
    //     return queue;
    // }
}
