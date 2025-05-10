# **Cuttleface**

## **An interactive system that designed to foster spontaneous engagement in a public exhibition settings.**  
Version: v0.9  
Upload Date: May 11th, 2025
Author: Dingbang Qi  
License: GPLv3

### Overview
Cuttleface is a computer vision–driven interactive system designed to foster spontaneous engagement in a public exhibition setting. Inspired by the camouflage and signaling behavior of cuttlefish, this project maps user presence and density to dynamic visual outputs.

### Features
- Real-time human segmentation using pre-trained YOLOv8.
- Dynamic visual feedback rendered in Unity.
- TCP socket communication between Python and Unity.
- State-based interaction logic for different user counts (1–6 users).
- Manual override mode for testing or any other scenarios (video recording).

### Project Structure
```plaintext
Cuttleface/
│
├── Python_Module_v0.9/
│   ├── Main.py
│   ├── Yolo8Seg.py
│   ├── SocketToUnity.py
│   ├── ProcessesAndThreadsManagement.py
│   └── requirements.txt
│    
│
└── Unity(C#)_Module_v0.9/
    ├── Assets
    ├── Packages
    └── ProjectSettings
```

### Requirements
1. Python
· Python 3.12.9+
· Dependencies are listed in requirements.txt

2. Unity
· Unity 6 (6000.0.34f1)
· Newtonsoft.Json (via NuGet or included in Packages/manifest.json)

### Setup
Step 1: Python Environment and Get Currrnt IP:
· Navigate to Python_Module_v0.9/
· Create a virtual environment
· Install dependencies from requirements.txt or via official wheel links
· Confirm access to an external webcam (update device name in code if needed)

Setp 2. Run the Project:
· Run Main.py first, several lines will be printed while the script is started, including the current IP address
· Stop the script copy the IP address, paste it into the box in the inspector in the Unity
· Run the Main.py again, after the captured view popped out, run the Unity project.

### Notes
· The system uses external webcams. The device is identified by its name string. If not found, please update the webcam name in the Yolo8Seg.py script.
· Manual override mode is available on the Python side for testing or scripted video demos.

### Contact
· For academic inquiries: [dq2166@columbia.edu]

