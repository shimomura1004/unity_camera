import socket
import numpy as np

class DataReader:
    def __init__(self, hostname, port, width=640, height=480, channel=3):
        self.width = width
        self.height = height
        self.channel = channel
        self.frame_size = width * height * channel

        self.client = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.client.connect((hostname, port))
        self.data = ""

    def read_frame(self):
        while len(self.data) < self.frame_size:
            self.data += self.client.recv(1024 * 1024)
        frame = np.fromstring(self.data[:self.frame_size], dtype=np.uint8).reshape(self.height, self.width, self.channel)
        self.data = self.data[self.frame_size:]
        return frame

