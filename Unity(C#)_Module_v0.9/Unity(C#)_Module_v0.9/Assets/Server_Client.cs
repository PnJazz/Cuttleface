using UnityEngine;
using System.Text;
using System.Net.Sockets;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using UnityEditor.Experimental.GraphView;
using Newtonsoft.Json.Linq;
using Unity.Collections;

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



public class Server_Client : MonoBehaviour
{

    public string serverIP = "192.168.0.110";
    //private string serverIP = "172.20.10.7";
    private int port_number = 1200;

    private Dictionary<string, object> unpacked_message = new Dictionary<string, object>();
    private Dictionary<string, int> segmented_result = new Dictionary<string, int>();

    TcpClient client;
    NetworkStream stream;

    ConcurrentQueue<Dictionary<string, object>> raw_dictionary_queue = new ConcurrentQueue<Dictionary<string, object>>();

    public int ppl_count;
    public Dictionary<(int i, int j), int> segentation_factors = new Dictionary<(int i, int j), int>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bool is_server_connected = ConnectedToServer(serverIP, port_number);

        if (is_server_connected == true)
        {
            Thread TCPRecvProcessor = new Thread(T1_Intensive_IO);
            TCPRecvProcessor.Start();
        }
        else
        {
            Debug.Log("[From Server Connecter]: Cannot Connect to Server");
        }

    }

    private void T1_Intensive_IO()
    {
        //Debug.Log("[T1_Intensive_IO]: T1 Start!");

        while (true)
        {
            MessgaeRcverAndProcessor();
            //Dictionary<string, int> message_from_tcp = MessgaeRcverAndProcessor();
            //raw_dictionary_queue.Enqueue(message_from_tcp);
        }
    }

    private bool ConnectedToServer(string serverIP, int port_number)
    {
        client = new TcpClient(serverIP, port_number);
        stream = client.GetStream();
        
        if (stream != null)
        {
            Debug.Log("[From Server Connecter]: Server Connected");
            return true;
        }
        else
        {
            return false;
        }
        
    }


    // Update is called once per frame
    void Update()
    {
        if (raw_dictionary_queue.IsEmpty == false)
        {
            raw_dictionary_queue.TryDequeue(out Dictionary<string, object> unpacked_message);
            string module_source_type = unpacked_message["type"].ToString();
            
            switch (module_source_type)
            {
                case "vision":
                    {
                        JObject raw_data_payload = (JObject)unpacked_message["data_payload"];
                       
                        Dictionary<string, int> seg_data_payload = raw_data_payload["segmentation_result"].ToObject<Dictionary<string, int>>();

                        ppl_count = raw_data_payload["count_people"].ToObject<int>();

                        Debug.Log("Current User Count" + ppl_count);

                        segmented_result = seg_data_payload;

                        DictionaryConverter(segmented_result, segentation_factors);

                        break;
                                    
                    }
            }
            
            //foreach (var key_value_pair in segentation_factors)
            //{
            //   Debug.Log($"Key: {key_value_pair.Key}, Value: {key_value_pair.Value}");
            //}

        }
        else
        {
            Debug.Log("[From Update Dequeue Process]: Currently Queue is Empty!");
        }
    }
    enum MessageReceiveStage{lengthprefix, payload};
    MessageReceiveStage current_reading_state = MessageReceiveStage.lengthprefix;
    int message_Length;

    //private Dictionary<string, int> MessgaeRcverAndProcessor()
    private void MessgaeRcverAndProcessor()
    {

        switch (current_reading_state) 
        {
            case MessageReceiveStage.lengthprefix:
                {
                    Debug.Log("Start to receive message!");
                    byte[] length_prefix = new byte[4];
                    int length_prefix_read = stream.Read(length_prefix, 0, 4);

                    if (length_prefix_read == 4)
                    {
                        message_Length = BitConverter.ToInt32(length_prefix.Reverse().ToArray(), 0);
                        current_reading_state = MessageReceiveStage.payload;
                        break;
                    }
                    else// if (length_prefix_read != 4)
                    {
                        Debug.Log("Length Prefix is not 4 digit!");
                        break;
                    }
                }

            case MessageReceiveStage.payload:
                {

                    byte[] raw_message_from_python = new byte[message_Length];
                    int total_bytes_read = 0;

                    while (total_bytes_read < message_Length)
                    {
                        int bytes_Read = stream.Read(raw_message_from_python, total_bytes_read, message_Length - total_bytes_read);

                        if (bytes_Read == 0)
                        {
                            Debug.Log("Connection closed by server");
                            break;
                        }
                        total_bytes_read += bytes_Read;
                    }

                    string message_from_python = Encoding.UTF8.GetString(raw_message_from_python, 0, message_Length);

                    Dictionary<string, object> processed_payload = DecodeJson(message_from_python);

                    current_reading_state = MessageReceiveStage.lengthprefix;
                    
                    raw_dictionary_queue.Enqueue(processed_payload);
                    break;
                    //return processed_message;
                }
        }
            
    }

    private Dictionary<string, object> DecodeJson(string json)
    {
        unpacked_message = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        //foreach (var key_value_pair in segmented_result)
        //{
        //    Debug.Log($"Key: {key_value_pair.Key}, Value: {key_value_pair.Value}");
        //}
        return unpacked_message;
    }

    private void DictionaryConverter(Dictionary<string, int> dict_need_to_process, Dictionary<(int i, int j), int> processed_factors)
    {
        if (dict_need_to_process != null)
        { 
            foreach (var key_value_pair in dict_need_to_process)
            {
                string[] parts_of_keys = key_value_pair.Key.Split('_');
                int i = int.Parse(parts_of_keys[0]);
                int j = int.Parse(parts_of_keys[1]);

                processed_factors[(i, j)] = key_value_pair.Value;
            }
        }
        else
        {
            Debug.Log("[From DictionaryConverter]: Dictionary Currently is Empty");
        }
    }
    
     
}
