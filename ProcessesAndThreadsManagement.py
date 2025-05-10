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

import queue
import threading

from SocketToUnity import SegmentedDictProcessor, TCPServer
from Yolo8Seg import YoloSegmetatation

####################### This file is a Threads terminal ##############################
######### This file works as a link between Processes and those functions ############

def P1_intensiveCalculation(to_process_into_json_queue:queue.Queue):
    T1_Segmentation = threading.Thread(target= YoloSegmetatation, args= (to_process_into_json_queue, ))

    T1_Segmentation.start()

def P2_intensiveIO(to_process_into_json_queue:queue.Queue):

    to_server_queue = queue.Queue()

    T2_SegmentedDictProcessor = threading.Thread(target= SegmentedDictProcessor, args= (to_process_into_json_queue, to_server_queue))
    T3_Server = threading.Thread(target=TCPServer, args=(to_server_queue,))

    T2_SegmentedDictProcessor.start()
    T3_Server.start()