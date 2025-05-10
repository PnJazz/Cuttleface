# ===========================================================================
# CuttleFace Interactive System – Python Module - v0.9
# Author: Dingbang Qi
# Version: v0.9 (May 11th, 2025)
# License: GNU General Public License v3.0 (GPLv3)
#
# This program is free software: you can redistribute it and/or modify
# it under the terms of the GNU General Public License as published by
# the Free Software Foundation, either version 3 of the License, or
# (at your option) any later version.
#
# This program is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
# GNU General Public License for more details.
#
# You should have received a copy of the GNU General Public License
# along with this program. If not, see <https://www.gnu.org/licenses/>.

# Description:
# This script is part of the Cuttleface system, an interactive system designed
# to enhance public engagement in a public exhibition setting. It provides
# real-time computer vision processing, TCP communication with Unity,
# and input logic for interaction control.
#
# For academic citation or inquiries, contact: [dq2166@columbia.edu]
# ===========================================================================

import struct
from ipaddress import ip_address
from socket import *
import multiprocessing
import threading
import queue
import json
from multiprocessing import queues

##################### This file is managing TCP Server related functions #########################

# def ClientSocket(to_unity_dict_queue:queue.Queue):
#
#     print("P2 Python IO Client Start!")
#
#     host_name = f"{gethostname()}" #192.168.0.110
#     print(f"the host name is{host_name}")
#     port_number = 1200
#
#     socket_python_to_unity = socket(family = AF_INET, type = SOCK_STREAM)
#     socket_python_to_unity.connect((host_name, port_number))
#
#     while True:
#         segmented_result_dict = to_unity_dict_queue.get(timeout = 10)
#
#         result_length = Dict2Json(segmented_result_dict)[0]
#         result_dict_json = Dict2Json(segmented_result_dict)[1]
#
#         #message = input("input something as test")
#         message = result_dict_json
#         socket_python_to_unity.send(result_length)
#         socket_python_to_unity.sendall(message)



def SegmentedDictProcessor(to_process_into_json_queue:queue.Queue, to_server_queue:queue.Queue):

    print("T2 SegmentedDictProcessor Client Start!")

    while True:
        segmented_result_dict = to_process_into_json_queue.get(timeout = 20)

        processed_dict = segmented_result_dict[0]
        raw_human_idx_list_len = segmented_result_dict[1]

        human_idx_list_len = ManualControl(true_seg_ppl_count = raw_human_idx_list_len)

        result_length = Dict2Json(message_module_type = "vision", result_dict = processed_dict, boxes_list_len = human_idx_list_len)[0]
        message_package_json = Dict2Json(message_module_type = "vision", result_dict = processed_dict, boxes_list_len = human_idx_list_len)[1]

        #message = input("input something as test")
        #message = result_dict_json
        packed_message = [result_length, message_package_json]

        to_server_queue.put(packed_message)
        # socket_python_to_unity.send(result_length)
        # socket_python_to_unity.sendall(message)

def TCPServer(to_server_queue:queue.Queue):
    print("P3 Python Based Server Start!")
    server_socket = socket(family = AF_INET, type = SOCK_STREAM)

    #host_name = "172.20.10.7"
    host_name = f"{gethostname()}"  # 192.168.0.110
    ip = gethostbyname(host_name)
    print(f"[Current IP Address]: {ip}")
    port_number = 1200

    server_socket.bind((host_name, port_number))

    server_socket.listen(2)
    print(f"currently port {port_number} is ready !")

    connection, address = server_socket.accept()
    print('check 1')
    while True:
        try:
            message_package = to_server_queue.get()

            result_length = message_package[0]
            message_package_json = message_package[1]

            # message_length_prefix = connection.recv(4)
            # length_unpacked = struct.unpack('!I', message_length_prefix)[0]

            length_unpacked = struct.unpack('!I', result_length)[0]
            print(length_unpacked)

            if len(result_length) == 4:
                #message_to_unity = connection.recv(length_unpacked)
                print('check 2')
                try:
                    connection.send(result_length)
                    connection.sendall(message_package_json)
                    #print(f"send message {result_dict_json} to unity with length prefix {length_unpacked}", flush = True)
                except Exception as e: print(f"Send failed, reason: {e}")


            else:
                print(f"length of the message is not 4 digit, currently: {length_unpacked}")

        except Exception as e: print(f"to_server_queue have some problems: {e}")

def Dict2Json(message_module_type, result_dict, boxes_list_len):

    dict_str = {f"{key[0]}_{key[1]}": value for key,value in result_dict.items()}

    people_count = boxes_list_len

    message = {"type": message_module_type, "data_payload": {"segmentation_result": dict_str, "count_people": people_count}}

    message_json = json.dumps(message)
    message_byte = message_json.encode('utf-8')

    message_byte_length = len(message_byte)
    prefix_length = struct.pack('!I', message_byte_length)


    return prefix_length, message_byte

def ManualControl(true_seg_ppl_count):

    manual_control = False
    manual_ppl_count = 1

    if manual_control == True:
        if true_seg_ppl_count == 0:
            return true_seg_ppl_count
        else:
            return manual_ppl_count

    else:
        return true_seg_ppl_count



