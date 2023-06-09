using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Dungeon : MonoBehaviour
{   

    [SerializeField] int number_of_cells;
    [SerializeField] GameObject room;
    [SerializeField] GameObject player;
    [SerializeField] GameObject connector;
    [SerializeField] GameObject corridor;
    [SerializeField] GameObject mad_exit;
    [SerializeField] GameObject exit;
    [SerializeField] GameObject background;
    [SerializeField] GameObject wall;
    [SerializeField] int int_avg_distance_between_cells;
    [SerializeField] int int_minimal_cell_size;
    [SerializeField] int int_maximal_cell_size;
    [SerializeField] int int_avg_cell_x_offset;
    [SerializeField] int int_avg_cell_y_offset;
    [SerializeField] int double_density_of_edges;

    bool check_if_free = false;
    List<GameObject> list_of_cells = new List<GameObject>();
    List<GameObject> list_of_connectors_x = new List<GameObject>();
    List<GameObject> list_of_connectors_y = new List<GameObject>();
    List<GameObject> list_of_connectors = new List<GameObject>();
    SpriteRenderer spriteRenderer;

    private int x_spawn;
    private int y_spawn;
    
    void Start()
    {   
        int i,j;
        for (i = 0 ; i < 4 ; i++) {
            for (j = 0 ; j < 4 ; j++) {
                GameObject cell = Instantiate(background, new Vector3(-25+j*50, -25+i*50 ,0), Quaternion.identity);
            }
        }
        Generation();
    }
    
    //void Awake() {
        
    //    GameObject [] players;
    //    GameObject player = GameObject.FindWithTag("Player");
    //    players = GameObject.FindGameObjectsWithTag("Player");
    //    if (players.Length > 1) {
    //        if (players[0].transform.position == new Vector3(0, 0, 0)) {
    //            Destroy(players[0]);
    //            player = players[1];
    //        }
    //        else {
    //            Destroy(players[1]);
    //            player = players[0];
    //        }
    //    }
    //    DontDestroyOnLoad(player);
    //}
  
    void Generation()
    {   
        int possible_cells = Convert.ToInt32(Math.Pow(Math.Ceiling(Math.Sqrt(number_of_cells)+2),2));
        int possible_cells_in_a_row = Convert.ToInt32(Math.Sqrt(possible_cells));

        Vector3Int[] vecArray = new Vector3Int[possible_cells];
        var list_of_connections = new List<Tuple<int, int>>();
        int x_pos, y_pos;
        x_pos = y_pos = 0;
        int x_offset, y_offset;
        x_offset = y_offset = 0;
        int room_size, room_placement_id;
        room_size = room_placement_id = 0;
        double room_height_width_ratio = 1;
        int cell_height, cell_width;
        cell_height = cell_width = 0;
        int i, j, k;
        i = j = k = 0;
        GraphNode[,] graph_array = new GraphNode[number_of_cells, number_of_cells];

        for (i = 0 ; i < possible_cells_in_a_row ; i++) {
            for (j = 0 ; j < possible_cells_in_a_row ; j++) {
                vecArray[k].x = x_pos;
                vecArray[k].y = y_pos;
                vecArray[k].z = 1;
                x_pos += int_avg_distance_between_cells;
                k++;
            }
            x_pos = 0;
            y_pos += int_avg_distance_between_cells;
        } 
        for (i = 0 ; i < number_of_cells ; i++) {
            check_if_free = false;
            while(check_if_free == false) {
                room_placement_id = GetRoomId(0,possible_cells-1, vecArray);
                x_offset = Convert.ToInt32(GetRng(-int_avg_cell_x_offset,int_avg_cell_x_offset));
                y_offset = Convert.ToInt32(GetRng(-int_avg_cell_y_offset,int_avg_cell_y_offset));
            }
            room_size = Convert.ToInt32(GetRng(int_minimal_cell_size, int_maximal_cell_size));
            room_height_width_ratio = GetRng(0.5,1.5);
            cell_width = room_size;
            cell_height = Convert.ToInt32(cell_width*room_height_width_ratio);
            x_pos = vecArray[room_placement_id].x;
            y_pos = vecArray[room_placement_id].y;
            GameObject cell = Instantiate(room, new Vector3(x_pos, y_pos ,0), Quaternion.identity);
            cell.transform.position = cell.transform.position + new Vector3(x_offset, y_offset, 0);
            spriteRenderer = cell.GetComponent<SpriteRenderer>();
            spriteRenderer.size = new Vector2(cell_width-1, cell_height-1);
            list_of_cells.Add(cell);
        }
        graph_array = CreateDistanceMatrix();
        GenerateMST(graph_array, list_of_connections);
        AddEdges(graph_array, list_of_connections);
        AddSpecialCells();
        AddBoundaries();
    }

    public int GetRoomId(int minNumber, int maxNumber, Vector3Int[] vecArray)
    {
        System.Random random = new System.Random();
        int return_val = random.Next(minNumber, maxNumber);
        if (vecArray[return_val].z == 1) {
            vecArray[return_val].z = -1;
            check_if_free = true;
        }
        return return_val;
    }

    public double GetRng(double minNumber, double maxNumber)
    {
        System.Random random = new System.Random();
        double return_val = random.NextDouble() * (maxNumber - minNumber) + minNumber;
        return return_val; 
    }

    public GraphNode[,] CreateDistanceMatrix() {
        GraphNode[,] graph_array = new GraphNode[number_of_cells, number_of_cells];
        int i, j;
        float distance;
        GraphNode gn, dummy;
        dummy = new GraphNode {
            dist = 0,
            mst = false
        };
        for (i = 0 ; i < number_of_cells ; i++) {
            for (j = 0 ; j < i ; j++) {
                if (i != j) {
                    distance = GetDistance(list_of_cells[i], list_of_cells[j]);
                    //Debug.Log(i + " -> " + j + " dist " + distance);
                    gn = new GraphNode {
                        dist = distance,
                        mst = false
                    };
                    graph_array[i,j] = gn;
                    graph_array[j,i] = gn;
                }
            }
            graph_array[i,i] = dummy;
        }

        return graph_array;

    }

    public float GetDistance(GameObject first, GameObject second) {
        float distance = Vector3.Distance(first.transform.position, second.transform.position);
        return distance;
    }

    public void GenerateMST(GraphNode[,] graph_array, List<Tuple<int,int>> list_of_connections) {
        int exist;
        int i, j, minimum_id_j, minimum_id_i;
        i = j = minimum_id_i = minimum_id_j = 0;
        float minimum, distance;;
        List<int> mst_list = new List<int>();
        graph_array[0,0].mst = true;
        mst_list.Add(0);
        while (mst_list.Count < number_of_cells) {
            minimum = float.MaxValue;
            for (i = 0 ; i < graph_array.GetLength(0) ; i++) {
                exist = mst_list.IndexOf(i);
                if (exist != -1) {
                    for (j = 0 ; j < graph_array.GetLength(1) ; j++) {
                        distance = graph_array[i,j].dist;
                        if (distance < minimum && distance > 0 && graph_array[i,j].mst == false) {
                            minimum = distance;
                            minimum_id_j = j;
                            minimum_id_i = i;
                        }
                    }
                }
            }
            AddToMST(minimum_id_j, graph_array, mst_list);
            DrawConnection(minimum_id_i, minimum_id_j, false);
            list_of_connections.Add(Tuple.Create(minimum_id_i, minimum_id_j));
        }

    }

    public void AddToMST(int collumn_id, GraphNode[,] graph_array, List<int> mst_list) {
        int i, elem;
        //Debug.Log(" dodano: " + collumn_id);
        mst_list.Add(collumn_id);

        for (i = 0 ; i < mst_list.Count-1 ; i++) {
            elem = mst_list[i];
            graph_array[elem, collumn_id].mst = true;
            graph_array[collumn_id, elem].mst = true;
        }
        
    }


    public void DrawConnection(int first_id, int second_id, Boolean additional_check) {

        float x_scale, x_positive, x_negative;
        float y_scale, y_positive, y_negative;
        int i;
        float distance_x, distance_y;
        float minimum_x, minimum_y;
        double roll;

        GameObject x_corridor = Instantiate(corridor, new Vector3(list_of_cells[first_id].transform.position.x, list_of_cells[first_id].transform.position.y, list_of_cells[first_id].transform.position.z), Quaternion.identity);
        spriteRenderer = x_corridor.GetComponent<SpriteRenderer>();

        x_positive = list_of_cells[first_id].transform.position.x;
        x_negative = list_of_cells[second_id].transform.position.x;
        x_scale = x_positive - x_negative;
        spriteRenderer.size = new Vector2(Math.Abs(x_scale-1), 2);
        x_corridor.transform.position += new Vector3((x_negative - x_positive)/2, 0, 0);

        if (additional_check == true) {
            minimum_x = Int32.MaxValue;
            minimum_y = Int32.MaxValue;
            for (i = 0 ; i < list_of_connectors_x.Count ; i++) {
                distance_x = Math.Abs(list_of_connectors_x[i].transform.position.x-x_corridor.transform.position.x);
                if (distance_x < minimum_x) {
                    minimum_x = distance_x;
                }
                distance_y = Math.Abs(list_of_connectors_x[i].transform.position.y-x_corridor.transform.position.y);
                if (distance_y < minimum_y) {
                    minimum_y = distance_y;
                }
            }
            if (minimum_x < 3 && minimum_y < 10) {
                Destroy(x_corridor);
                roll = GetRng(0,6);
                if (roll > 3) {
                    return;
                }
            }
        }

        GameObject connect = Instantiate(connector, new Vector3(list_of_cells[first_id].transform.position.x+(x_negative - x_positive), list_of_cells[first_id].transform.position.y, list_of_cells[first_id].transform.position.z), Quaternion.identity);
        spriteRenderer = connect.GetComponent<SpriteRenderer>();
        spriteRenderer.size = new Vector2(2,2);

        GameObject y_corridor = Instantiate(corridor, new Vector3(list_of_cells[first_id].transform.position.x+(x_negative - x_positive), list_of_cells[first_id].transform.position.y, list_of_cells[first_id].transform.position.z), Quaternion.identity);
        spriteRenderer = y_corridor.GetComponent<SpriteRenderer>();

        y_positive = list_of_cells[first_id].transform.position.y;
        y_negative = list_of_cells[second_id].transform.position.y;
        y_scale = y_positive - y_negative;
        spriteRenderer.size = new Vector2(2,Math.Abs(y_scale-1));
        y_corridor.transform.position += new Vector3(0, (y_negative - y_positive)/2, 0);

        if (additional_check == true) {
            minimum_x = Int32.MaxValue;
            minimum_y = Int32.MaxValue;
            for (i = 0 ; i < list_of_connectors_y.Count ; i++) {
                distance_x = Math.Abs(list_of_connectors_y[i].transform.position.x-y_corridor.transform.position.x);
                if (distance_x < minimum_x) {
                    minimum_x = distance_x;
                }
                distance_y = Math.Abs(list_of_connectors_y[i].transform.position.y-y_corridor.transform.position.y);
                if (distance_y < minimum_y) {
                    minimum_y = distance_y;
                }
            }
            if (minimum_x < 10 && minimum_y < 2.5) {
                Destroy(y_corridor);
                Destroy(connect);
                Destroy(x_corridor);
                return;
            }
        }
        if (list_of_connectors_x != null) {
            list_of_connectors_x.Add(x_corridor);
        }
        if (list_of_connectors_y != null) {
            list_of_connectors_y.Add(y_corridor);
        }
        if (list_of_connectors != null) {
            list_of_connectors.Add(connect);
        }

    }

    public void AddEdges(GraphNode[,] graph_array, List<Tuple<int,int>> list_of_connections) {
        int i,j;
        double random, threshold, minimum, maximum;
        Tuple<int,int> comparator;
        for (i = 0 ; i < graph_array.GetLength(0) ; i++) {
            for (j = 0 ; j < graph_array.GetLength(1) ; j++) {
                threshold = GetRng(0.0, 10.0);
                random = GetRng(double_density_of_edges, 10.0);
                minimum = GetRng(4*int_avg_distance_between_cells/3, 2*int_avg_distance_between_cells);
                maximum = GetRng(7*int_avg_distance_between_cells/3, 4*int_avg_distance_between_cells);
                comparator = Tuple.Create(i,j);
                if (list_of_connections.IndexOf(Tuple.Create(i,j)) == -1) {
                    if (graph_array[i,j].dist > minimum && graph_array[i,j].dist < maximum && random > threshold) {
                        DrawConnection(j, i, true);
                        list_of_connections.Add(comparator);
                    }
                }
            }
        }
    }

    public void AddSpecialCells() {
        int exit_id, mad_exit_id, spawn_cell_id;
        int [] special_cells_ids = new int [] {Int32.MaxValue-2, Int32.MaxValue-1, Int32.MaxValue};
        int i = 0;
        int comparate_to = list_of_cells.Count;
        while (true) {
            special_cells_ids[i] = Convert.ToInt32(GetRng(0, comparate_to));
            if (special_cells_ids[i] == comparate_to) {
                special_cells_ids[i]--;
                if (special_cells_ids[0] != special_cells_ids[1] && special_cells_ids[0] != special_cells_ids[2] && special_cells_ids[1] != special_cells_ids[2])
                    i++;
            }
            else {
                if (special_cells_ids[0] != special_cells_ids[1] && special_cells_ids[0] != special_cells_ids[2] && special_cells_ids[1] != special_cells_ids[2])
                    i++;
            }
            if (i == 3) {
                exit_id = special_cells_ids[0];
                mad_exit_id = special_cells_ids[1];
                spawn_cell_id = special_cells_ids[2];
                break;
            }
        }
        //Debug.Log(exit_id + " " + mad_exit_id + " " + spawn_cell_id);
        int x_spawn = Convert.ToInt32(list_of_cells[spawn_cell_id].transform.position.x);
        int y_spawn = Convert.ToInt32(list_of_cells[spawn_cell_id].transform.position.y);
        int x_mad_exit = Convert.ToInt32(list_of_cells[mad_exit_id].transform.position.x);
        int y_mad_exit = Convert.ToInt32(list_of_cells[mad_exit_id].transform.position.y);
        int x_exit = Convert.ToInt32(list_of_cells[exit_id].transform.position.x);
        int y_exit = Convert.ToInt32(list_of_cells[exit_id].transform.position.y);
        GameObject ent = Instantiate(mad_exit, new Vector3(x_mad_exit, y_mad_exit, 0), Quaternion.identity);
        GameObject ext = Instantiate(exit, new Vector3(x_exit, y_exit, 0), Quaternion.identity);
        GameObject pl = Instantiate(player, new Vector3(x_spawn, y_spawn, 0), Quaternion.identity);

        //GameObject pl = GameObject.FindWithTag("Player");
        
        //pl.tag = "Player";
        //Debug.Log("przed spawn" + pl.transform.position.x + " " + pl.transform.position.y);
        //Debug.Log("-" + -pl.transform.position.x + " " + -pl.transform.position.y);
        //Debug.Log("spawn" + x_spawn + " " + y_spawn);
        //pl.transform.position += new Vector3(x_spawn, y_spawn, 0);
        //Debug.Log("po spawn" + pl.transform.position.x + " " + pl.transform.position.y);
    }

    public void AddBoundaries() {
        int i,j;
        Vector2 room_size;
        Vector3 position;
        int vector_size = list_of_cells.Count;
        for (i = 0 ; i < vector_size ; i++) {
            GameObject cell = list_of_cells[i];
            position = cell.transform.position;
            spriteRenderer = cell.GetComponent<SpriteRenderer>();
            room_size = spriteRenderer.size;
            for (j = 0 ; j < room_size.x+2 ; j++) {
                Instantiate(wall, new Vector3(position.x-(room_size.x+1)/2+j, position.y+(room_size.y+1)/2 ,0), Quaternion.identity);
                Instantiate(wall, new Vector3(position.x-(room_size.x+1)/2+j, position.y-(room_size.y+1)/2 ,0), Quaternion.identity);
            }
            for (j = 1 ; j < room_size.y+1 ; j++) {
                Instantiate(wall, new Vector3(position.x+(room_size.x+1)/2, position.y-(room_size.y+1)/2+j ,0), Quaternion.identity);
                Instantiate(wall, new Vector3(position.x-(room_size.x+1)/2, position.y-(room_size.y+1)/2+j ,0), Quaternion.identity);
            }
        }
        vector_size = list_of_connectors_x.Count;
        for (i = 0 ; i < vector_size ; i++) {
            GameObject corridor_x = list_of_connectors_x[i];
            position = corridor_x.transform.position;
            spriteRenderer = corridor_x.GetComponent<SpriteRenderer>();
            room_size = spriteRenderer.size;
            for (j = 0 ; j < room_size.x+2 ; j++) {
                Instantiate(wall, new Vector3(position.x-(room_size.x+1)/2+j, position.y+1.5F ,0), Quaternion.identity);
                Instantiate(wall, new Vector3(position.x-(room_size.x+1)/2+j, position.y-1.5F ,0), Quaternion.identity);
            }
        }
        vector_size = list_of_connectors_x.Count;
        for (i = 0 ; i < vector_size ; i++) {
            GameObject corridor_y = list_of_connectors_y[i];
            position = corridor_y.transform.position;
            spriteRenderer = corridor_y.GetComponent<SpriteRenderer>();
            room_size = spriteRenderer.size;
            for (j = 1 ; j < room_size.y+1 ; j++) {
                Instantiate(wall, new Vector3(position.x+1.5F, position.y-(room_size.y+1)/2+j ,0), Quaternion.identity);
                Instantiate(wall, new Vector3(position.x-1.5F, position.y-(room_size.y+1)/2+j ,0), Quaternion.identity);
            }
        }
        vector_size = list_of_connectors.Count;
        for (i = 0 ; i < vector_size ; i++) {
            GameObject corridor = list_of_connectors[i];
            position = corridor.transform.position;
            for (j = 0 ; j < 4 ; j++) {
                Instantiate(wall, new Vector3(position.x-1.5F+j, position.y+1.5F), Quaternion.identity);
                Instantiate(wall, new Vector3(position.x-1.5F+j, position.y-1.5F), Quaternion.identity);
                Instantiate(wall, new Vector3(position.x-1.5F, position.y-1.5F+j), Quaternion.identity);
                Instantiate(wall, new Vector3(position.x+1.5F, position.y-1.5F+j), Quaternion.identity);
            }
        }

    }  

}
