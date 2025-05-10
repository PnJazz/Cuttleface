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


import multiprocessing
from ProcessesAndThreadsManagement import P2_intensiveIO, P1_intensiveCalculation


#========================= This file is a Processes terminal ====================================
#=================== Please Run this File after every thing is alright ==========================


if __name__ == "__main__":

    to_process_into_json_queue = multiprocessing.Queue(maxsize = 2)

    Process1_Intensive_Calculation = multiprocessing.Process(target = P1_intensiveCalculation, args = (to_process_into_json_queue, ))
    Process2_Intensive_IO = multiprocessing.Process(target = P2_intensiveIO, args = (to_process_into_json_queue, ))

    Process1_Intensive_Calculation.start()
    Process2_Intensive_IO.start()
