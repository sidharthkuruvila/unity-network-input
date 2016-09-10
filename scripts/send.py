import socket
import time
import json
#UDP_IP = "127.0.0.1" 
UDP_IP = "localhost"
UDP_PORT = 9050
MESSAGE = "123"

print "UDP target IP:", UDP_IP
print "UDP target port:", UDP_PORT

sock = socket.socket(socket.AF_INET, # Internet
                        socket.SOCK_DGRAM) # UDP

for i in range(100):
    MESSAGE = {
        "left" : {"x":0, "y":1.55, "x":i* 0.1},
        "right": {"x":0, "y":1.55, "x":i* 0.5}
    }
    #MESSAGE = ",".join(str(i) for i in [0, 1.55, i * 0.1])
    print MESSAGE
    sock.sendto(json.dumps(MESSAGE), (UDP_IP, UDP_PORT))
    time.sleep(0.01)
