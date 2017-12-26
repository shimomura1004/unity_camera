#!/usr/bin/env python

import rospy
import argparse
import cv2

from camera_info_manager import CameraInfoManager

from std_msgs.msg import Header
from sensor_msgs.msg import Image, CameraInfo
from cv_bridge import CvBridge, CvBridgeError

from data_reader import DataReader


def unity_camera(args):
    image_pub = rospy.Publisher("/ip_raw_cam/image_raw", Image, queue_size=10)

    rate = rospy.Rate(40)

    cinfo = CameraInfoManager(cname="camera1", namespace="/ip_raw_cam")
    cinfo.loadCameraInfo()
    cinfo_pub = rospy.Publisher("/ip_raw_cam/camera_info", CameraInfo, queue_size=1)
    
    bridge = CvBridge()

    data_reader = DataReader(args['hostname'], args['port'])

    while not rospy.is_shutdown():
        frame = data_reader.read_frame()
        image_pub.publish(bridge.cv2_to_imgmsg(frame, "bgr8"))
        cinfo_pub.publish(cinfo.getCameraInfo())
        rate.sleep()

def updateArgs(arg_defaults):
    args = {}
    for name, val in arg_defaults.iteritems():
        full_name = rospy.search_param(name)
        if full_name is None:
            args[name] = val
        else:
            args[name] = rospy.get_param(full_name, val)
    return(args)

if __name__ == '__main__':
    rospy.init_node('ip_raw_cam', anonymous=True)

    arg_defaults = {
        'hostname': 'localhost',
        'port': 8080,
        'width': 640,
        'height': 480,
        'frame_id': 'camera1',
        'camera_info_url': ''
    }
    args = updateArgs(arg_defaults)

    try:
        unity_camera(args)
    except rospy.ROSInterruptException:
        pass

