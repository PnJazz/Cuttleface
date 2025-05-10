using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using static Squid_Pattern_itself;

// ===========================================================================
// CuttleFace Interactive System ¨C Unity(C#) Module - v0.9
// Author: Dingbang Qi
// Version: v0.9 (May 11th, 2025)
// License: GNU General Public License v3.0 (GPLv3)
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

// Description:
// This script is part of the Cuttleface system, an interactive system designed
// to enhance public engagement in a public exhibition setting. It provides
// real-time computer vision processing, TCP communication with Unity,
// and input logic for interaction control.
//
// For academic citation or inquiries, contact: [dq2166@columbia.edu]
// ===========================================================================


public class Squid_Pattern_itself : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public GameObject prefab_Pigment;
    
    public int gridiCount = 20;
    public int gridjCount = 20;
    public float stride = 5.0f;

    public float black_pigment_original_Scale_Factor = 1.0f;
    public float red_pigment_original_Scale_Factor = 0.5f;
    public float yellow_pigment_original_Scale_Factor = 0.25f;

    private Dictionary<(int i, int j), Vector3> pigment_Base_Grid = new Dictionary<(int i, int j), Vector3> ();

    private Dictionary<(int i, int j), GameObject> black_Pigment_Grid = new Dictionary<(int i, int j), GameObject>();
    private Dictionary<(int i, int j), GameObject> red_Pigment_Grid = new Dictionary<(int i, int j), GameObject>();
    private Dictionary<(int i, int j), GameObject> yellow_Pigment_Grid = new Dictionary<(int i, int j), GameObject>();

    private Dictionary<(int i, int j), PigmentBehavior> black_Pigment_Behavior = new Dictionary<(int i, int j), PigmentBehavior>();
    private Dictionary<(int i, int j), PigmentBehavior> red_Pigment_Behavior = new Dictionary<(int i, int j), PigmentBehavior>();
    private Dictionary<(int i, int j), PigmentBehavior> yellow_Pigment_Behavior = new Dictionary<(int i, int j), PigmentBehavior>();

    private Server_Client Server_Client; 

    private ColorManagement color_manager = new ColorManagement();

    public float realtime_scale_factor_black;
    public float realtime_scale_factor_red;
    public float realtime_scale_factor_yellow;

    void Start()
    {
        UnityEngine.Color costumized_black = color_manager.Black_Pigment_Color();
        UnityEngine.Color costumized_red = color_manager.Red_Pigment_Color();
        UnityEngine.Color costumized_yellow = color_manager.Yellow_Pigment_Color();

        pigment_Base_Grid = GridGeneration(gridiCount, gridjCount, stride);
        
        black_Pigment_Grid = BlackPigmentGrid(gridiCount, gridjCount, prefab_Pigment, pigment_Base_Grid, black_pigment_original_Scale_Factor, costumized_black);
        red_Pigment_Grid = RedPigmentGrid(gridiCount, gridjCount, prefab_Pigment, pigment_Base_Grid, red_pigment_original_Scale_Factor, costumized_red);
        yellow_Pigment_Grid = YellowPigmentGrid(gridiCount, gridjCount, prefab_Pigment, pigment_Base_Grid, yellow_pigment_original_Scale_Factor, costumized_yellow);

        Server_Client = GetComponent<Server_Client>();

    }

    Vector3 PtsMidPt(Vector3 p1, Vector3 p2)
    {
        Vector3 midPt = (p1 + p2) / 2;
        return midPt; 
    }


    Dictionary<(int i, int j), Vector3> GridGeneration(int grid_i_count, int grid_j_count, float stride)
    {
        Dictionary<(int i, int j), Vector3> pointGrid = new Dictionary<(int i, int j), Vector3>();
        for (int i = 0; i < grid_i_count; i++)
        {
            for (int j = 0; j < grid_j_count; j++)
            {
                float x = i* stride ;
                float y = 0.0f;
                float z = (float)Math.Sqrt(3.0) * j* stride;

                Vector3 gridPt = new Vector3(x, y, z); 

                pointGrid[(i, j)] = gridPt;
                
            }
        }
        return pointGrid;
    }

    Dictionary<(int i, int j), GameObject> BlackPigmentGrid(int grid_i_count, int grid_j_count, GameObject obj, Dictionary<(int i, int j), Vector3> pointGrid, float black_pigment_original_Scale_Factor, UnityEngine.Color black_pigment_color)
    {
        Dictionary<(int i, int j), GameObject> black_Pigment_Grid = new Dictionary<(int i, int j), GameObject>();
        
        Quaternion quaternion_of_Pigment = new Quaternion(0f,0f,0f,0f);

        for (int i = -3; i < grid_i_count; i+=4)
        {
            for (int j = -1; j < grid_j_count; j+=2)
            {
                if (i > -3 && j > -1)
                {
                    GameObject newObj = GameObject.Instantiate(obj, pointGrid[(i, j)], Quaternion.identity);
                    
                    newObj.transform.localScale = Vector3.one * black_pigment_original_Scale_Factor;

                    MeshRenderer meshRendererforBlackPigment = newObj.GetComponentInChildren<MeshRenderer>();
                    
                    if(meshRendererforBlackPigment != null)
                    {
                        meshRendererforBlackPigment.material.color = black_pigment_color;
                    }

                    black_Pigment_Grid[(i, j)] = newObj.gameObject;
                    newObj.name = "Black_Pigment" + "_" + i.ToString() + "_" + j.ToString();

                    black_Pigment_Behavior[(i, j)] = new PigmentBehavior();
                }
            }
        }
        return black_Pigment_Grid;
    }

    Dictionary<(int i, int j), GameObject> RedPigmentGrid(int grid_i_count, int grid_j_count, GameObject obj, Dictionary<(int i, int j), Vector3> pointGrid, float red_pigment_original_Scale_Factor, UnityEngine.Color red_pigment_color)
    {
        Dictionary<(int i, int j), GameObject> red_pigment_Grid = new Dictionary<(int i, int j), GameObject>();
        Quaternion quaternion_of_Pigment = new Quaternion(0f, 0f, 0f, 0f);
       

        for (int i = 0; i < grid_i_count; i += 2)
        {
            for (int j = 0; j < grid_j_count; j += 2)
            {
                if (i > 0)
                {
                    GameObject newObj1 = GameObject.Instantiate(obj, pointGrid[(i, j)], Quaternion.identity);
                    MeshRenderer meshRendererforRedPigment1 = newObj1.GetComponentInChildren<MeshRenderer>();
                    if (meshRendererforRedPigment1 != null)
                    {
                        meshRendererforRedPigment1.material.color = red_pigment_color;
                    }
                    newObj1.transform.localScale = Vector3.one * red_pigment_original_Scale_Factor;
                    red_pigment_Grid[(i, j)] = newObj1;
                    newObj1.name = "Red_Pigment" + "_" + i.ToString() + "_" + j.ToString();
                }
            }

        }

        for (int i = -1; i < grid_i_count; i += 4)
        {
            for (int j = 1; j < grid_j_count; j += 2)
            {
                if (i > 0)
                {
                    GameObject newObj1 = GameObject.Instantiate(obj, pointGrid[(i, j)], Quaternion.identity);
                    MeshRenderer meshRendererforRedPigment1 = newObj1.GetComponentInChildren<MeshRenderer>();
                    if (meshRendererforRedPigment1 != null)
                    {
                        meshRendererforRedPigment1.material.color = red_pigment_color;
                    }
                    newObj1.transform.localScale = Vector3.one * red_pigment_original_Scale_Factor;
                    red_pigment_Grid[(i, j)] = newObj1;
                    newObj1.name = "Red_Pigment" + "_" + i.ToString() + "_" + j.ToString();
                }
            }

        }

        return red_pigment_Grid;
    }

    Dictionary<(int i, int j), GameObject> YellowPigmentGrid(int grid_i_count, int grid_j_count, GameObject obj, Dictionary<(int i, int j), Vector3> pointGrid, float yellow_pigment_original_Scale_Factor, UnityEngine.Color yellow_pigment_color)
    {
        Dictionary<(int i, int j), GameObject> yellow_Pigment_Grid = new Dictionary<(int i, int j), GameObject>();
        Quaternion quaternion_of_Pigment = new Quaternion(0f, 0f, 0f, 0f);
        float scaleFactor = 0.25f;

        for (int i = -3; i < grid_i_count; i += 4)
        {
            for (int j = -2; j < grid_j_count; j += 2)
            {
                if (i - 2 > 0 && j > -2 && i + 2 < grid_i_count && j + 1 < grid_j_count)
                {
                    Vector3 Position2 = PtsMidPt(pointGrid[(i - 2, j + 1)], pointGrid[(i - 1, j)]);
                    Vector3 Position3 = PtsMidPt(pointGrid[(i + 2, j + 1)], pointGrid[(i + 1, j)]);

                    GameObject newObj_Odd_Even1 = GameObject.Instantiate(obj, pointGrid[(i, j)], Quaternion.identity);
                    GameObject newObj_Odd_Even2 = GameObject.Instantiate(obj, Position2, Quaternion.identity);
                    GameObject newObj_Odd_Even3 = GameObject.Instantiate(obj, Position3, Quaternion.identity);

                    newObj_Odd_Even1.transform.localScale = Vector3.one * yellow_pigment_original_Scale_Factor;
                    newObj_Odd_Even2.transform.localScale = Vector3.one * yellow_pigment_original_Scale_Factor;
                    newObj_Odd_Even3.transform.localScale = Vector3.one * yellow_pigment_original_Scale_Factor;

                    newObj_Odd_Even1.name = "Yellow_Pigment_Down_Middle" + "_" + i.ToString() + "_" + j.ToString();
                    newObj_Odd_Even2.name = "Yellow_Pigment_Down_Left" + "_" + i.ToString() + "_" + j.ToString();
                    newObj_Odd_Even3.name = "Yellow_Pigment_Down_Right" + "_" + i.ToString() + "_" + j.ToString();

                    MeshRenderer meshRendererforYellowPigment1 = newObj_Odd_Even1.GetComponentInChildren<MeshRenderer>();
                    MeshRenderer meshRendererforYellowPigment2 = newObj_Odd_Even2.GetComponentInChildren<MeshRenderer>();
                    MeshRenderer meshRendererforYellowPigment3 = newObj_Odd_Even3.GetComponentInChildren<MeshRenderer>();

                    if (meshRendererforYellowPigment1 != null && meshRendererforYellowPigment2 != null && meshRendererforYellowPigment3 != null)
                    {
                        meshRendererforYellowPigment1.material.color = yellow_pigment_color;
                        meshRendererforYellowPigment2.material.color = yellow_pigment_color;
                        meshRendererforYellowPigment3.material.color = yellow_pigment_color;
                    }

                    yellow_Pigment_Grid[(i, j)] = newObj_Odd_Even1;
                    yellow_Pigment_Grid[(i - 2, j + 1)] = newObj_Odd_Even2;
                    yellow_Pigment_Grid[(i + 2, j + 1)] = newObj_Odd_Even3;
                }
            }
        }

        for (int i = -3; i < grid_i_count; i += 4)
        {
            for (int j = -1; j < grid_j_count; j += 2)
            {
                if (i - 2 > 0 && j > -1 && i + 2 < grid_i_count && j + 1 < grid_j_count)
                {
                    Vector3 Position2 = PtsMidPt(pointGrid[(i - 2, j + 1)], pointGrid[(i - 1, j)]);
                    Vector3 Position3 = PtsMidPt(pointGrid[(i + 2, j + 1)], pointGrid[(i + 1, j)]);


                    GameObject newObj_Odd_Even2 = GameObject.Instantiate(obj, Position2, Quaternion.identity);
                    GameObject newObj_Odd_Even3 = GameObject.Instantiate(obj, Position3, Quaternion.identity);

                    newObj_Odd_Even2.transform.localScale = Vector3.one * yellow_pigment_original_Scale_Factor;
                    newObj_Odd_Even3.transform.localScale = Vector3.one * yellow_pigment_original_Scale_Factor;

                    newObj_Odd_Even2.name = "Yellow_Pigment_Upper_Left" + "_" + i.ToString() + "_" + j.ToString();
                    newObj_Odd_Even3.name = "Yellow_Pigment_Upper_Right" + "_" + i.ToString() + "_" + j.ToString();


                    MeshRenderer meshRendererforYellowPigment2 = newObj_Odd_Even2.GetComponentInChildren<MeshRenderer>();
                    MeshRenderer meshRendererforYellowPigment3 = newObj_Odd_Even3.GetComponentInChildren<MeshRenderer>();

                    if (meshRendererforYellowPigment2 != null && meshRendererforYellowPigment3 != null)
                    {
                        meshRendererforYellowPigment2.material.color = yellow_pigment_color;
                        meshRendererforYellowPigment3.material.color = yellow_pigment_color;
                    }

                    yellow_Pigment_Grid[(i - 2, j + 1)] = newObj_Odd_Even2;
                    yellow_Pigment_Grid[(i + 2, j + 1)] = newObj_Odd_Even3;
                }
            }
        }

        return yellow_Pigment_Grid;
    }


    // Update is called once per frame

    public float popping_offset = 1.5f;
    public float popping_amplitude = 2f;

    //public float wave_amplitude = 0.1f;
    //public float wave_offset = 0.1f;

    public float t_for_whole_cycle = 5f;
    //public float wave_t_for_whole_cycle = 10f;
    public float popping_t_for_whole_cycle = 5f;

    //public float wave_speed = 2f;

    public float expected_pause_time = 15f;
    
    
    private PigmentGlobalBehaviorControllor pigmentGlobalBehavior = new PigmentGlobalBehaviorControllor();
    private PigmentBehavior pigment_behavior = new PigmentBehavior();

    //Parameters from TCP Server

    //Vision Module
    private Dictionary<(int i, int j), int> segmentation_factors_dict = new Dictionary<(int i, int j), int>();
    private int current_user_count = 0;

    //Pattern Control
    public float still_factor;

    enum PigmentSystemState {resting, activation, one_to_two_people, three_to_five_people, six_to_eight_people, back_to_rest}
    PigmentSystemState current_state = PigmentSystemState.resting; 

    void Update()
    {
        segmentation_factors_dict = Server_Client.segentation_factors;
        current_user_count = Server_Client.ppl_count;
        pigmentGlobalBehavior.PigmentWaveFactor_i(pigment_Base_Grid);


        switch (current_state)
        {
            case PigmentSystemState.resting:
                {
                    foreach (var black_key in black_Pigment_Grid.Keys)
                    {
                        //int realtime_activation_factor = segmentation_factors_dict[red_key];

                        black_Pigment_Grid[black_key].transform.localScale = Vector3.one * still_factor;

                    }

                    foreach (var red_key in red_Pigment_Grid.Keys)
                    {
                        //int realtime_activation_factor = segmentation_factors_dict[red_key];

                        red_Pigment_Grid[red_key].transform.localScale = Vector3.one * still_factor;

                    }

                    foreach (var yellow_key in yellow_Pigment_Grid.Keys)
                    {
                        //int realtime_activation_factor = segmentation_factors_dict[yellow_key];

                        yellow_Pigment_Grid[yellow_key].transform.localScale = Vector3.one * still_factor;

                    }

                    if (current_user_count != 0)
                    {
                        current_state = PigmentSystemState.activation;
                    }

                    break;
                }

            case PigmentSystemState.activation:
                {
                    // Individual Behavior
                    float scale_controller = pigment_behavior.ActivationScaleFactor(Time.deltaTime);

                    foreach (var key in black_Pigment_Grid.Keys)
                    {
                        float black_pigment_scale_factor = scale_controller * 1.325f;

                        black_Pigment_Grid[key].transform.localScale = Vector3.one * black_pigment_scale_factor;
                    }

                    foreach (var key in red_Pigment_Grid.Keys)
                    {
                        float red_pigment_scale_factor = scale_controller * 1.05f;

                        red_Pigment_Grid[key].transform.localScale = Vector3.one * red_pigment_scale_factor;
                    }

                    foreach (var key in yellow_Pigment_Grid.Keys)
                    {
                        float yellow_pigment_scale_factor = scale_controller * 0.95f;

                        yellow_Pigment_Grid[key].transform.localScale = Vector3.one * yellow_pigment_scale_factor;
                    }

                    if (current_user_count >= 1 & pigment_behavior.is_activation_finished == true)
                    {
                        current_state = PigmentSystemState.one_to_two_people;
                    }

                    break;
                }

            case PigmentSystemState.six_to_eight_people:
                {

                    //Individual Behavior
                    //Debug.Log(red_Pigment_Grid.Keys.Count);
                    foreach (var black_key in black_Pigment_Grid.Keys)
                    {
                        //float scalefactor = black_Pigment_Behavior[key].RandomPoppingFactor(popping_t_for_whole_cycle, popping_offset, popping_amplitude);
                        //int realtime_activation_factor = segmentation_factors_dict[black_key];
                        //Debug.Log(segmentation_factors_dict.ContainsKey(black_key));
                        //Debug.Log(realtime_activation_factor);

                        if (segmentation_factors_dict.TryGetValue(black_key, out int realtime_activation_factor))

                            if (realtime_activation_factor == 1)
                            {

                                black_Pigment_Grid[black_key].transform.localScale = Vector3.one * still_factor;

                            }
                            else if (realtime_activation_factor == 0)
                            {
                                //float realtime_scale_factor = 1f;
                                //black_Pigment_Grid[key].transform.localScale = Vector3.one * realtime_scale_factor;

                                int i = black_key.i;
                                int j = black_key.j;

                                float wave_scalefactor = pigmentGlobalBehavior.GetWaveFactor(i, j);

                                float true_wave_offset_factor = realtime_scale_factor_black - pigmentGlobalBehavior.wave_amplitude;

                                black_Pigment_Grid[(i, j)].transform.localScale = Vector3.one * (wave_scalefactor + true_wave_offset_factor);
                            }

                    }
                    //Debug.Log(red_Pigment_Grid.Keys.Count);
                    foreach (var red_key in red_Pigment_Grid.Keys)
                    {
                        //int realtime_activation_factor = segmentation_factors_dict[red_key];

                        if (segmentation_factors_dict.TryGetValue(red_key, out int realtime_activation_factor))

                            //Debug.Log(segmentation_factors_dict.ContainsKey(red_key));    

                            if (realtime_activation_factor == 1)
                            {
                                
                                red_Pigment_Grid[red_key].transform.localScale = Vector3.one * still_factor;
                            }

                            else if (realtime_activation_factor == 0)
                            {
                                //float realtime_scale_factor_black = 1f;
                                //red_Pigment_Grid[key].transform.localScale = Vector3.one * realtime_scale_factor_black;

                                int i = red_key.i;
                                int j = red_key.j;

                                float wave_scalefactor = pigmentGlobalBehavior.GetWaveFactor(i, j);

                                float true_wave_offset_factor = realtime_scale_factor_red - pigmentGlobalBehavior.wave_amplitude;

                                red_Pigment_Grid[(i, j)].transform.localScale = Vector3.one * (wave_scalefactor + true_wave_offset_factor);
                            }
                    }

                    foreach (var yellow_key in yellow_Pigment_Grid.Keys)
                    {
                        //int realtime_activation_factor = segmentation_factors_dict[yellow_key];

                        if (segmentation_factors_dict.TryGetValue(yellow_key, out int realtime_activation_factor))

                            if (realtime_activation_factor == 1)
                            {
                                yellow_Pigment_Grid[yellow_key].transform.localScale = Vector3.one * still_factor;

                            }
                            else if (realtime_activation_factor == 0)
                            {

                                int i = yellow_key.i;
                                int j = yellow_key.j;

                                float wave_scalefactor = pigmentGlobalBehavior.GetWaveFactor(i, j);

                                float true_wave_offset_factor = realtime_scale_factor_yellow - pigmentGlobalBehavior.wave_amplitude;

                                yellow_Pigment_Grid[(i, j)].transform.localScale = Vector3.one * (wave_scalefactor + true_wave_offset_factor);
                            }
                    }


                    if (current_user_count == 0)
                    {
                        current_state = PigmentSystemState.resting;
                    }

                    else if (current_user_count >= 1 && current_user_count <= 2)
                    {
                        current_state = PigmentSystemState.one_to_two_people;
                    }

                    else if (current_user_count >= 3 && current_user_count <= 5)
                    {
                        current_state = PigmentSystemState.three_to_five_people;
                    }

                    break;
                }

            case PigmentSystemState.one_to_two_people:
                {
                    foreach (var black_key in black_Pigment_Grid.Keys)
                    {

                        if (segmentation_factors_dict.TryGetValue(black_key, out int realtime_activation_factor))

                            if (realtime_activation_factor == 1)
                            {
                                //float realtime_scale_factor_black = 2.325f;
                                black_Pigment_Grid[black_key].transform.localScale = Vector3.one * still_factor;

                            }
                            else if (realtime_activation_factor == 0)
                            {
                                //float realtime_scale_factor = 1f;
                                //black_Pigment_Grid[key].transform.localScale = Vector3.one * realtime_scale_factor;

                                int i = black_key.i;
                                int j = black_key.j;

                                float wave_scalefactor = pigmentGlobalBehavior.GetWaveFactor(i, j);
                                black_Pigment_Grid[(i, j)].transform.localScale = Vector3.one * still_factor;
                            }

                    }
                    //Debug.Log(red_Pigment_Grid.Keys.Count);
                    foreach (var red_key in red_Pigment_Grid.Keys)
                    {
                        //int realtime_activation_factor = segmentation_factors_dict[red_key];

                        if (segmentation_factors_dict.TryGetValue(red_key, out int realtime_activation_factor))

                            //Debug.Log(segmentation_factors_dict.ContainsKey(red_key));    

                            if (realtime_activation_factor == 1)
                            {
                                
                                red_Pigment_Grid[red_key].transform.localScale = Vector3.one * realtime_scale_factor_red;
                            }

                            else if (realtime_activation_factor == 0)
                            {
                                //float realtime_scale_factor_black = 1f;
                                //red_Pigment_Grid[key].transform.localScale = Vector3.one * realtime_scale_factor_black;

                                int i = red_key.i;
                                int j = red_key.j;

                                float wave_scalefactor = pigmentGlobalBehavior.GetWaveFactor(i, j);
                                red_Pigment_Grid[(i, j)].transform.localScale = Vector3.one * still_factor;
                            }
                    }

                    foreach (var yellow_key in yellow_Pigment_Grid.Keys)
                    {
                        //int realtime_activation_factor = segmentation_factors_dict[yellow_key];

                        if (segmentation_factors_dict.TryGetValue(yellow_key, out int realtime_activation_factor))

                            //Debug.Log(yellow_Pigment_Grid.Keys.Count);
                            //Debug.Log(segmentation_factors_dict.ContainsKey(yellow_key));

                            if (realtime_activation_factor == 1)
                            {
                                if (current_user_count == 1)
                                {
                                    yellow_Pigment_Grid[yellow_key].transform.localScale = Vector3.one * still_factor;
                                }

                                else if (current_user_count == 2)
                                {
                                    
                                    yellow_Pigment_Grid[yellow_key].transform.localScale = Vector3.one * realtime_scale_factor_yellow;
                                }

                            }
                            else if (realtime_activation_factor == 0)
                            {
                                //float realtime_scale_factor = 1f;
                                //yellow_Pigment_Grid[key].transform.localScale = Vector3.one * realtime_scale_factor;

                                int i = yellow_key.i;
                                int j = yellow_key.j;

                                float wave_scalefactor = pigmentGlobalBehavior.GetWaveFactor(i, j);
                                yellow_Pigment_Grid[(i, j)].transform.localScale = Vector3.one * still_factor;
                            }
                    }


                    if (current_user_count == 0)
                    {
                        current_state = PigmentSystemState.resting;
                    }

                    else if (current_user_count >= 3 && current_user_count <= 5)
                    {
                        current_state = PigmentSystemState.three_to_five_people;
                    }

                    else if (current_user_count >= 6 && current_user_count <= 8)
                    {
                        current_state = PigmentSystemState.six_to_eight_people;
                    }

                    break;
                }


            case PigmentSystemState.three_to_five_people:
                {
                    foreach (var black_key in black_Pigment_Grid.Keys)
                    {

                        if (segmentation_factors_dict.TryGetValue(black_key, out int realtime_activation_factor))

                            if (realtime_activation_factor == 1)
                            {
                                if (current_user_count == 3)
                                {

                                    black_Pigment_Grid[black_key].transform.localScale = Vector3.one * realtime_scale_factor_black;
                                }

                                else if (current_user_count == 4)
                                {

                                    black_Pigment_Grid[black_key].transform.localScale = Vector3.one * realtime_scale_factor_black;
                                }

                                else if (current_user_count == 5)
                                {
                                    int i = black_key.i;
                                    int j = black_key.j;

                                    float wave_factor = pigmentGlobalBehavior.GetWaveFactor(i, j);

                                    float true_wave_offset_factor = realtime_scale_factor_black - pigmentGlobalBehavior.wave_amplitude;

                                    black_Pigment_Grid[black_key].transform.localScale = Vector3.one * (true_wave_offset_factor + wave_factor);

                                }
                            }

                            else if (realtime_activation_factor == 0)
                            {
                                //float realtime_scale_factor = 1f;
                                //black_Pigment_Grid[key].transform.localScale = Vector3.one * realtime_scale_factor;

                                int i = black_key.i;
                                int j = black_key.j;

                                black_Pigment_Grid[(i, j)].transform.localScale = Vector3.one * still_factor;
                            }

                            
                    }

                        //Debug.Log(red_Pigment_Grid.Keys.Count);
                        foreach (var red_key in red_Pigment_Grid.Keys)
                        {
                            //int realtime_activation_factor = segmentation_factors_dict[red_key];

                            if (segmentation_factors_dict.TryGetValue(red_key, out int realtime_activation_factor))

                                //Debug.Log(segmentation_factors_dict.ContainsKey(red_key));    

                                if (realtime_activation_factor == 1)
                                {
                                    if (current_user_count == 3)
                                    {
                                        //float realtime_scale_factor_red = 2.05f;
                                        red_Pigment_Grid[red_key].transform.localScale = Vector3.one * realtime_scale_factor_red;
                                    }
                                    else if (current_user_count == 4)
                                    {
                                        int i = red_key.i;
                                        int j = red_key.j;

                                        float wave_scalefactor = pigmentGlobalBehavior.GetWaveFactor(i, j);

                                        float true_wave_offset_factor = realtime_scale_factor_red - pigmentGlobalBehavior.wave_amplitude;

                                        red_Pigment_Grid[red_key].transform.localScale = Vector3.one * (wave_scalefactor + true_wave_offset_factor);
                                    }
                                    else if (current_user_count == 5)
                                    {
                                        int i = red_key.i;
                                        int j = red_key.j;

                                        float wave_scalefactor = pigmentGlobalBehavior.GetWaveFactor(i, j);

                                        float true_wave_offset_factor = realtime_scale_factor_red - pigmentGlobalBehavior.wave_amplitude;

                                        red_Pigment_Grid[red_key].transform.localScale = Vector3.one * (wave_scalefactor + true_wave_offset_factor);
                                    }
                                }

                                else if (realtime_activation_factor == 0)
                                {
                                    //float realtime_scale_factor_black = 1f;
                                    //red_Pigment_Grid[key].transform.localScale = Vector3.one * realtime_scale_factor_black;

                                    int i = red_key.i;
                                    int j = red_key.j;

                                    red_Pigment_Grid[(i, j)].transform.localScale = Vector3.one * still_factor;
                                }
                        }

                        foreach (var yellow_key in yellow_Pigment_Grid.Keys)
                        {
                            //int realtime_activation_factor = segmentation_factors_dict[yellow_key];

                            if (segmentation_factors_dict.TryGetValue(yellow_key, out int realtime_activation_factor))

                                if (realtime_activation_factor == 1)
                                {
                                    if (current_user_count == 3)
                                    {
                                        //float realtime_scale_factor_yellow = 2.95f;
                                        yellow_Pigment_Grid[yellow_key].transform.localScale = Vector3.one * realtime_scale_factor_yellow;
                                    }

                                    else if (current_user_count == 4)
                                    {
                                        int i = yellow_key.i;
                                        int j = yellow_key.j;

                                        float wave_factor = pigmentGlobalBehavior.GetWaveFactor(i, j);

                                        float true_wave_offset_factor = realtime_scale_factor_yellow - pigmentGlobalBehavior.wave_amplitude;

                                        yellow_Pigment_Grid[yellow_key].transform.localScale = Vector3.one * (wave_factor + true_wave_offset_factor);
                                    }

                                    else if (current_user_count == 5)
                                    {
                                        int i = yellow_key.i;
                                        int j = yellow_key.j;

                                        float wave_factor = pigmentGlobalBehavior.GetWaveFactor(i, j);

                                        float true_wave_offset_factor = realtime_scale_factor_yellow - pigmentGlobalBehavior.wave_amplitude;
                                        
                                        yellow_Pigment_Grid[yellow_key].transform.localScale = Vector3.one * (wave_factor + true_wave_offset_factor);
                                    }


                                }
                                else if (realtime_activation_factor == 0)
                                {
                                    //float realtime_scale_factor = 1f;
                                    //yellow_Pigment_Grid[key].transform.localScale = Vector3.one * realtime_scale_factor;

                                    int i = yellow_key.i;
                                    int j = yellow_key.j;

                                    yellow_Pigment_Grid[(i, j)].transform.localScale = Vector3.one * still_factor;
                                }
                        }

                        if (current_user_count == 0)
                        {
                            current_state = PigmentSystemState.resting;
                        }

                        if (current_user_count >= 1 && current_user_count <= 2)
                        {
                            current_state = PigmentSystemState.one_to_two_people;
                        }

                        if (current_user_count >= 6)
                        {
                            current_state = PigmentSystemState.six_to_eight_people;
                        }

                        break;
                    }
                


            case PigmentSystemState.back_to_rest:
                        {

                            break;
                        }

                    }


                }
        
    public class PigmentBehavior
    {
        public float offset = 1.25f;
        public float Amplitude = 1f;

        public float t_for_whole_cycle = 5f;
        public float popping_t_for_whole_cycle = 5f;
        private int jumping_count = 0;

        public float expected_pause_time = 15f;
        private float timer = 0f;

        // Parameters for the activation Stage
        private float activation_timer = 0f;

        private float activation_t_for_whole_cycle = 3f;
        private float expected_still_time = 1.5f;
        
        private enum RandomPoppingState {Waiting, Popping }
        RandomPoppingState current_state = RandomPoppingState.Popping;

        public float RandomPoppingFactor(float t_for_whole_cycle, float offset, float Amplititude)
        {
           timer += Time.deltaTime;
            //float random_popping_factor = 1.0f;

            switch (current_state)
            {
                case RandomPoppingState.Popping:

                    float frequence = 2f * Mathf.PI / t_for_whole_cycle;

                    float progress = frequence * timer;
                    float random_popping_factor = Amplititude * Mathf.Sin(progress) + offset;

                    if (progress >= 2 * Mathf.PI * (jumping_count + 1))
                    {
                        jumping_count++;
                    }
                    if (jumping_count >= 2)
                    {
                        jumping_count = 0;
                        timer = 0f;
                        current_state = RandomPoppingState.Waiting;
                    }
                    return random_popping_factor;

                case RandomPoppingState.Waiting:

                    if (timer >= expected_pause_time)
                    {

                        current_state = RandomPoppingState.Popping;
                        timer = 0f;
                    }
                    return 1.0f;
            
                default:
                    return 1.0f;
            }
        }
        private enum ActivationFactorState { scale_up, still, scale_down, buffer }
        
        ActivationFactorState activation_current_state = ActivationFactorState.scale_up;
        public bool is_activation_finished => activation_current_state == ActivationFactorState.buffer;

        public float ActivationScaleFactor(float global_delta_time)
        {
            activation_timer += global_delta_time;

            switch (activation_current_state) 
            {
                case ActivationFactorState.scale_up:
                    {
                        Debug.Log("scale_up");
                        float frequence = 2 * Mathf.PI / activation_t_for_whole_cycle;

                        float progress = frequence * activation_timer;
                        float activation_scale_up_factor = Amplitude * Mathf.Sin(progress) + offset;

                        if (progress >= Mathf.PI / 2)
                        {
                            activation_current_state = ActivationFactorState.still;
                            activation_timer = 0f;
                        }

                        return activation_scale_up_factor;
                    }

                case ActivationFactorState.still:
                    {
                        Debug.Log("Still"); 

                        float activation_still_factor = Amplitude * Mathf.Sin(Mathf.PI / 2) + offset;

                        if(activation_timer >= expected_still_time)
                        {
                            activation_current_state = ActivationFactorState.scale_down;
                            activation_timer = 0f;
                        }

                        return activation_still_factor;
                    }

                case ActivationFactorState.scale_down:
                    {
                        Debug.Log("Scale_Down");
                        float frequence = 2 * Mathf.PI / activation_t_for_whole_cycle;

                        float scale_down_stage_frequence_offset = Mathf.PI/2;

                        float progress = frequence * activation_timer + scale_down_stage_frequence_offset;

                        float activation_scale_up_factor = Amplitude * Mathf.Sin(progress) + offset;

                        if(progress >= Mathf.PI)
                        {
                            activation_current_state = ActivationFactorState.buffer;
                            activation_timer = 0f;
                        }

                        return activation_scale_up_factor;
                    }
                case ActivationFactorState.buffer:
                    {
                        Debug.Log("Buffer");
                        float activation_buffer_factor = Amplitude * Mathf.Sin(0) + offset;
                        return activation_buffer_factor;
                    }

                default:
                    Debug.Log("Default");
                    return Amplitude * Mathf.Sin(0) + offset;

            }
        }
    }


    public class PigmentGlobalBehaviorControllor
    {
        private float timer = 0f;
        public float wave_speed = 2f;
        public float wave_t_for_whole_cycle = 10f;
        public float wave_offset = 0f;
        public float wave_amplitude = 0.35f;

        public Dictionary<(int i, int j), float> wave_factor_i = new Dictionary<(int i, int j), float>();

        enum WaveController {Waving, Stop}
        WaveController current_wave_status = WaveController.Stop;

        public float PigmentPopFactor(float t_for_whole_cycle, float offset, float amplitude)
        {
            float x = 2f * Mathf.PI / t_for_whole_cycle;
            float pop_factor = amplitude * Mathf.Sin(Time.time * x) + offset;

            return pop_factor;
        }

        public void PigmentWaveFactor_i(Dictionary<(int i, int j), Vector3> grid_position)
        {
            //along_i_direction_waving
            Vector3 direction_reference = new Vector3(1, 0, 0);

            //along_j_direction_waving
            //Vector3 j_direction_reference = new Vector3(0, 0, 1);

            timer += Time.deltaTime;

            foreach (var key in grid_position.Keys) 
            {
                float frequence = 2f * Mathf.PI / wave_t_for_whole_cycle;

                float distance_along_i = grid_position[key].z - direction_reference.z;
                //float distance_along_j = grid_position[key].x - j_direction_reference.x;

                float progress = timer * frequence - distance_along_i / wave_speed;

                wave_factor_i[key] = wave_amplitude * Mathf.Sin(progress) + wave_offset;
            }
            
        }

        public float GetWaveFactor(int i, int j) 
        {
            if (wave_factor_i.ContainsKey((i, j)))
                return wave_factor_i[(i, j)];

            else 
                return 1.0f;
        }

    }
}

public class ColorManagement
{
    private float alpha = 1.0f;

    //Black Color
    private float r_black = 0f / 255f;
    private float g_black = 255f / 255f;
    private float b_black = 251f / 255f;

    //Red Color
    private float r_red = 117f / 255f;
    private float g_red = 137f / 255f;
    private float b_red = 243f / 255f;

    //Yellow Color
    private float r_yellow = 165f / 255f;
    private float g_yellow = 169f / 255f;
    private float b_yellow = 205f / 255f;

    public UnityEngine.Color Black_Pigment_Color()
    {
        UnityEngine.Color costomized_black_color = new UnityEngine.Color(r_black, g_black, b_black, alpha);
        return costomized_black_color;
    }

    public UnityEngine.Color Red_Pigment_Color()
    {
        UnityEngine.Color costomized_red_color = new UnityEngine.Color(r_red, g_red, b_red, alpha);
        return costomized_red_color;
    }

    public UnityEngine.Color Yellow_Pigment_Color()
    {
        UnityEngine.Color costomized_yellow_color = new UnityEngine.Color(r_yellow, g_yellow, b_yellow, alpha);
        return costomized_yellow_color;
    }
}

