// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using Object = UnityEngine.Object;
//
// public class AStar {
//     private Dictionary<int, HashSet<Neighbor>> adjacency;
//     private int destination;
//
//     private NavigableSet<Step> pending = new TreeSet<>();
//
//     public Astar(Map<Integer, Set<Neighbor>> adjacency, int source, int destination) {
//         this.adjacency = adjacency;
//         this.destination = destination;
//
//         this.pending.add(new Step(source, null, 0));
//     }
//
//     public List<int> nextShortestPath() {
//         Step current = this.pending.pollFirst();
//         while( current != null) {
//             if( current.getId() == this.destination )
//                 return current.generatePath();
//             for (Neighbor neighbor : this.adjacency.get(current.id)) {
//                 if(!current.seen(neighbor.getId())) {
//                     final Step nextStep = new Step(neighbor.getId(), current, current.cost + neighbor.cost + predictCost(neighbor.id, this.destination));
//                     this.pending.add(nextStep);
//                 }
//             }
//             current = this.pending.pollFirst();
//         }
//         return null;
//     }
//
//     protected int predictCost(int source, int destination) {
//         return 0; //Behaves identical to Dijkstra's algorithm, override to make it A*
//     }
//
//     private class Step : IComparable<Step> {
//         int id;
//         Step parent;
//         int cost;
//
//         public Step(int id, Step parent, int cost) {
//             this.id = id;
//             this.parent = parent;
//             this.cost = cost;
//         }
//
//         public int getId() {
//             return id;
//         }
//
//         public Step getParent() {
//             return parent;
//         }
//
//         public int getCost() {
//             return cost;
//         }
//
//         public bool seen(int node) {
//             if(this.id == node)
//                 return true;
//             else if(parent == null)
//                 return false;
//             else
//                 return this.parent.seen(node);
//         }
//
//         public List<int> generatePath() {
//             List<int> path;
//             if(this.parent != null)
//                 path = this.parent.generatePath();
//             else
//                 path = new List<int>();
//             path.Add(this.id);
//             return path;
//         }
//
//         public bool equals(Object o) {
//             if (this.Equals(o))
//             {
//                 return true;
//             }
//
//             if (o == null || this.GetType() != o.GetType())
//             {
//                 return false;
//             }
//             Step step = (Step) o;
//             return id == step.id &&
//                 cost == step.cost &&
//                 Object.equals(parent, step.parent);
//         }
//         
//         public static int hashCode() {
//             return Objects.hash(id, parent, cost);
//         }
//
//         public int CompareTo(Step step)
//         {
//             if(step == null)
//                 return 1;
//             if( this.cost != step.cost)
//                 return int.compare(this.cost, step.cost);
//             if( this.id != step.id )
//                 return Integer.compare(this.id, step.id);
//             if( this.parent != null )
//                 this.parent.compareTo(step.parent);
//             if(step.parent == null)
//                 return 0;
//             return -1;
//         }
//     }
//
//     private class Neighbor {
//         int id;
//         int cost;
//
//         public Neighbor(int id, int cost) {
//             this.id = id;
//             this.cost = cost;
//         }
//
//         public int getId() {
//             return id;
//         }
//
//         public int getCost() {
//             return cost;
//         }
//     }
//
//     public static void main(String[] args) {
//         // Dictionary<int, HashSet<Neighbor>> adjacency = createAdjacency();
//         AStar search = new AStar(adjacency, 1, 4);
//         Debug.Log("printing all paths from shortest to longest...");
//         List<int> path = search.nextShortestPath();
//         while(path != null) {
//             Debug.Log(path);
//             path = search.nextShortestPath();
//         }
//     }
//
//     // private Dictionary<int, HashSet<Neighbor>> createAdjacency() {
//     //     Dictionary<int, HashSet<Neighbor>> adjacency = new Dictionary<int, HashSet<Neighbor>>();
//     //
//     //     //This sets up the adjacencies. In this case all adjacencies have a cost of 1, but they dont need to. Otherwise
//     //     //They are exactly the same as the example you gave in your question
//     //     addAdjacency(adjacency, 5,1,1,2,1,3,1);
//     //     addAdjacency(adjacency, 1,2,1,5,1);
//     //     addAdjacency(adjacency, 2,1,1,3,1,4,1,5,1);
//     //     addAdjacency(adjacency, 3,2,1,5,1);
//     //     addAdjacency(adjacency, 4,2,1);
//     //
//     //     return Collections.unmodifiableMap(adjacency);
//     // }
//
//     // private static void addAdjacency(Map<Integer, Set<Neighbor>> adjacency, int source, Integer... dests) {
//     //     if( dests.length % 2 != 0)
//     //         throw new IllegalArgumentException("dests must have an equal number of arguments, each pair is the id and cost for that traversal");
//     //
//     //     final Set<Neighbor> destinations = new HashSet<>();
//     //     for(int i = 0; i < dests.length; i+=2)
//     //         destinations.add(new Neighbor(dests[i], dests[i+1]));
//     //     adjacency.put(source, Collections.unmodifiableSet(destinations));
//     // }
// }
