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

import pythoncom
from pygrabber.dshow_graph import FilterGraph

import numpy as np
import torch
from sympy.codegen.ast import int32
from ultralytics import YOLO
import multiprocessing
from multiprocessing import queues
import cv2

##################### This file is managing CV related functions #########################

def Numpy2Dictionary(processed_segmented_img, target_dict, img_size):
    # img_shape ----- 480x640
    img = processed_segmented_img.squeeze()
    #print(img.shape)
    dictionary_benchmark = 0.2

    grid_i_count = 153
    grid_j_count = 50

    pixel_per_sample_i = int(img.shape[1] / grid_i_count)
    pixel_per_sample_j = int(img.shape[0] / grid_j_count)

    #print(f"[From Yolo Segmentation ModUle]: LiveStream Img shape = {img.shape}; Each Sampling Block contains i x j = {pixel_per_sample_i} x {pixel_per_sample_j} pixels")

    for m in range(grid_i_count):
        for n in range(grid_j_count):
            if m+1 < grid_i_count and n+1 < grid_j_count:

                i_range_start = m * pixel_per_sample_i
                i_range_end = (m+1) * pixel_per_sample_i

                j_range_start = n * pixel_per_sample_j
                j_range_end = (n+1) * pixel_per_sample_j

                #total_number_of_pixel = pixel_per_sample_i * pixel_per_sample_j

                #sampling_average = img[i_range_start: i_range_end, j_range_start: j_range_end].mean()
                sampling = img[j_range_start: j_range_end, i_range_start: i_range_end]
                sampling_average = sampling.mean()

                #print(f"sampling_average = {sampling_average}")
                #print(f"Block shape: {sampling.shape}, Average: {sampling_average}")

                if sampling_average >= dictionary_benchmark:
                    target_dict[m, n] = 1
                else:
                    target_dict[m, n] = 0

    return target_dict


def ObjMaskProcessor(segmented_results, img_size):

    human_idx = []
    raw_segmented_human = []
    segmented_human = {}

    for result_id, result in enumerate(segmented_results):
        boxes = result.boxes.cpu()
        # print(type(boxes))
        #masks = result.masks.cpu()
        # print(type(masks))

        for m in range(len(boxes)):
            obj_class = int(boxes[m].cls)

            if obj_class == torch.tensor([0]):
                human_idx.append(m)
        print(f"current result id is {result_id}", f", human index is: {human_idx}")

        if len(human_idx) == 0:
            substitution_tensor = np.zeros((1, img_size[1], img_size[0])).astype(np.int32)
            #print(substitution_tensor.shape)
            #print(substitution_tensor.dtype)

            raw_segmented_human.append(substitution_tensor)

        elif len(human_idx) > 0:

            masks = result.masks.cpu()

            # img_shape ----- height x width == 480x640
            for n in range(len(masks)):
                if n in human_idx:
                    mask = masks[n].data.to(dtype=torch.int32)
                    np_mask = mask.numpy()
                    raw_segmented_human.append(np_mask)


        #if len(human_idx) >= 1:
        base_matrix = np.zeros_like(raw_segmented_human[0])

        for mask in raw_segmented_human:
            processed_segmented_human = np.logical_or(base_matrix, mask)
            base_matrix = processed_segmented_human

        processed_segmented_human = processed_segmented_human.astype(np.int32)

        #print(f"processed_multi_human_mask_shape_is: {processed_segmented_human.shape}")
        #print(f"processed_multi_human_mask_dtype_is: {processed_segmented_human.dtype}")

        Numpy2Dictionary(processed_segmented_human, segmented_human, img_size)

        return segmented_human, len(human_idx)

            # elif len(human_idx) == 1:
            #     processed_segmented_human = raw_segmented_human[0]
            #     #np.set_printoptions(threshold=np.inf)
            #     #print(processed_segmented_human)
            #     #np.set_printoptions(threshold=1000)
            #     Numpy2Dictionary(processed_segmented_human, segmented_human, img_size)
            #
            #     return segmented_human, len(human_idx)


def YoloSegmetatation(to_process_into_json_queue:queue.Queue):

    print("P1 Yolo Segmentation Start!")

    if torch.cuda.is_available() == True:
        model = YOLO("yolov8n-seg.pt").to("cuda")
        print(f"CUDA: {torch.cuda.is_available()}")
    else:
        model = YOLO("yolov8n-seg.pt")
        print(f"[Warning] CUDA: {torch.cuda.is_available()}, CPU")

    pythoncom.CoInitialize()
    target_camera_name = 'NexiGo N60 FHD Webcam'

    target_camera_idx = SelectCamera(target_camera_name)

    cap = cv2.VideoCapture(target_camera_idx)

    img_size = []

    img_width = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))
    img_height = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))
    img_size.append(img_width)
    img_size.append(img_height)

    while True:
        success, img = cap.read()
        if success:

            #img = cv2.flip(img, 1)

            segmented_results = model(img)

            #print(segmented_results)
            processed_mask_data = ObjMaskProcessor(segmented_results, img_size)
            processed_dict = processed_mask_data[0]
            human_idx_len = processed_mask_data[1]
            packaged_message = [processed_dict, human_idx_len]
            #print(type(processed_dict))

            to_process_into_json_queue.put(packaged_message)

            annotated_img = segmented_results[0].plot()

            cv2.imshow('segmentation', annotated_img)
            cv2.waitKey(1)

        else:
            break


def SelectCamera(target_camera_name):

    graph = FilterGraph()
    input_devices = graph.get_input_devices()

    camera_index = None

    for i, name in enumerate(input_devices):
        if target_camera_name.lower() in name.lower():
            camera_index = i
            print(f"[cv2 & Yolo] Webcam named {name} connected")
            return camera_index

        if camera_index is None:
            print(f"[cv2 & Yolo] Webcam was not founded, using the default camera")
            camera_index = 0
            return camera_index



